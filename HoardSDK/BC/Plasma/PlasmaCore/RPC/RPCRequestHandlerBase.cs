namespace PlasmaCore.RPC
{
    /// <summary>
    /// Base RPC request handler
    /// </summary>
    public abstract class RPCRequestHandlerBase
    {
        /// <summary>
        /// RPC client
        /// </summary>
        protected IClient client;

        /// <summary>
        /// Constructs RPC request handler
        /// </summary>
        /// <param name="_client">RPC client</param>
        public RPCRequestHandlerBase(IClient _client)
        {
            client = _client;
        }
    }
}
