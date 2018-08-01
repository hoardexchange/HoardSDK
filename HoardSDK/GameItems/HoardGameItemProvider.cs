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
        private IDistributedStorageClient storageClient;

        //TODO: should we pass connector explicitly, or should provider query HoardService for available connectors?
        public HoardGameItemProvider(IDistributedStorageClient storageClient, IBackendConnector connector)
        {
            this.storageClient = storageClient;
            this.connector = connector;
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
            if (itemTypes.ContainsKey(game))
            {
                var types = itemTypes[game];
                foreach (string type in types)
                {
                    items.AddRange(connector.GetPlayerItems(player, type));
                }
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

        public void RegisterGameItemType(GameID game, string type)
        {
            if (!itemTypes.ContainsKey(game))
                itemTypes.Add(game, new List<string>());
            System.Diagnostics.Debug.Assert(!itemTypes[game].Contains(type), string.Format("Type [{0}] already registered!",type));
            itemTypes[game].Add(type);
        }
    }
}
