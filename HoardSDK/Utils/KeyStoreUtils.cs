using Nethereum.Web3.Accounts;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace Hoard.Utils
{
    /// <summary>
    /// Utilitites for key store management
    /// </summary>
    public class KeyStoreUtils
    {
        /// <summary>
        /// Iterates through all accounts in specified folder
        /// </summary>
        /// <param name="user">User whose accounts to iterate</param>
        /// <param name="accountsDir">folder with key store data</param>
        /// <param name="enumFunc">action to call for each account</param>
        public static void EnumerateAccounts(User user, string accountsDir, Action<string> enumFunc)
        {
            string hashedName = Helper.Keccak256HexHashString(user.UserName);
            string path = Path.Combine(accountsDir, hashedName);

            System.Diagnostics.Trace.TraceInformation(string.Format("Loading accounts from path: {0}", path));

            if (!Directory.Exists(path))
            {
                System.Diagnostics.Trace.TraceWarning("Not found any account files.");
                return;
            }

            var accountsFiles = Directory.GetFiles(path, "*.keystore");
            if (accountsFiles.Length == 0)
            {
                System.Diagnostics.Trace.TraceWarning("Not found any account files.");
                return;
            }

            foreach (var fullPath in accountsFiles)
            {
                string fileName = Path.GetFileName(fullPath);
                if ((fileName != null) && (fileName != System.String.Empty))
                    enumFunc(fileName);
            }
        }

        /// <summary>
        /// Loads account information for the user
        /// </summary>
        /// <param name="user">User to load account for</param>
        /// <param name="userInputProvider">Provider with user credentials</param>
        /// <param name="filename">filename of the file with account to load</param>
        /// <param name="accountsDir">folder where the key store files are stored</param>
        /// <returns>pair of public/private keys making an account</returns>
        public static Tuple<string, string> LoadAccount(User user, IUserInputProvider userInputProvider, string filename, string accountsDir)
        {
            string hashedName = Helper.Keccak256HexHashString(user.UserName);
            var accountsFiles = Directory.GetFiles(Path.Combine(accountsDir, hashedName), filename);
            if (accountsFiles.Length == 0)
                return null;
            System.Diagnostics.Trace.TraceInformation(string.Format("Loading account {0}", accountsFiles[0]), "INFO");

            string json = File.ReadAllText(accountsFiles[0]);
            var details = JObject.Parse(json);
            if (details == null)
                return null;
            string address = details["address"].Value<string>();
            string password = userInputProvider.RequestInput(user, eUserInputType.kPassword, address).Result;
            var account = Account.LoadFromKeyStore(json, password);

            return new Tuple<string, string>(account.Address, account.PrivateKey);
        }

        /// <summary>
        /// Create new account for user.
        /// </summary>
        /// <param name="user">User for which to create account</param>
        /// <param name="password">Password for encrypting the account</param>
        /// <param name="accountsDir">folder where to store key store data</param>
        /// <returns></returns>
        public static Tuple<string, string> CreateAccount(User user, string password, string accountsDir)
        {
            string hashedName = Helper.Keccak256HexHashString(user.UserName);
            string path = Path.Combine(accountsDir, hashedName);
            //generate new secure random key
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            string accountFile = CreateAccountUTCFile(ecKey, password, path);

            return new Tuple<string, string>(ecKey.GetPublicAddress(), ecKey.GetPrivateKey());
        }

        private static string CreateAccountUTCFile(Nethereum.Signer.EthECKey ecKey, string password, string path)
        {
            //Get the public address (derivied from the public key)
            var address = ecKey.GetPublicAddress();

            //Create a store service, to encrypt and save the file using the web3 standard
            var service = new Nethereum.KeyStore.KeyStoreService();
            var encryptedKey = service.EncryptAndGenerateDefaultKeyStoreAsJson(password, ecKey.GetPrivateKeyAsBytes(), address).ToLower();
            var keystoreJsonObject = JObject.Parse(encryptedKey);
            if (keystoreJsonObject == null)
                return null;
            string id = keystoreJsonObject["id"].Value<string>();
            var fileName = id + ".keystore";
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
