using System.Threading.Tasks;

namespace PlasmaCore.RPC
{
    /// <summary>
    /// RPC Client interface
    /// </summary>
    public interface IClient
    {
        /// <summary>
        /// Sends request and returns response
        /// </summary>
        /// <typeparam name="T">response type</typeparam>
        /// <param name="request">RPC request</param>
        /// <returns></returns>
        Task<T> SendRequestAsync<T>(RPCRequest request);
    }
}
