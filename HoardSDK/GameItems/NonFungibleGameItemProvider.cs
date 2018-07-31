using Hoard.BC.Contracts;
using Hoard.DistributedStorage;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.GameItems
{
    public class NonFungibleGameItemProvider : IGameItemProvider
    {
        public class Metadata : BaseGameItemMetadata
        {
            public string Checksum { get; set; }
            public string OwnerAddress { get; set; }
            public ulong ItemId { get; set; }

            public Metadata(string checksum, string ownerAddress, ulong itemID)
            {
                Checksum = checksum;
                OwnerAddress = ownerAddress;
                ItemId = itemID;
            }
        }

        protected string symbol;
        protected ERC721GameItemContract contract;
        protected IDistributedStorageClient storageClient;

        string IGameItemProvider.Symbol { get { return symbol; } }
        GameItemContract IGameItemProvider.Contract { get { return contract; } }

        NonFungibleGameItemProvider(ERC721GameItemContract contract, IDistributedStorageClient storageClient)
        {
            this.contract = contract;
            this.symbol = contract.Symbol().Result;
            this.storageClient = storageClient;
        }

        public async Task<bool> Transfer(PlayerID recipient, GameItem item)
        {
            Metadata metadata = item.Metadata as Metadata;
            if (metadata != null)
            {
                return await contract.Transfer(recipient.ID, metadata.ItemId);
            }

            return false;
        }

        public async Task<ulong> GetBalanceOf(PlayerID player)
        {
            return await contract.BalanceOf(player.ID);
        }

        public GameItem[] GetGameItems(PlayerID player)
        {
            List<GameItem> gameItems = new List<GameItem>();

            ulong balance = contract.BalanceOf(player.ID).Result;
            string globalChecksum = contract.Checksum().Result;

            ulong[] ids = contract.GetItems(player.ID, 0, balance).Result;
            foreach (ulong id in ids)
            {
                string itemChecksum = contract.GetItemChecksum(id).Result;
                Metadata metadata = new Metadata(itemChecksum, player.ID, id);

                gameItems.Add(new GameItem(symbol, metadata));
            }

            return gameItems.ToArray();
        }
        
        public void UpdateGameItemProperties(GameItem item)
        {
            // FIXME: handle unsuccessful data download
            // FIXME: add properties hardcoded into contract?

            string globalChecksum = contract.Checksum().Result;
            byte[] globalData = storageClient.DownloadBytesAsync(globalChecksum).Result;
            string globalJson = Encoding.UTF8.GetString(globalData);
            item.Properties = JsonConvert.DeserializeObject<ItemProperties>(globalJson);

            Metadata metadata = item.Metadata as Metadata;
            if (metadata != null)
            {
                byte[] localData = storageClient.DownloadBytesAsync(metadata.Checksum).Result;
                string localJson = Encoding.UTF8.GetString(localData);

                // TODO: join global and local properties
                item.Properties = JsonConvert.DeserializeObject<ItemProperties>(localJson);
            }
        }
    }
}
