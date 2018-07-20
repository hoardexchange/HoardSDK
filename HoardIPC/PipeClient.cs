using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace HoardIPC
{
    public class PipeClient
    {
        protected NamedPipeClientStream NamedPipeClient;
        byte[] MessageBuffer = new byte[PipeMessage.MessageChunkSize];
        StringBuilder MessageBuilder = new StringBuilder();

        public int Initialize()
        {
            NamedPipeClient = new NamedPipeClientStream(".", "HoardPipe", PipeDirection.InOut);
            return 0;
        }

        public void Shutdown()
        {
            NamedPipeClient.Dispose();
        }

        public void ReceiveMessage()
        {
            string messageChunk = string.Empty;
            MessageBuilder.Clear();
            MessageBuffer = Enumerable.Repeat((byte)0, MessageBuffer.Length).ToArray();
            do
            {
                NamedPipeClient.Read(MessageBuffer, 0, MessageBuffer.Length);
                messageChunk = Encoding.UTF8.GetString(MessageBuffer);
                MessageBuilder.Append(messageChunk);
            }
            while (!NamedPipeClient.IsMessageComplete);
            PipeMessage msg = JsonConvert.DeserializeObject<PipeMessage>(MessageBuilder.ToString());
            //Console.WriteLine("Customer {0} has ordered {1} {2} with delivery address {3}", order.CustomerName, order.Quantity, order.ProductName, order.Address);
        }

        public void SendMessage(PipeMessage msg)
        {
            Debug.Assert(NamedPipeClient != null);
            NamedPipeClient.Connect();
            string serialised = JsonConvert.SerializeObject(msg);
            byte[] messageBytes = Encoding.UTF8.GetBytes(serialised);
            NamedPipeClient.Write(messageBytes, 0, messageBytes.Length);
            NamedPipeClient.Close();
        }
    }
}
