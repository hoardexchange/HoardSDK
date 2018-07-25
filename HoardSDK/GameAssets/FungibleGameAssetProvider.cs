using Hoard.BC;
using Hoard.BC.Contracts;
using Hoard.DistributedStorage;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.GameAssets
{
    public class FungibleGameAssetProvider : IGameAssetProvider
    {
        public class Metadata : BaseGameAssetMetadata
        {
            public string OwnerAddress { get; set; }
            public ulong Balance { get; set; }
            public ulong TransferAmount { get; set; }

            public Metadata(string ownerAddress, ulong balance, ulong transferAmount = 0)
            {
                OwnerAddress = ownerAddress;
                Balance = balance;
                TransferAmount = transferAmount;
            }
        }

        protected string assetSymbol;
        protected ERC223GameAssetContract contract;
        protected IDistributedStorageClient storageClient;

        string IGameAssetProvider.AssetSymbol { get { return assetSymbol; } }
        GameAssetContract IGameAssetProvider.Contract { get { return contract; } }

        FungibleGameAssetProvider(ERC223GameAssetContract contract, IDistributedStorageClient storageClient)
        {
            this.contract = contract;
            this.assetSymbol = contract.Symbol().Result;
            this.storageClient = storageClient;
        }

        public async Task<bool> Transfer(BCComm bcComm, GameAsset gameAsset, string from, string to)
        {
            Metadata metadata = gameAsset.Metadata as Metadata;
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

        public async Task<GameAsset[]> GetItems(BCComm bcComm, string ownerAddress)
        {
            ulong propertiesAddress = await contract.PropertiesAddress();
            Props properties = await GetProperties(propertiesAddress);

            Metadata metadata = await FetchMetadataFromBC(ownerAddress);

            return new GameAsset[] { new GameAsset(assetSymbol, properties, metadata) };
        }

        private async Task<Props> GetProperties(ulong propertiesAddress)
        {
            // FIXME: handle unsuccessful data download
            byte[] data = await storageClient.DownloadBytesAsync(propertiesAddress);
            string jsonStr = Encoding.UTF8.GetString(data);

            Props properties = JsonConvert.DeserializeObject<Props>(jsonStr, new PropsConverter());

            // FIXME: add properties hardcoded into contract?

            return properties;
        }

        protected async Task<Metadata> FetchMetadataFromBC(string ownerAddress)
        {
            ulong balance = await contract.BalanceOf(ownerAddress);
            return new Metadata(ownerAddress, balance);
        }
    }
}
