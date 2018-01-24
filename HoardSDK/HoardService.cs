using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

using Nethereum.Web3.Accounts;

namespace Hoard
{
    public class HoardService
    {
        public GBDesc GameBackendDesc { get; private set;}

        private BC.BCComm bcComm = null;

        private GBClient client = null;

        private Dictionary<PlayerID, Account> accounts = new Dictionary<PlayerID, Account>();

        public HoardService()
        {
            
        }

        /// <summary>
        /// Connects to BC and fills missing options
        /// </summary>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<bool> Init(HoardServiceOptions options)
        {
            InitAccounts(options.AccountsDir, options.DefaultAccountPass);
            
            bcComm = new BC.BCComm(options.RpcClient, Accounts[0]);
            string connectionResponse = await bcComm.Connect();

            GBDesc gbDesc = await bcComm.GetGBDesc(options.GameID);
            if (gbDesc == null)
            {
                bool p = await bcComm.AddGame();
                gbDesc = new GBDesc();
                gbDesc.Url = options.GameBackendUrl;
                return false;
            }

            GameBackendDesc = gbDesc;

            return true;
        }

        public async Task<Item[]> RequestItemList(PlayerID id)
        {
            var list = await client.GetData<List<GBClient.AssetInfo>>("assets/" + accounts[id].Address + "/", null);
            List<Item> items = new List<Item>();
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
            return items.ToArray();
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

        public bool IsSignedIn(PlayerID id)
        {
            if (client != null)
                return client.signedPlayerID == id.ID 
                    && client.IsSessionValid();
            else
                return false;
        }

        public async Task<bool> SignIn(PlayerID id)
        {
            if (IsSignedIn(id))
                return true;
            //create hoard client
            client = new GBClient(GameBackendDesc);
            //connect to backend
            return await client.Connect(accounts[id]);
        }

        public async Task<ItemCRC[]> RequestItemsCRC(Item[] items)
        {
            throw new NotImplementedException();
        }

        public async Task<ItemData> RequestItemData(Item id)
        {
            throw new NotImplementedException();
        }

        public void InitAccounts(string path, string password) 
        {
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }

            var accountsFiles = ListAccountsUTCFiles(path);

            // if no account in accounts dir create one with default password.
            if (accountsFiles.Length == 0)
            {
                accountsFiles = new string[1];
                accountsFiles[0] = AccountCreator.CreateAccountUTCFile(password, path);
            }

            foreach(var fileName in accountsFiles)
            {
                var json = File.ReadAllText(System.IO.Path.Combine(path, fileName));

                var account = Account.LoadFromKeyStore(json, password);
                this.accounts.Add(account.Address, account);
            }
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
