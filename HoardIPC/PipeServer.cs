using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HoardIPC
{
    public class PipeServer
    {
        protected Thread ServerThread;
        protected bool IsRunning;
        protected bool IsCLosed;
        protected NamedPipeServerStream NamedPipeServer;

        private static void ServerThreadLoop(object data)
        {
            PipeServer pipeServer = (PipeServer)data;
            using (pipeServer.NamedPipeServer = new NamedPipeServerStream("HoardPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous))
            {
                byte[] messageBuffer = new byte[PipeHelper.MessageChunkSize];
                byte[] headerBuffer = new byte[PipeHelper.HeaderChunkSize];
                while (pipeServer.IsRunning)
                {
                    System.Threading.Thread.Sleep(0);
                    try
                    {
                        if (pipeServer.NamedPipeServer.IsConnected == false)
                            pipeServer.NamedPipeServer.WaitForConnection();
                        messageBuffer = Enumerable.Repeat((byte)0, messageBuffer.Length).ToArray();
                        headerBuffer = Enumerable.Repeat((byte)0, headerBuffer.Length).ToArray();

                        // read header
                        int readBytes = pipeServer.NamedPipeServer.Read(headerBuffer, 0, headerBuffer.Length);
                        if (readBytes > 0)
                        {
                            PipeHeader header = new PipeHeader();
                            header.Desserialize(headerBuffer);
                            Debug.Assert(header.msgSize <= PipeHelper.MessageChunkSize);
                            readBytes = pipeServer.NamedPipeServer.Read(messageBuffer, 0, header.msgSize);

                            switch(header.msgId)
                            {
                                case Messages.MSG_REGISTER_GAME:
                                    {
                                        RegisterGame msg = new RegisterGame();
                                        msg.Desserialize(messageBuffer);
                                    }
                                    break;
                                case Messages.MSG_UNREGISTER_GAME:
                                    {
                                        UnregisterGame msg = new UnregisterGame();
                                        msg.Desserialize(messageBuffer);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                        //
                    }
                    catch (IOException)
                    {
                    }
                }
                pipeServer.IsCLosed = true;
            }
        }

        public bool Initialize()
        {
            IsRunning = false;
            IsCLosed = true;
            ServerThread = new Thread(ServerThreadLoop);
            if (ServerThread != null)
            {
                IsRunning = true;
                IsCLosed = false;
                ServerThread.Start(this);
                ServerThread.IsBackground = true;
            }
            return IsRunning;
        }

        public void Shutdown()
        {
            IsRunning = false;
            NamedPipeServer.Close();
            while (!IsCLosed)
            {
                System.Threading.Thread.Sleep(1);
            }
            if (NamedPipeServer.IsConnected)
                NamedPipeServer.Disconnect();
            NamedPipeServer.Dispose();
            ServerThread = null;
        }
    }
}
