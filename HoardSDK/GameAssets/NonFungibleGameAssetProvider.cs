using Hoard.BC;
using Hoard.BC.Contracts;
using Hoard.DistributedStorage;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.GameAssets
{
    public class NonFungibleGameAssetProvider : IGameAssetProvider
    {
        public class Metadata : BaseGameAssetMetadata
        {
            public string OwnerAddress { get; set; }
            public ulong AssetID { get; set; }

            public Metadata(string ownerAddress, ulong assetID)
            {
                OwnerAddress = ownerAddress;
                AssetID = assetID;
            }
        }

        protected string assetSymbol;
        protected ERC721GameAssetContract contract;
        protected IDistributedStorageClient storageClient;

        string IGameAssetProvider.AssetSymbol { get { return assetSymbol; } }
        GameAssetContract IGameAssetProvider.Contract { get { return contract; } }

        NonFungibleGameAssetProvider(ERC721GameAssetContract contract, IDistributedStorageClient storageClient)
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
                return await contract.Transfer(from, to, metadata.AssetID);
            }

            return false;
        }

        public async Task<ulong> GetBalanceOf(string ownerAddress)
        {
            return await contract.BalanceOf(ownerAddress);
        }

        public async Task<GameAsset[]> GetItems(BCComm bcComm, string ownerAddress)
        {
            List<GameAsset> gameAssets = new List<GameAsset>();

            ulong balance = await contract.BalanceOf(ownerAddress);
            ulong globalPropertiesAddress = await contract.PropertiesAddress();
            Props globalProperties = await GetProperties(globalPropertiesAddress);

            ulong[] ids = await contract.GetItems(ownerAddress, 0, balance);
            foreach (ulong id in ids)
            {
                Metadata metadata = new Metadata(ownerAddress, id);

                ulong itemPropertiesAddress = await contract.GetItemPropertyAddress(id);
                Props itemProperties = await GetProperties(itemPropertiesAddress);

                // TODO: join global and local properties
                Props properties = null;

                gameAssets.Add(new GameAsset(assetSymbol, properties, metadata));
            }

            return gameAssets.ToArray();
        }
        
        protected async Task<Props> GetProperties(ulong propertiesAddress)
        {
            // FIXME: handle unsuccessful data download
            byte[] data = await storageClient.DownloadBytesAsync(propertiesAddress);
            string jsonStr = Encoding.UTF8.GetString(data);

            Props properties = JsonConvert.DeserializeObject<Props>(jsonStr, new PropsConverter());

            // FIXME: add properties hardcoded into contract?

            return properties;
        }
    }
}
