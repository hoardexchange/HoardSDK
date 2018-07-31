using System;
using System.Threading.Tasks;

namespace Hoard.DistributedStorage
{
    public interface IDistributedStorageClient
    {
        // FIXME: maybe it should use string instead of ulong?
        Task<byte[]> DownloadBytesAsync(string address);

        Task<string> UploadAsync(byte[] data);
    }
}
