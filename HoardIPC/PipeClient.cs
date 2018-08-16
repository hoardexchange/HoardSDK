using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;

namespace HoardIPC
{
    public class PipeClient
    {
        protected NamedPipeClientStream NamedPipeClient;
        byte[] MessageBuffer = new byte[PipeHelper.MessageChunkSize];
        StringBuilder MessageBuilder = new StringBuilder();

        public int Initialize()
        {
            NamedPipeClient = new NamedPipeClientStream(".", "HoardPipe", PipeDirection.InOut);
            NamedPipeClient.Connect();
            return 0;
        }

        public void Shutdown()
        {
            NamedPipeClient.Close();
            NamedPipeClient.Dispose();
            NamedPipeClient = null;
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
            //PipeMessage msg = JsonConvert.DeserializeObject<PipeMessage>(MessageBuilder.ToString());
            //Console.WriteLine("Customer {0} has ordered {1} {2} with delivery address {3}", order.CustomerName, order.Quantity, order.ProductName, order.Address);
        }

        public void SendMessage(PipeMessage msg, Messages msgId)
        {
            Debug.Assert(NamedPipeClient != null);
            byte[] msgBytes = msg.Serialize();

            PipeHeader header = new PipeHeader
            {
                msgSize = msgBytes.Length,
                msgId = msgId
            };
            byte[] headerBytes = header.Serialize();
            NamedPipeClient.Write(headerBytes, 0, headerBytes.Length);            
            NamedPipeClient.Write(msgBytes, 0, msgBytes.Length);

            //string serialised = JsonConvert.SerializeObject(msg);
            //byte[] messageBytes = Encoding.UTF8.GetBytes(serialised);

            //PipeHeader header = new PipeHeader();
            //header.msgSize = messageBytes.Length;
            //string serialisedHeader = JsonConvert.SerializeObject(header);
            //byte[] headerBytes = Encoding.UTF8.GetBytes(serialisedHeader);

            //NamedPipeClient.Write(headerBytes, 0, headerBytes.Length);
            //NamedPipeClient.Write(messageBytes, 0, messageBytes.Length);
            NamedPipeClient.Flush();
        }
    }
}
