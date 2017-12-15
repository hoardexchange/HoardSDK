using Nethereum.ABI.Encoders;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.KeyStore;
using Nethereum.Signer;
using Nethereum.Web3;

namespace Hoard
{

    public class Account 
    {
        private string utcFilePath = "";
        private Nethereum.Web3.Account unlockedAccount = null;

        static Create(string password, string path)
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

            return new Account(fileName);
        }

        public Account(string utcFilePath)
        {
            this.utcFilePath = utcFilePath;
        }

        public boolean Unlock(string password)
        {
            if(unlockedAccount == null)
            {
                try
                {
                    unlockedAccount = Nethereum.Web3.Account.LoadFromKeyStoreFile(utcFilePath, password);
                }
                catch (Exception ex)
                {
                    Console.Write("Cannot unlock account");
                }
            }

            return unlockedAccount != null;
        }
    }
}
