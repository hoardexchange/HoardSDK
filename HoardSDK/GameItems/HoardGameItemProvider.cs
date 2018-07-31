using Hoard.BackendConnectors;
using Hoard.BC.Contracts;
using Hoard.DistributedStorage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.GameItems
{
    /// <summary>
    /// Default Hoard Platform GameItemProvider supports all GameItems complying to Hoard standards
    /// </summary>
    public class HoardGameItemProvider : IGameItemProvider
    {
        private Dictionary<GameID, List<string>> itemTypes = new Dictionary<GameID, List<string>>();
        private IBackendConnector connector = null;

        protected IDistributedStorageClient storageClient;

        public HoardGameItemProvider(IDistributedStorageClient storageClient)
        {
            this.storageClient = storageClient;
        }

        public bool Supports(string typeName)
        {
            foreach(var list in itemTypes)
            {
                if (list.Value.Contains(typeName))
                    return true;
            }
            return false;
        }

        public GameItem[] GetGameItems(PlayerID player, GameID game)
        {
            List<GameItem> items = new List<GameItem>();
            var types = itemTypes[game];
            foreach (string type in types)
            {
                items.AddRange(connector.GetPlayerItems(player, type));
            }
            return items.ToArray();
        }

        public void UpdateGameItemProperties(GameItem item)
        {
            // FIXME: handle unsuccessful data download
            // FIXME: add properties hardcoded into contract?

            byte[] globalData = storageClient.DownloadBytesAsync(item.Checksum).Result;
            string globalJson = Encoding.UTF8.GetString(globalData);

            item.Properties = JsonConvert.DeserializeObject<ItemProperties>(globalJson);
        }

        public async Task<bool> Transfer(PlayerID recipient, GameItem item)
        {
            return await connector.Transfer(recipient.ID, item);
        }

        //public async Task<ulong> GetBalanceOf(PlayerID player)
        //{
        //    return await connector.(player.ID);
        //}
    }
}
