using System.Threading.Tasks;

namespace PlasmaCore.RPC
{
    public interface IClient
    {
        Task<T> SendRequestAsync<T>(RpcRequest request);
    }
}
