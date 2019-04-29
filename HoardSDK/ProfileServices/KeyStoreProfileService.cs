using Hoard.Utils;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Account service working with locally stored keys (on the device).
    /// </summary>
    public class KeyStoreProfileService : IProfileService
    {
        /// <summary>
        /// Implementation of AccountInfo that has a direct access to account private key.
        /// </summary>
        private class KeyStoreProfile : Profile
        {
            public string PrivateKey = null;

            /// <summary>
            /// Creates a new KeyStoreAccount.
            /// </summary>
            /// <param name="name">Name of account</param>
            /// <param name="id">identifier (public address)</param>
            /// <param name="key">private key</param>
            public KeyStoreProfile(string name, HoardID id, string key)
                :base(name, id)
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
                return KeyStoreProfileService.SignTransaction(input,PrivateKey);
            }

            /// <summary>
            /// Sign message with the private key
            /// </summary>
            /// <param name="input">input arguments</param>
            /// <returns>signed message string</returns>
            public override Task<string> SignMessage(byte[] input)
            {
                return KeyStoreProfileService.SignMessage(input, PrivateKey);
            }
        }

        private readonly string ProfilesDir = null;
        private readonly IUserInputProvider UserInputProvider = null;

        /// <summary>
        /// Creates new account service instance
        /// </summary>
        /// <param name="userInputProvider">input provider</param>
        /// <param name="profilesDir">folder path where keystore files are stored</param>
        public KeyStoreProfileService(IUserInputProvider userInputProvider, string profilesDir = null)
        {
            UserInputProvider = userInputProvider;
            if (profilesDir != null)
                ProfilesDir = profilesDir;
            else
                ProfilesDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Hoard", "profiles");
        }

        /// <summary>
        /// Helper function to create Profile object based on privateKey
        /// </summary>
        /// <param name="name">name of profile</param>
        /// <param name="privateKey">private key of account</param>
        /// <returns></returns>
        public static Profile CreateProfileDirect(string name, string privateKey)
        {
            ErrorCallbackProvider.ReportInfo("Generating user account.");
            var ecKey = new Nethereum.Signer.EthECKey(privateKey);
            Profile profile = new KeyStoreProfile(name, new HoardID(ecKey.GetPublicAddress()), privateKey);
            return profile;
        }

        /// <summary>
        /// Creates new profile with given name
        /// </summary>
        /// <param name="name">name of profile </param>
        /// <returns>new account</returns>
        public async Task<Profile> CreateProfile(string name)
        {
            ErrorCallbackProvider.ReportInfo("Generating user account.");
            string password = await UserInputProvider.RequestInput(name, null, eUserInputType.kPassword, "new password");
            Tuple<string, string> accountTuple = KeyStoreUtils.CreateProfile(name, password, ProfilesDir);
            Profile profile = new KeyStoreProfile(name, new HoardID(accountTuple.Item1), accountTuple.Item2);
            return profile;
        }

        /// <summary>
        /// Deletes profile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="checkPassword"></param>
        /// <returns></returns>
        public async Task<bool> DeleteProfile(HoardID id, bool checkPassword = false)
        {
            return await KeyStoreUtils.DeleteProfile(UserInputProvider, id, ProfilesDir, checkPassword);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public Task<string> ChangePassword(HoardID id, string oldPassword, string newPassword)
        {
            return Task.FromResult<string>(KeyStoreUtils.ChangePassword(id, oldPassword, newPassword, ProfilesDir));
        }

        /// <summary>
        /// Retrieves profile stored in profile folder for particular user name.
        /// Found accounts will be stored in User object.
        /// </summary>
        /// <param name="addressOrName">user address or name to retrieve profile for</param>
        /// <returns>true if at least one profile has been properly loaded</returns>
        public async Task<Profile> RequestProfile(string addressOrName)
        {
            ErrorCallbackProvider.ReportInfo("Requesting user account.");
            KeyStoreUtils.ProfileDesc accountDesc = await KeyStoreUtils.RequestProfile(UserInputProvider, addressOrName, ProfilesDir);
            if (accountDesc != null)
            {
                return new KeyStoreProfile(accountDesc.Name, new HoardID(accountDesc.Address), accountDesc.PrivKey);
            }
            return null;
        }

        /// <summary>
        /// Saves account to selected directory.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="profilesDir"></param>
        /// <param name="profileData"></param>
        /// <returns></returns>
        public static Task<bool> SaveProfile(string userName, string profilesDir, string profileData)
        {
            var accountJsonObject = JObject.Parse(profileData);
            if (accountJsonObject == null)
            {
                return Task.FromResult<bool>(false);
            }

            string id = accountJsonObject["id"].Value<string>();
            var fileName = id + ".keystore";

            //save the File
            using (var newfile = File.CreateText(Path.Combine(profilesDir, fileName)))
            {
                newfile.Write(profileData);
                newfile.Flush();
            }

            return Task.FromResult<bool>(true);
        }

        /// <summary>
        /// Sings a message with given account
        /// </summary>
        /// <param name="input">message input arguments</param>
        /// <param name="profile">signing profile</param>
        /// <returns></returns>
        public Task<string> SignMessage(byte[] input, Profile profile)
        {
            KeyStoreProfile ksa = profile as KeyStoreProfile;
            if (ksa == null)
            {
                ErrorCallbackProvider.ReportError("Invalid signature!");
                return null;
            }

            return SignMessage(input, ksa.PrivateKey);
        }

        /// <summary>
        /// Handy function to sign any transaction with given profile
        /// </summary>
        /// <param name="rlpEncodedTransaction">transaction input encoded in RLP format</param>
        /// <param name="profile">profile that signs transaction</param>
        /// <returns></returns>
        public Task<string> SignTransaction(byte[] rlpEncodedTransaction, Profile profile)
        {
            KeyStoreProfile ksa = profile as KeyStoreProfile;
            if (ksa == null)
            {
                ErrorCallbackProvider.ReportError("Invalid signature!");
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
            var signer = new Nethereum.Signer.EthereumMessageSigner();
            var ecKey = new Nethereum.Signer.EthECKey(privKey);
            return Task.FromResult(signer.Sign(input, ecKey));
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
            var ecKey = new Nethereum.Signer.EthECKey(privKey);

            var rawHash = new Sha3Keccack().CalculateHash(rlpEncodedTransaction);
            var signature = ecKey.SignAndCalculateV(rawHash);

            var encodedData = new List<byte[]>();
            encodedData.Add(rlpEncodedTransaction);
            encodedData.Add(RLP.EncodeElement(signature.V));
            encodedData.Add(RLP.EncodeElement(signature.R));
            encodedData.Add(RLP.EncodeElement(signature.S));

            return Task.FromResult(RLP.EncodeList(encodedData.ToArray()).ToHex().EnsureHexPrefix());
        }
    }
}
