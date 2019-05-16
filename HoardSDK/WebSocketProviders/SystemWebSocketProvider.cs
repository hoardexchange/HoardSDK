using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Default web socket provider
    /// </summary>
    public class SystemWebSocketProvider : IWebSocketProvider
    {
        private ClientWebSocket WhisperClient = null;
        private string Url = "";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="url"></param>
        public SystemWebSocketProvider(string url)
        {
            Url = url;
            WhisperClient = new ClientWebSocket();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> Connect(CancellationToken token)
        {
            try
            {
                await WhisperClient.ConnectAsync(new Uri(Url), token);
            }
            catch (TimeoutException)
            {
                ErrorCallbackProvider.ReportError("Connection timedout!");
                return false;
            }

            if (WhisperClient.State != WebSocketState.Open)
            {
                ErrorCallbackProvider.ReportError("Cannot connect to destination host: " + Url);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task Close()
        {
            if (WhisperClient.State == WebSocketState.Open)
                await WhisperClient.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsConnectionOpen()
        {
            return (WhisperClient?.State == WebSocketState.Open);
        }

        private async Task<WebSocketReceiveResult> InternalReceiveAsync(System.IO.Stream stream, CancellationToken ctok)
        {
            WebSocketReceiveResult rcvResult = null;
            do
            {
                var rcvBytes = new byte[1024];
                var rcvBuffer = new ArraySegment<byte>(rcvBytes);

                rcvResult = await WhisperClient.ReceiveAsync(rcvBuffer, ctok);

                stream.Write(rcvBuffer.Array, rcvBuffer.Offset, rcvResult.Count);
            } while (!rcvResult.EndOfMessage);
            return rcvResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<byte[]> Receive(CancellationToken token)
        {
            //read in 1K chunks                    
            System.IO.MemoryStream msgBytes = new System.IO.MemoryStream(1024);
            WebSocketReceiveResult rcvResult = await InternalReceiveAsync(msgBytes, token);
            if (rcvResult?.MessageType == WebSocketMessageType.Text)
            {
                return msgBytes.ToArray();
            }
            else if (rcvResult?.MessageType == WebSocketMessageType.Binary)
            {
                throw new NotSupportedException("Binary data is not supported!");
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task Send(byte[] data, CancellationToken token)
        {
            await WhisperClient.SendAsync(new ArraySegment<byte>(data), WebSocketMessageType.Text, true, token);
        }
    }
}
