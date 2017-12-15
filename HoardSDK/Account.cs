using Nethereum.KeyStore;
using Nethereum.Web3.Accounts;
using System.IO;

namespace Hoard
{

    public class AccountCreator
    {
        private string utcFilePath = "";
        private Nethereum.Web3.Accounts.Account unlockedAccount = null;

        public static string CreateAccountUTCFile(string password, string path)
        {
            //Generate a private key pair using SecureRandom
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            //Get the public address (derivied from the public key)
            var address = ecKey.GetPublicAddress();

            //Create a store service, to encrypt and save the file using the web3 standard
            var service = new KeyStoreService();
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

        //public Account(string utcFilePath)
        //{
        //    this.utcFilePath = utcFilePath;
        //}

        //public bool Unlock(string password)
        //{
        //    if(unlockedAccount == null)
        //    {
        //        try
        //        {
        //            var json = File.ReadAllText(utcFilePath);
        //            unlockedAccount = Nethereum.Web3.Accounts.Account.LoadFromKeyStore(json, password);
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.Write("Cannot unlock account: " + ex.ToString());
        //        }
        //    }

        //    return unlockedAccount != null;
        //}
    }
}
