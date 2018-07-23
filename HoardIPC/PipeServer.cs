using Newtonsoft.Json;
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
        protected NamedPipeServerStream namedPipeServer;

        private void WaitForConnectionCallBack(IAsyncResult iar)
        {
            try
            {
                // Get the pipe
                NamedPipeServerStream pipeServer = (NamedPipeServerStream)iar.AsyncState;

                // End waiting for the connection
                pipeServer.EndWaitForConnection(iar);

                // Recursively wait for the connection again and again....
                pipeServer.BeginWaitForConnection(new AsyncCallback(WaitForConnectionCallBack), pipeServer);
            }
            catch
            {
            }
        }

        private static async Task<bool> ConnectAsync(NamedPipeServerStream pipe)
        {
            CancellationTokenSource cts = new CancellationTokenSource(1000);
            await pipe.WaitForConnectionAsync(cts.Token);
            return pipe.IsConnected;
        }

        private static void ServerThreadLoop(object data)
        {
            PipeServer pipeServer = (PipeServer)data;
            using (pipeServer.namedPipeServer = new NamedPipeServerStream("HoardPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous))
            {
                byte[] messageBuffer = new byte[PipeMessage.MessageChunkSize];
                StringBuilder messageBuilder = new StringBuilder();
                //pipeServer.namedPipeServer.BeginWaitForConnection(new AsyncCallback(pipeServer.WaitForConnectionCallBack), pipeServer.namedPipeServer);
                while (pipeServer.IsRunning)
                {
                    System.Threading.Thread.Sleep(0);
                    try
                    {
                        pipeServer.namedPipeServer.WaitForConnection();
                        string messageChunk = string.Empty;
                        messageBuilder.Clear();
                        messageBuffer = Enumerable.Repeat((byte)0, messageBuffer.Length).ToArray();
                        do
                        {
                            int readBytes = pipeServer.namedPipeServer.Read(messageBuffer, 0, messageBuffer.Length);
                            messageChunk = Encoding.UTF8.GetString(messageBuffer);
                            messageBuilder.Append(messageChunk);
                        }
                        while (!pipeServer.namedPipeServer.IsMessageComplete);
                        PipeMessage msg = JsonConvert.DeserializeObject<PipeMessage>(messageBuilder.ToString());
                        //Console.WriteLine("Customer {0} has ordered {1} {2} with delivery address {3}", order.CustomerName, order.Quantity, order.ProductName, order.Address);
                    }
                    catch (IOException e)
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
            namedPipeServer.Close();
            while (!IsCLosed)
            {
                System.Threading.Thread.Sleep(1);
            }
            if (namedPipeServer.IsConnected)
                namedPipeServer.Disconnect();
            namedPipeServer.Dispose();
            ServerThread = null;
        }
    }
}
