using System;
using System.Threading.Tasks;

namespace Hoard.DistributedStorage
{
    public interface IDistributedStorageClient
    {
        Task<byte[]> DownloadBytesAsync(byte[] address);

        Task<byte[]> UploadAsync(byte[] data);
    }
}
