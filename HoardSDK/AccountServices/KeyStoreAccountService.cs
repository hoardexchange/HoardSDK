using Hoard.Utils;
using System;
using System.Threading.Tasks;

namespace Hoard
{
    public class KeyStoreAccountService : IAccountService
    {
        private class KeyStoreAccount : AccountInfo
        {
            public string PrivateKey = null;

            public KeyStoreAccount(string name, string id, string key, KeyStoreAccountService service)
                :base(name,id,service)
            {
                PrivateKey = key;
            }
        }

        private HoardServiceOptions Options;

        public KeyStoreAccountService(HoardServiceOptions options)
        {
            Options = options;
        }

        public async Task<AccountInfo> CreateAccount(string name, User user)
        {
            System.Diagnostics.Trace.TraceInformation("Generating user account.");

            string password = await Options.UserInputProvider.RequestInput(user, eUserInputType.kPassword, "new password");

            Tuple<string, string> accountTuple = KeyStoreUtils.CreateAccount(user, name, password);

            AccountInfo accountInfo = new KeyStoreAccount(name, accountTuple.Item1, accountTuple.Item2, this);
            user.Accounts.Add(accountInfo);

            return accountInfo;
        }

        public Task<bool> RequestAccounts(User user)
        {
            var task = new Task<bool>(() =>
            {
                KeyStoreUtils.EnumerateAccounts(user.UserName, (string accountId) =>
                 {
                     string password = Options.UserInputProvider.RequestInput(user, eUserInputType.kPassword, accountId).Result;
                     Tuple<string, string> accountTuple = KeyStoreUtils.LoadAccount(user.UserName, accountId, password);

                     if (accountTuple != null)
                     {
                         AccountInfo accountInfo = new KeyStoreAccount(accountId, accountTuple.Item1, accountTuple.Item2, this);
                         user.Accounts.Add(accountInfo);
                     }
                 });
                return user.Accounts.Count > 0;
            });
            task.Start();
            return task;
        }

        public Task<string> Sign(byte[] input, AccountInfo signature)
        {
            KeyStoreAccount ksa = signature as KeyStoreAccount;
            if (ksa == null)
            {
                System.Diagnostics.Trace.Fail("Invalid signature!");
                return null;
            }

            var task =  new Task<string>(() =>
            {
                var signer = new Nethereum.Signer.EthereumMessageSigner();
                var ecKey = new Nethereum.Signer.EthECKey(ksa.PrivateKey);
                return signer.Sign(input, ecKey);
            });
            task.Start();
            return task;
        }
    }
}
