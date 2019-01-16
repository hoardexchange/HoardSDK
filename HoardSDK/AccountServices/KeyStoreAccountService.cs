using Hoard.Utils;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Account service working with locally stored keys (on the device).
    /// </summary>
    public class KeyStoreAccountService : IAccountService
    {
        /// <summary>
        /// Implementation of AccountInfo that has a direct access to account private key.
        /// </summary>
        private class KeyStoreAccount : AccountInfo
        {
            public string PrivateKey = null;

            /// <summary>
            /// Creates a new KeyStoreAccount.
            /// </summary>
            /// <param name="name">Name of account</param>
            /// <param name="id">identifier (public address)</param>
            /// <param name="user">owner of account</param>
            /// <param name="key">private key</param>
            public KeyStoreAccount(string name, HoardID id, string key, User user)
                :base(name, id, user)
            {
                PrivateKey = key;
            }

            /// <summary>
            /// Sign transaction with the private key
            /// </summary>
            /// <param name="input">input arguments</param>
            /// <returns>signed transaction string</returns>
            public override Task<string> SignTransaction(byte[] input)
            {
                return KeyStoreAccountService.SignTransaction(input,PrivateKey);
            }

            /// <summary>
            /// Sign message with the private key
            /// </summary>
            /// <param name="input">input arguments</param>
            /// <returns>signed message string</returns>
            public override Task<string> SignMessage(byte[] input)
            {
                return KeyStoreAccountService.SignMessage(input, PrivateKey);
            }

            /// <summary>
            /// Activates account (currently not used)
            /// </summary>
            /// <returns></returns>
            public override Task<AccountInfo> Activate()
            {
                return KeyStoreAccountService.ActivateAccount(this);
            }
        }

        private readonly string AccountsDir = null;
        private readonly IUserInputProvider UserInputProvider = null;

        /// <summary>
        /// Creates new account service instance
        /// </summary>
        /// <param name="userInputProvider">input provider</param>
        /// <param name="accountsDir">folder path where keystore files are stored</param>
        public KeyStoreAccountService(IUserInputProvider userInputProvider, string accountsDir = null)
        {
            UserInputProvider = userInputProvider;
            if (accountsDir != null)
                AccountsDir = accountsDir;
            else
                AccountsDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Hoard", "accounts");
        }

        /// <summary>
        /// Helper function to create AccountInfo object based on privateKey
        /// </summary>
        /// <param name="name">name of account</param>
        /// <param name="privateKey">private key of account</param>
        /// <param name="user">owner of created account</param>
        /// <returns></returns>
        public static AccountInfo CreateAccountDirect(string name, string privateKey, User user)
        {
            var ecKey = new Nethereum.Signer.EthECKey(privateKey);

            AccountInfo accountInfo = new KeyStoreAccount(name, new HoardID(ecKey.GetPublicAddress()), privateKey, user);

            return accountInfo;
        }

        /// <summary>
        /// Creates new account for user with given name
        /// </summary>
        /// <param name="name">name of account </param>
        /// <param name="user">owner of account</param>
        /// <returns>new account</returns>
        public async Task<AccountInfo> CreateAccount(string name, User user)
        {
            System.Diagnostics.Trace.TraceInformation("Generating user account.");

            string password = await UserInputProvider.RequestInput(user, eUserInputType.kPassword, "new password");

            Tuple<string, string> accountTuple = KeyStoreUtils.CreateAccount(user, password, AccountsDir);
            
            AccountInfo accountInfo = new KeyStoreAccount(name, new HoardID(accountTuple.Item1), accountTuple.Item2, user);
            user.Accounts.Add(accountInfo);

            return accountInfo;
        }

        /// <summary>
        /// Retrieves all accounts stored in account folder for particular user.
        /// Found accounts will be stored in User object.
        /// </summary>
        /// <param name="user">user to retrieve accounts for</param>
        /// <returns>true if at least one account has been properly loaded</returns>
        public async Task<bool> RequestAccounts(User user)
        {
            return await Task.Run(() =>
            {
                user.Accounts.Clear();
                KeyStoreUtils.EnumerateAccounts(user, AccountsDir, (string filename) =>
                {
                    Tuple<string, string> accountTuple = KeyStoreUtils.LoadAccount(user, UserInputProvider, filename, AccountsDir);
                    if (accountTuple != null)
                    {
                        AccountInfo accountInfo = new KeyStoreAccount(accountTuple.Item1, new HoardID(accountTuple.Item1), accountTuple.Item2, user);
                        user.Accounts.Add(accountInfo);
                    }
                });
                return user.Accounts.Count > 0;
            });
        }

        /// <summary>
        /// Sings a message with given account
        /// </summary>
        /// <param name="input">message input arguments</param>
        /// <param name="signature">signing account</param>
        /// <returns></returns>
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

        /// <summary>
        /// Handy function to sign any transaction with given account
        /// </summary>
        /// <param name="rlpEncodedTransaction">transaction input encoded in RLP format</param>
        /// <param name="signature">account that signs transaction</param>
        /// <returns></returns>
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

        /// <summary>
        /// Handy function to sign any message with given private key
        /// </summary>
        /// <param name="input">input to sign</param>
        /// <param name="privKey">key to sign message with</param>
        /// <returns></returns>
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

        /// <summary>
        /// Handy function to sign any transaction with given private key
        /// </summary>
        /// <param name="rlpEncodedTransaction">transaction input encoded in RLP format</param>
        /// <param name="privKey">key that signs transaction</param>
        /// <returns></returns>
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

        /// <summary>
        /// Activate account for given user.
        /// </summary>
        /// <param name="account">Account to make active</param>
        /// <returns></returns>
        public static Task<AccountInfo> ActivateAccount(AccountInfo account)
        {
            Trace.Assert(account != null);
            return Task.Run(() =>
            {
                //TODO: this code is awkward
                if (account.Owner.Accounts.Contains(account))
                {
                    return account;
                }
                return null;
            });
        }
    }
}
