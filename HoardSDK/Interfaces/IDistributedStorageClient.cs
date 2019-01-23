using System;
using System.Threading.Tasks;

namespace Hoard.DistributedStorage
{
    /// <summary>
    /// Client for accessing external DB with game assets
    /// </summary>
    public interface IDistributedStorageClient
    {
        /// <summary>
        /// Download data
        /// </summary>
        /// <param name="address">hash of data (usually derived from u256 - BigInteger)</param>
        /// <returns>data as a byte array</returns>
        Task<byte[]> DownloadBytesAsync(byte[] address);

        /// <summary>
        /// Uploads data to DB (if permission is granted)
        /// </summary>
        /// <param name="data">data to upload</param>
        /// <returns>new hash of uploaded data</returns>
        Task<byte[]> UploadAsync(byte[] data);
    }
}
