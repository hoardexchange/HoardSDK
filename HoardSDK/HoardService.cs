using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Org.BouncyCastle.Math;

#if DEBUG
using System.Diagnostics;
#endif

using Nethereum.Web3.Accounts;

namespace Hoard
{
    public class HoardService
    {
        public GBDesc GameBackendDesc { get; private set;}
        public Dictionary<ulong, GameAsset> GameAssetIdDict { get; private set; } = new Dictionary<ulong, GameAsset>();
        public Dictionary<string, GameAsset> GameAssetSymbolDict { get; private set; } = new Dictionary<string, GameAsset>();
        public Dictionary<string, GameAsset> GameAssetAddressDict { get; private set; } = new Dictionary<string, GameAsset>();
        public GameExchangeService GameExchangeService { get; private set; }

        private BC.BCComm bcComm = null;
        private GBClient client = null;
        private Dictionary<PlayerID, Account> accounts = new Dictionary<PlayerID, Account>();

        public HoardService()
        {}

        public GameAsset GetGameAsset(ulong assetId)
        {
            if (GameAssetIdDict.ContainsKey(assetId))
                return GameAssetIdDict[assetId];
            else
                return null;
        }

        public GameAsset GetGameAsset(string symbol)
        {
            if (GameAssetSymbolDict.ContainsKey(symbol))
                return GameAssetSymbolDict[symbol];
            else
                return null;
        }

        public GameAsset[] GameAssets()
        {
            return GameAssetSymbolDict.Values.ToArray();
        }

        public async Task<GameAssetBalance[]> RequestGameAssetsBalanceOf(PlayerID playerId)
        {
            //iterate for all items and get balance
            List<GameAssetBalance> assetBalancesList = new List<GameAssetBalance>();

            foreach (var ga in GameAssets())
            {
                var balance = await bcComm.GetGameAssetBalanceOf(playerId.ID, ga.ContractAddress);
                assetBalancesList.Add(new GameAssetBalance(ga, balance));
            }

            return assetBalancesList.ToArray();
        }

        public async Task<ulong> RequestGameAssetBalanceOf(GameAsset asset, PlayerID id)
        {
            return await bcComm.GetGameAssetBalanceOf(id.ID, asset.ContractAddress);
        }

        public async Task<bool> RefreshGameAssets()
        {
            var gaContracts = await RequestGameAssetContracts();

            GameAssetSymbolDict.Clear();
            GameAssetIdDict.Clear();
            GameAssetAddressDict.Clear();

            ulong i = 0;
            foreach (var gac in gaContracts)
                await RegisterGameAssetContract(gac, i++);

            return true;
        }

        public bool IsSignedIn(PlayerID id)
        {
            if (client != null)
                return client.signedPlayerID.Equals(id) 
                    && client.IsSessionValid();
            else
                return false;
        }

        public bool SignIn(PlayerID id)
        {
            if (IsSignedIn(id))
                return true;
            //create hoard client
            client = new GBClient(GameBackendDesc);
            
            // Init game exchange. TODO: redesign it and make all this initialization in seprated function.
            GameExchangeService = new GameExchangeService(client, bcComm, this);
            GameExchangeService.Init(bcComm.GetContract<BC.Contracts.GameContract>(GameBackendDesc.GameContract));

            //connect to backend
            return client.Connect(accounts[id]);
        }

        // PRIVATE SECTION

        /// <summary>
        /// Connects to BC and fills missing options
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public bool Init(HoardServiceOptions options)
        {
            InitAccounts(options.AccountsDir, options.DefaultAccountPass);

            return InitGBDescriptor(options);
        }

        private async Task<object> RegisterGameAssetContract(BC.Contracts.GameAssetContract gameAssetContract, ulong assetId)
        {
            var symbol = await gameAssetContract.Symbol();

            if (!GameAssetSymbolDict.ContainsKey(symbol))
            {
                var ga = new GameAsset(
                    symbol,
                    await gameAssetContract.Name(),
                    gameAssetContract,
                    await gameAssetContract.TotalSupply(),
                    assetId,
                    await gameAssetContract.AssetType());

                GameAssetSymbolDict.Add(symbol, ga);
                GameAssetIdDict.Add(assetId, ga);
                GameAssetAddressDict.Add(ga.ContractAddress, ga);
            }
            else
            {
                throw new Exception("Truing to register same key twice");
            }

            return null;
        }

        private async Task<BC.Contracts.GameAssetContract[]> RequestGameAssetContracts()
        {
            return await bcComm.GetGameAssetContacts(GameBackendDesc.GameContract);
        }

        public List<Account> Accounts
        {
            get { return accounts.Values.ToList(); }
        }

        private bool InitGBDescriptor(HoardServiceOptions options)
        {
#if DEBUG
            Debug.WriteLine("Initializing GB descriptor.");
#endif
            bcComm = new BC.BCComm(options.RpcClient, Accounts[0]);

            GBDesc gbDesc = bcComm.GetGBDesc(options.GameID).Result;

            GameBackendDesc = gbDesc;
#if DEBUG
            Debug.WriteLine(String.Format("GB descriptor initialized.\n {0} \n {1} \n {2} \n {3}",
                gbDesc.GameContract,
                gbDesc.GameID,
                gbDesc.Name,
                gbDesc.Url
                ));
#endif

            RefreshGameAssets().Wait();

            return true;
        }

        private void InitAccounts(string path, string password) 
        {
#if DEBUG
            Debug.WriteLine(String.Format("Initializing account from path: {0}", path), "INFO");
#endif
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            var accountsFiles = ListAccountsUTCFiles(path);

            // if no account in accounts dir create one with default password.
            if (accountsFiles.Length == 0)
            {
#if DEBUG
                Debug.WriteLine("No account found. Generating one.", "INFO");
#endif
                accountsFiles = new string[1];
                accountsFiles[0] = AccountCreator.CreateAccountUTCFile(password, path);
            }

            foreach(var fileName in accountsFiles)
            {
#if DEBUG
                Debug.WriteLine(String.Format("Loading account {0}", fileName), "INFO");
#endif
                var json = File.ReadAllText(System.IO.Path.Combine(path, fileName));

                var account = Account.LoadFromKeyStore(json, password);
                this.accounts.Add(account.Address, account);
            }

#if DEBUG
            Debug.WriteLine("Accounts initialized.", "INFO");
#endif
        }

        private string[] ListAccountsUTCFiles(string path)
        {
            return Directory.GetFiles(path, "UTC--*");
        }

        public async Task<ItemCRC[]> RequestItemsCRC(GameAsset[] items)
        {
            throw new NotImplementedException();
        }

        public async Task<ItemData> RequestItemData(GameAsset id)
        {
            throw new NotImplementedException();
        }
    }
}
