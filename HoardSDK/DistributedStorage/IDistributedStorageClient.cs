using System;
using System.Threading.Tasks;

namespace Hoard.DistributedStorage
{
    public interface IDistributedStorageClient
    {
        Task<byte[]> DownloadBytesAsync(string address);

        Task<string> UploadAsync(byte[] data);
    }
}
