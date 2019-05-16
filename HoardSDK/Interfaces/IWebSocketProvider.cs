using System.Threading;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// WebSocket provider for WhisperService
    /// </summary>
    public interface IWebSocketProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<bool> Connect(CancellationToken token);

        /// <summary>
        /// 
        /// </summary>
        Task Close();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        bool IsConnectionOpen();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<byte[]> Receive(CancellationToken token);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="token"></param>
        Task Send(byte[] data, CancellationToken token); 
    }
}
