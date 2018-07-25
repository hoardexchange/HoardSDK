using System;
using System.Threading.Tasks;

namespace Hoard.DistributedStorage
{
    public interface IDistributedStorageClient
    {
        // FIXME: maybe it should use string instead of ulong?
        Task<byte[]> DownloadBytesAsync(ulong address);

        Task<ulong> UploadAsync(byte[] data);
    }
}
