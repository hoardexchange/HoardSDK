using Hoard.Utils;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    public class KeyContainerService : IAccountService
    {
        private List<AccountInfo> Accounts = new List<AccountInfo>();

        private string[] ListAccountsUTCFiles(string path)
        {
            return Directory.GetFiles(path, "UTC--*");
        }

        private bool CheckAccount(string address)
        {
            foreach (AccountInfo ainfo in Accounts)
            {
                if (ainfo.ID == address)
                {
                    return true;
                }
            }
            return false;
        }

        public bool CreateAccount(HoardServiceOptions options, string username, string password)
        {
#if DEBUG
            Debug.WriteLine("Generating user.", "INFO");
#endif
            string hashedName = Helper.SHA256HexHashString(username);
            string path = System.IO.Path.Combine(options.AccountsDir, hashedName);

            var lastUserFile = System.IO.Path.Combine(options.AccountsDir, "lastUser.txt");
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string accountFile = AccountCreator.CreateAccountUTCFile(password, path, ecKey);
            var json = File.ReadAllText(System.IO.Path.Combine(path, accountFile));
            var account = Account.LoadFromKeyStore(json, password);
            AccountInfo accountInfo = new AccountInfo(account.Address, account.PrivateKey, password, this);
            Accounts.Add(accountInfo);

            System.IO.StreamWriter file = new System.IO.StreamWriter(lastUserFile);
            file.WriteLine(username);
            file.Close();

            return true;
        }

        public bool LoadAccounts(HoardServiceOptions options, string username, string password)
        {
            string hashedName = Helper.SHA256HexHashString(username);
            string path = System.IO.Path.Combine(options.AccountsDir, hashedName);
#if DEBUG
            Debug.WriteLine(String.Format("Initializing account from path: {0}", path), "INFO");
#endif
            if (!System.IO.Directory.Exists(path))
            {
                return false;
            }

            var accountsFiles = ListAccountsUTCFiles(path);
            if (accountsFiles.Length == 0)
            {
                return false;
            }

            foreach (var fileName in accountsFiles)
            {
#if DEBUG
                Debug.WriteLine(String.Format("Loading account {0}", fileName), "INFO");
#endif
                var json = File.ReadAllText(System.IO.Path.Combine(path, fileName));

                var account = Account.LoadFromKeyStore(json, password);
                if (CheckAccount(account.Address))
                    continue;

                AccountInfo accountInfo = new AccountInfo(account.Address, account.PrivateKey, password, this);
                Accounts.Add(accountInfo);
            }

            return true;
        }

        public bool LoadAccountInplace(string address, string key, string password)
        {
            if (CheckAccount(address))
                return false;

            AccountInfo accountInfo = new AccountInfo(address, key, password, this);
            Accounts.Add(accountInfo);
            return true;
        }

        public List<AccountInfo> GetAccounts()
        {
            return Accounts;
        }
    }
}
