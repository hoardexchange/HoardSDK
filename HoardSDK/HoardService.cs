using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace Hoard
{
    public class HoardService
    {
        public GBDesc GameBackendDesc { get; private set;}
        public bool IsSingedIn { get; private set;}

        private BC.BCComm bcComm = null;
        private Exception Exception = null;

        private readonly string accountsDir = null;
        private readonly List<Account> accounts = null;

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
            bcComm = new BC.BCComm(options.BlockChainClientUrl);
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

            InitAccounts(options.AccountsDir);
            this.accountsDir = options.AccountsDir;

            return true;
        }

        public async Task<Item[]> RequestItemList()
        {
            throw new NotImplementedException();
        }

        public async Task<Item[]> RequestItemListFromBC(PlayerID playerId)
        {
            //get all item count
            ulong count = await bcComm.GetGameItemCount(GameBackendDesc.GameContract);
            //iterate for all items and get balance
            List<Item> itemList = new List<Item>();
            for(ulong i=0;i<count;++i)
            {
                //Item item = await bcComm.GetItemBalance(GameBackendDesc.GameContract, playerId);
            }
            return itemList.ToArray();
        }

        public async Task<bool> SignIn(PlayerID id)
        {
            //create hoard client
            Client client = new Client(GameBackendDesc);
            //connect to backend
            return await client.Connect(id);
        }

        public async Task<ItemCRC[]> RequestItemsCRC(Item[] items)
        {
            throw new NotImplementedException();
        }

        public async Task<ItemData> RequestItemData(Item id)
        {
            throw new NotImplementedException();
        }

        public void InitAccounts(string path) 
        {
            var accountsFiles = ListAccountsUTCFiles(path);

            if(accountsFiles)
            {
                foreach(var fileName in accountsFiles)
                {
                    this.accounts.Add(new Account(System.IO.Combile(path, fileName)))
                }
            }
        }

        public CreateNewAccount(string password)
        {
            this.accounts.Add(Account.Create(password, this.accountsDir));
        }

        public List<string> ListAccounts()
        {
            List<string> addresses = new List<string>();

            foreach(var account in this.accounts)
            {
                addresses
            }
        }

        private List<string> ListAccountsUTCFiles(string path)
        {
            FileInfo[] files = System.IO.GetFiles("UTC--*");

            if(files.Length > 0)
            {
                List<string> ret = new List<string>();

                foreach(var file in files)
                {
                    ret.Add(file.Name());
                }

                return ret;
            }
            else
            {
                return null;
            }
        }
    }
}
