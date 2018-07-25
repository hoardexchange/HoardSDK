using Hoard.BC;
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
            public string Symbol { get; set; }
            public ulong Checksum { get; set; }
            public string OwnerAddress { get; set; }
            public ulong Balance { get; set; }
            public ulong TransferAmount { get; set; }

            public Metadata(string symbol, ulong checksum, string ownerAddress, ulong balance)
            {
                Symbol = symbol;
                Checksum = checksum;
                OwnerAddress = ownerAddress;
                Balance = balance;
                TransferAmount = 0;
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

        public async Task<bool> Transfer(BCComm bcComm, GameItem gameItem, string from, string to)
        {
            Metadata metadata = gameItem.Metadata as Metadata;
            if (metadata != null)
            {
                return await contract.Transfer(from, to, metadata.TransferAmount);
            }

            return false;
        }

        public async Task<ulong> GetBalanceOf(string ownerAddress)
        {
            return await contract.BalanceOf(ownerAddress);
        }

        public GameItem[] GetGameItems(PlayerID player)
        {
            ulong balance = contract.BalanceOf(player.ID).Result;
            if (balance > 0)
            {
                ulong globalChecksum = contract.Checksum().Result;
                Metadata metadata = new Metadata(symbol, globalChecksum, player.ID, balance);

                return new GameItem[] { new GameItem(metadata) };
            }
            return new GameItem[] { };
        }

        public ItemProps GetGameItemProperties(GameItem item)
        {
            // FIXME: handle unsuccessful data download
            // FIXME: add properties hardcoded into contract?

            ulong globalChecksum = contract.Checksum().Result;
            byte[] globalData = storageClient.DownloadBytesAsync(globalChecksum).Result;
            string globalJson = Encoding.UTF8.GetString(globalData);

            return JsonConvert.DeserializeObject<ItemProps>(globalJson, new ItemPropsConverter());
        }
    }
}
