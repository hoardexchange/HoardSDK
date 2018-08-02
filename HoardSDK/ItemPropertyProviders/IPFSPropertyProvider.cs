using Hoard.DistributedStorage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.ItemPropertyProviders
{
    public class IPFSPropertyProvider : IItemPropertyProvider
    {
        private IPFSClient Client = null;

        public bool Supports(string type)
        {
            throw new NotImplementedException();
        }

        public bool UpdateGameItemProperties(GameItem item)
        {
            // FIXME: handle unsuccessful data download

            byte[] globalData = Client.DownloadBytesAsync(item.State).Result;
            string globalJson = Encoding.UTF8.GetString(globalData);

            item.Properties = JsonConvert.DeserializeObject<ItemProperties>(globalJson);

            return true;
        }
    }
}
