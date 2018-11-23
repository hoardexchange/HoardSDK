using Hoard.Utils;
using Nethereum.Web3.Accounts;
using System.IO;
using System.Threading.Tasks;

namespace Hoard
{
    public class KeyContainerService : IAccountService
    {
        private class KeyStoreAccount : AccountInfo
        {
            public string PrivateKey = null;

            public KeyStoreAccount(string name, string id, string key, KeyContainerService service)
                :base(name,id,service)
            {
                PrivateKey = key;
            }
        }

        private HoardServiceOptions Options;

        public KeyContainerService(HoardServiceOptions options)
        {
            Options = options;
        }

        private static string CreateAccountUTCFile(string password, string path, Nethereum.Signer.EthECKey ecKey)
        {
            //Get the public address (derivied from the public key)
            var address = ecKey.GetPublicAddress();

            //Create a store service, to encrypt and save the file using the web3 standard
            var service = new Nethereum.KeyStore.KeyStoreService();
            var encryptedKey = service.EncryptAndGenerateDefaultKeyStoreAsJson(password, ecKey.GetPrivateKeyAsBytes(), address);
            var fileName = service.GenerateUTCFileName(address);
            //save the File
            using (var newfile = File.CreateText(Path.Combine(path, fileName)))
            {
                newfile.Write(encryptedKey);
                newfile.Flush();
            }

            return fileName;
        }

        public async Task<bool> CreateAccount(string name, User user)
        {
            System.Diagnostics.Trace.TraceInformation("Generating user account.");

            string hashedName = Helper.SHA256HexHashString(user.UserName);
            string path = Path.Combine(Options.AccountsDir, hashedName);
            //generate new secure random key
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string password = await Options.UserInputProvider.RequestInput(user, eUserInputType.kPassword, "new password");
            string accountFile = CreateAccountUTCFile(password, path, ecKey);

            Account account = new Account(ecKey);
            AccountInfo accountInfo = new KeyStoreAccount(name, account.Address, account.PrivateKey, this);
            user.Accounts.Add(accountInfo);

            return true;
        }

        public async Task<bool> RequestAccounts(User user)
        {
            string hashedName = Helper.SHA256HexHashString(user.UserName);
            string path = Path.Combine(Options.AccountsDir, hashedName);

            System.Diagnostics.Trace.TraceInformation(string.Format("Loading accounts from path: {0}", path));

            if (!Directory.Exists(path))
            {
                System.Diagnostics.Trace.TraceWarning("Not found any account files.");
                return false;
            }

            var accountsFiles = Directory.GetFiles(path, "UTC--*");
            if (accountsFiles.Length == 0)
            {
                System.Diagnostics.Trace.TraceWarning("Not found any account files.");
                return false;
            }

            foreach (var fileName in accountsFiles)
            {
                System.Diagnostics.Trace.WriteLine(string.Format("Loading account {0}", fileName), "INFO");

                var json = File.ReadAllText(Path.Combine(path, fileName));

                string password = await Options.UserInputProvider.RequestInput(user, eUserInputType.kPassword, fileName);

                var account = Account.LoadFromKeyStore(json, password);

                AccountInfo accountInfo = new KeyStoreAccount(fileName, account.Address, account.PrivateKey, this);
                user.Accounts.Add(accountInfo);
            }

            return true;
        }

        public Task<string> Sign(byte[] input, AccountInfo signature)
        {
            KeyStoreAccount ksa = signature as KeyStoreAccount;
            if (ksa == null)
            {
                System.Diagnostics.Trace.Fail("Invalid signature!");
                return null;
            }

            return new Task<string>(() =>
            {
                var signer = new Nethereum.Signer.EthereumMessageSigner();
                var ecKey = new Nethereum.Signer.EthECKey(ksa.PrivateKey);
                return signer.Sign(input, ecKey);
            });
        }
    }
}
