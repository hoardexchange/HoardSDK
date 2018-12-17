using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.Utils
{
    public class KeyStoreUtils
    {
        public static void EnumerateAccounts(string userName, string accountsDir, Action<string> enumFunc)
        {
            string hashedName = Helper.SHA256HexHashString(userName);
            string path = Path.Combine(accountsDir, hashedName);

            System.Diagnostics.Trace.TraceInformation(string.Format("Loading accounts from path: {0}", path));

            if (!Directory.Exists(path))
            {
                System.Diagnostics.Trace.TraceWarning("Not found any account files.");
                return;
            }

            var accountsFiles = Directory.GetFiles(path, "UTC--*");
            if (accountsFiles.Length == 0)
            {
                System.Diagnostics.Trace.TraceWarning("Not found any account files.");
                return;
            }

            foreach (var fileName in accountsFiles)
            {
                string[] parts = fileName.Split(new string[]{ "--"},StringSplitOptions.None);
                if (parts.Length>3)
                    enumFunc(parts[3]);
            }
        }

        public static Tuple<string, string> LoadAccount(string userName, string accountId, string password, string accountsDir)
        {
            string hashedName = Helper.SHA256HexHashString(userName);
            var accountsFiles = Directory.GetFiles(Path.Combine(accountsDir, hashedName), "UTC--*--" + accountId);
            if (accountsFiles.Length == 0)
                return null;
            string fileName = accountsFiles[0];
            System.Diagnostics.Trace.WriteLine(string.Format("Loading account {0}", fileName), "INFO");

            string json = File.ReadAllText(fileName);

            var account = Account.LoadFromKeyStore(json, password);

            return new Tuple<string, string>(account.Address, account.PrivateKey);
        }

        public static Tuple<string, string> CreateAccount(User user, string name, string password, string accountsDir)
        {
            string hashedName = Helper.SHA256HexHashString(user.UserName);
            string path = Path.Combine(accountsDir, hashedName);
            //generate new secure random key
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string accountFile = CreateAccountUTCFile(ecKey, password, path, name);

            return new Tuple<string, string>(ecKey.GetPublicAddress(), ecKey.GetPrivateKey());
        }

        private static string CreateAccountUTCFile(Nethereum.Signer.EthECKey ecKey, string password, string path, string accountId)
        {
            //Get the public address (derivied from the public key)
            var address = ecKey.GetPublicAddress();

            //Create a store service, to encrypt and save the file using the web3 standard
            var service = new Nethereum.KeyStore.KeyStoreService();
            var encryptedKey = service.EncryptAndGenerateDefaultKeyStoreAsJson(password, ecKey.GetPrivateKeyAsBytes(), address);
            string fileName = service.GenerateUTCFileName(address) + "--" + accountId;
            //save the File
            using (var newfile = File.CreateText(Path.Combine(path, fileName)))
            {
                newfile.Write(encryptedKey);
                newfile.Flush();
            }

            return fileName;
        }
    }
}
