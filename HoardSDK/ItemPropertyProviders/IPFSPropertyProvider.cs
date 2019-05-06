using Hoard.DistributedStorage;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.ItemPropertyProviders
{
    /// <summary>
    /// Manages Item properties stored in the IPFS distributed database
    /// </summary>
    public class IPFSPropertyProvider : IItemPropertyProvider
    {
        private IPFSClient Client = null;

        /// <inheritdoc/>
        public bool Supports(string type)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task<Result> FetchGameItemProperties(GameItem item)
        {
            try
            {
                byte[] globalData = await Client.DownloadBytesAsync(item.State);
                string globalJson = Encoding.UTF8.GetString(globalData);

                item.Properties = JsonConvert.DeserializeObject<ItemProperties>(globalJson);
                return Result.Ok;
            }
            catch (Exception ex)
            {
                ErrorCallbackProvider.ReportError(ex.ToString());
            }
            return Result.Error;
        }
    }
}
