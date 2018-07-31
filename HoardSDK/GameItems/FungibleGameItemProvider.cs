using Hoard.BC.Contracts;
using Hoard.DistributedStorage;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.GameItems
{
    public class FungibleGameItemProvider : IGameItemProvider
    {
        public class Metadata : BaseGameItemMetadata
        {
            public ulong Checksum { get; set; }
            public string OwnerAddress { get; set; }
            public ulong Balance { get; set; }

            public Metadata(ulong checksum, string ownerAddress, ulong balance)
            {
                Checksum = checksum;
                OwnerAddress = ownerAddress;
                Balance = balance;
            }
        }

        protected string symbol;
        protected ERC223GameItemContract contract;
        protected IDistributedStorageClient storageClient;

        string IGameItemProvider.Symbol { get { return symbol; } }
        GameItemContract IGameItemProvider.Contract { get { return contract; } }

        FungibleGameItemProvider(ERC223GameItemContract contract, IDistributedStorageClient storageClient)
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
                return await contract.Transfer(recipient.ID, metadata.Balance);
            }

            return false;
        }

        public async Task<ulong> GetBalanceOf(PlayerID player)
        {
            return await contract.BalanceOf(player.ID);
        }

        public GameItem[] GetGameItems(PlayerID player)
        {
            ulong balance = contract.BalanceOf(player.ID).Result;
            if (balance > 0)
            {
                ulong globalChecksum = contract.Checksum().Result;
                Metadata metadata = new Metadata(globalChecksum, player.ID, balance);

                return new GameItem[] { new GameItem(symbol, metadata) };
            }
            return new GameItem[] { };
        }

        public void UpdateGameItemProperties(GameItem item)
        {
            // FIXME: handle unsuccessful data download
            // FIXME: add properties hardcoded into contract?

            ulong globalChecksum = contract.Checksum().Result;
            byte[] globalData = storageClient.DownloadBytesAsync(globalChecksum).Result;
            string globalJson = Encoding.UTF8.GetString(globalData);

            item.Properties = JsonConvert.DeserializeObject<ItemProperties>(globalJson);
        }
    }
}
