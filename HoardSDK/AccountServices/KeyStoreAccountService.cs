using Hoard.Utils;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using System;
using System.Threading.Tasks;

namespace Hoard
{
    public class KeyStoreAccountService : IAccountService
    {
        public class KeyStoreAccount : AccountInfo
        {
            public string PrivateKey = null;

            public KeyStoreAccount(string name, HoardID id, string key)
                :base(name,id)
            {
                PrivateKey = key;
            }

            public override Task<string> SignTransaction(byte[] input)
            {
                return KeyStoreAccountService.SignTransaction(input,PrivateKey);
            }

            public override Task<string> SignMessage(byte[] input)
            {
                return KeyStoreAccountService.SignMessage(input, PrivateKey);
            }

            public override Task<AccountInfo> Activate(User user)
            {
                return KeyStoreAccountService.ActivateAccount(user, this);
            }
        }

        private string AccountsDir = null;
        private IUserInputProvider UserInputProvider = null;

        public KeyStoreAccountService(IUserInputProvider userInputProvider, string accountsDir = null)
        {
            UserInputProvider = userInputProvider;
            if (accountsDir != null)
                AccountsDir = accountsDir;
            else
                AccountsDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Hoard", "accounts");
        }

        public async Task<AccountInfo> CreateAccount(string name, User user)
        {
            System.Diagnostics.Trace.TraceInformation("Generating user account.");

            string password = await UserInputProvider.RequestInput(user, eUserInputType.kPassword, "new password");

            Tuple<string, string> accountTuple = KeyStoreUtils.CreateAccount(user, name, password, AccountsDir);
            
            AccountInfo accountInfo = new KeyStoreAccount(name, new HoardID(accountTuple.Item1), accountTuple.Item2);
            user.Accounts.Add(accountInfo);

            return accountInfo;
        }

        public async Task<bool> RequestAccounts(User user)
        {
            return await Task.Run(() =>
            {
                user.Accounts.Clear();
                KeyStoreUtils.EnumerateAccounts(user.UserName, AccountsDir, (string accountId) =>
                {
                    string password = UserInputProvider.RequestInput(user, eUserInputType.kPassword, accountId).Result;
                    Tuple<string, string> accountTuple = KeyStoreUtils.LoadAccount(user.UserName, accountId, password, AccountsDir);

                    if (accountTuple != null)
                    {
                        AccountInfo accountInfo = new KeyStoreAccount(accountId, new HoardID(accountTuple.Item1), accountTuple.Item2);
                        user.Accounts.Add(accountInfo);
                    }
                });
                return user.Accounts.Count > 0;
            });
        }

        public Task<string> SignMessage(byte[] input, AccountInfo signature)
        {
            KeyStoreAccount ksa = signature as KeyStoreAccount;
            if (ksa == null)
            {
                System.Diagnostics.Trace.Fail("Invalid signature!");
                return null;
            }

            return SignMessage(input, ksa.PrivateKey);
        }

        public Task<string> SignTransaction(byte[] rlpEncodedTransaction, AccountInfo signature)
        {
            KeyStoreAccount ksa = signature as KeyStoreAccount;
            if (ksa == null)
            {
                System.Diagnostics.Trace.Fail("Invalid signature!");
                return null;
            }

            return SignTransaction(rlpEncodedTransaction, ksa.PrivateKey);
        }

        public static Task<string> SignMessage(byte[] input, string privKey)
        {
            //CPU-bound
            return Task.Run(() =>
            {
                var signer = new Nethereum.Signer.EthereumMessageSigner();
                var ecKey = new Nethereum.Signer.EthECKey(privKey);
                return signer.Sign(input, ecKey);
            });
        }

        public static Task<string> SignTransaction(byte[] rlpEncodedTransaction, string privKey)
        {
            //CPU-bound
            return Task.Run(() =>
            {
                var decodedList = RLP.Decode(rlpEncodedTransaction);
                var decodedRlpCollection = (RLPCollection)decodedList[0];
                var data = decodedRlpCollection.ToBytes();

                var ecKey = new Nethereum.Signer.EthECKey(privKey);
                var signer = new Nethereum.Signer.RLPSigner(data);

                signer.Sign(ecKey);
                return signer.GetRLPEncoded().ToHex();
            });
        }

        public static Task<AccountInfo> ActivateAccount(User user, AccountInfo account)
        {
            return Task.Run(() =>
            {
                if (user.Accounts.Contains(account))
                {
                    return account;
                }
                return null;
            });
        }
    }
}
