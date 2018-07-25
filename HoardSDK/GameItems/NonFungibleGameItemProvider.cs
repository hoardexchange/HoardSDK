using Hoard.BC;
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
            public string Symbol { get; set; }
            public ulong Checksum { get; set; }
            public string OwnerAddress { get; set; }
            public ulong ItemId { get; set; }

            public Metadata(string symbol, ulong checksum, string ownerAddress, ulong itemID)
            {
                Symbol = symbol;
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

        public async Task<bool> Transfer(BCComm bcComm, GameItem gameItem, string from, string to)
        {
            Metadata metadata = gameItem.Metadata as Metadata;
            if (metadata != null)
            {
                return await contract.Transfer(from, to, metadata.ItemId);
            }

            return false;
        }

        public async Task<ulong> GetBalanceOf(string ownerAddress)
        {
            return await contract.BalanceOf(ownerAddress);
        }

        public GameItem[] GetGameItems(PlayerID player)
        {
            List<GameItem> gameItems = new List<GameItem>();

            ulong balance = contract.BalanceOf(player.ID).Result;
            ulong globalChecksum = contract.Checksum().Result;

            ulong[] ids = contract.GetItems(player.ID, 0, balance).Result;
            foreach (ulong id in ids)
            {
                ulong itemChecksum = contract.GetItemChecksum(id).Result;
                Metadata metadata = new Metadata(symbol, itemChecksum, player.ID, id);

                gameItems.Add(new GameItem(metadata));
            }

            return gameItems.ToArray();
        }
        
        public ItemProps GetGameItemProperties(GameItem item)
        {
            // FIXME: handle unsuccessful data download
            // FIXME: add properties hardcoded into contract?
            ItemProps properties = null;

            ulong globalChecksum = contract.Checksum().Result;
            byte[] globalData = storageClient.DownloadBytesAsync(globalChecksum).Result;
            string globalJson = Encoding.UTF8.GetString(globalData);
            properties = JsonConvert.DeserializeObject<ItemProps>(globalJson, new ItemPropsConverter());

            Metadata metadata = item.Metadata as Metadata;
            if (metadata != null)
            {
                byte[] localData = storageClient.DownloadBytesAsync(metadata.Checksum).Result;
                string localJson = Encoding.UTF8.GetString(localData);
                
                // TODO: join global and local properties
                properties = JsonConvert.DeserializeObject<ItemProps>(localJson, new ItemPropsConverter());
            }

            return properties;
        }
    }
}
