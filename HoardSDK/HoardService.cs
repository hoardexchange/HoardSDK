using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

#if DEBUG
using System.Diagnostics;
#endif

using Nethereum.Web3.Accounts;

namespace Hoard
{
    public class HoardService
    {
        public GBDesc GameBackendDesc { get; private set;}

        private BC.BCComm bcComm = null;

        private GBClient client = null;

        private Dictionary<PlayerID, Account> accounts = new Dictionary<PlayerID, Account>();

        private Dictionary<string, BC.Contracts.GameCoinContract> gameCoinsContracts = new Dictionary<string, BC.Contracts.GameCoinContract>();

        public HoardService()
        {}

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

        private void RegisterGameCoinContract(string symbol, BC.Contracts.GameCoinContract gameCoinContract) // TODO: This function should be private and address shoul be taken from GameContract
        {
            gameCoinsContracts.Add(symbol, gameCoinContract);
        }

        public async Task<Item[]> RequestItemList(PlayerID id)
        {
            var list = await client.GetData<List<GBClient.AssetInfo>>("assets/" + accounts[id].Address + "/", null);
            List<Item> items = new List<Item>();

            if (list != null)
            {
                list.ForEach(asset =>
                {
                    if (asset.game_id == GameBackendDesc.GameID)
                    {
                        items.Add(new Item
                        {
                            Count = asset.amount,
                            ID = asset.asset_id,
                        });
                    }
                });
            }

            return items.ToArray();
        }

        public async Task<Coin[]> RequestGameCoinList(PlayerID id)
        {
            var gameContracts = await bcComm.GetGameCoinsContacts(GameBackendDesc.GameContract);

            List<Coin> ret = new List<Coin>();

            foreach(var gc in gameContracts)
            {
                var balance = await gc.BalanceOf(id.ID);
                ret.Add(new Coin(await gc.Symbol(), await gc.Name(), balance));
            }

            return ret.ToArray();
        }

        public async Task<Item[]> RequestItemListFromBC(PlayerID playerId)
        {
            //get all item count
            ulong count = await bcComm.GetGameItemCount(GameBackendDesc.GameContract);
            //iterate for all items and get balance
            List<Item> itemList = new List<Item>();
            for(ulong i=0;i<count;++i)
            {
                ulong itemCount = await bcComm.GetItemBalance(GameBackendDesc.GameContract, playerId, i);
                if (itemCount > 0)
                {
                    Item item = new Hoard.Item();
                    item.ID = i;
                    item.Count = itemCount;
                    itemList.Add(item);
                }
            }
            return itemList.ToArray();
        }

        private async Task<BC.Contracts.GameCoinContract[]> RequestGameCoinsContracts()
        {
            return await bcComm.GetGameCoinsContacts(GameBackendDesc.GameContract);
        }

        public async Task<ulong> RequestGameCoinsBalansOf(PlayerID playerId, string coinSymbol)
        {
            if (gameCoinsContracts.ContainsKey(coinSymbol))
                return await gameCoinsContracts[coinSymbol].BalanceOf(playerId.ID);
            else
                return 0;
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
            //connect to backend
            return client.Connect(accounts[id]);
        }

        public async Task<ItemCRC[]> RequestItemsCRC(Item[] items)
        {
            throw new NotImplementedException();
        }

        public async Task<ItemData> RequestItemData(Item id)
        {
            throw new NotImplementedException();
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
            var gcContracts = RequestGameCoinsContracts().Result;

            foreach (var gc in gcContracts)
                RegisterGameCoinContract(gc.Symbol().Result, gc);

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

        public List<Account> Accounts
        {
            get { return accounts.Values.ToList(); }
        }

        public string[] ListAccountsUTCFiles(string path)
        {
            return Directory.GetFiles(path, "UTC--*");
        }
    }
}
