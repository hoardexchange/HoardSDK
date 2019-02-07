using Hoard.DistributedStorage;
using Newtonsoft.Json;
using System;
using System.Text;

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
        public bool FetchGameItemProperties(GameItem item)
        {
            // FIXME: handle unsuccessful data download

            byte[] globalData = Client.DownloadBytesAsync(item.State).Result;
            string globalJson = Encoding.UTF8.GetString(globalData);

            item.Properties = JsonConvert.DeserializeObject<ItemProperties>(globalJson);

            return true;
        }
    }
}
