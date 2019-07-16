using Hoard.Utils;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
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
            private static readonly SecureRandom Random = new SecureRandom();

            private readonly byte[] localKey = new byte[32];
            private readonly byte[] encryptedKey = null;

            /// <summary>
            /// Creates a new KeyStoreAccount.
            /// </summary>
            /// <param name="name">Name of account</param>
            /// <param name="id">identifier (public address)</param>
            /// <param name="key">private key</param>
            public KeyStoreProfile(string name, HoardID id, byte[] key)
                :base(name, id)
            {
                Random.NextBytes(localKey);
                encryptedKey = Encode(key);
            }

            /// <summary>
            /// Sign transaction with the private key
            /// </summary>
            /// <param name="input">input arguments</param>
            /// <returns>transaction signature string</returns>
            public override Task<string> SignTransaction(byte[] input)
            {
                //CPU-bound
                var ecKey = new Nethereum.Signer.EthECKey(DecodeLocalKey(), true);
                var rawHash = new Sha3Keccack().CalculateHash(input);
                var signature = ecKey.SignAndCalculateV(rawHash);
                return Task.FromResult(Nethereum.Signer.EthECDSASignature.CreateStringSignature(signature));
            }

            /// <summary>
            /// Sign message with the private key
            /// </summary>
            /// <param name="input">input arguments</param>
            /// <returns>signed message string</returns>
            public override Task<string> SignMessage(byte[] input)
            {
                //CPU-bound
                var signer = new Nethereum.Signer.EthereumMessageSigner();
                var ecKey = new Nethereum.Signer.EthECKey(DecodeLocalKey(), true);
                return Task.FromResult(signer.Sign(input, ecKey));
            }

            private byte[] Encode(byte[] val)
            {
                // Initialize AES CTR (counter) mode cipher from the BouncyCastle cryptography library
                IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");
                cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", localKey), new byte[16]));
                return cipher.DoFinal(val);
            }

            private byte[] DecodeLocalKey()
            {
                IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");
                cipher.Init(false, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", localKey), new byte[16]));
                return cipher.DoFinal(encryptedKey);
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
                ProfilesDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Hoard", "Profiles");
        }

        /// <summary>
        /// Enumerates existing profiles
        /// </summary>
        /// <returns>list of found profiles</returns>
        public async Task<KeyStoreUtils.ProfileDesc[]> EnumerateProfiles()
        {
            List<KeyStoreUtils.ProfileDesc> profiles = new List<KeyStoreUtils.ProfileDesc>();
            await KeyStoreUtils.EnumerateProfiles(ProfilesDir, async (fileName) => {
                StreamReader jsonReader = new StreamReader(Path.Combine(ProfilesDir, fileName));
                JObject jobj = JObject.Parse(await jsonReader.ReadToEndAsync());
                jsonReader.Close();
                if (jobj.TryGetValue("address", out JToken valueAddress) && jobj.TryGetValue("name", out JToken valueName))
                {
                    profiles.Add(new KeyStoreUtils.ProfileDesc(valueName.Value<string>(), valueAddress.Value<string>(), null));
                }
            });
            return profiles.ToArray();
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
            Profile profile = new KeyStoreProfile(name, new HoardID(ecKey.GetPublicAddress()), privateKey.HexToByteArray());
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
            Tuple<string, byte[]> accountTuple = KeyStoreUtils.CreateProfile(name, password, ProfilesDir);
            Profile profile = new KeyStoreProfile(name, new HoardID(accountTuple.Item1), accountTuple.Item2);
            accountTuple.Item2.Initialize();
            return profile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="privKey"></param>
        /// <returns></returns>
        public async Task<Profile> CreateProfileFromPrivKey(string privKey)
        {
            ErrorCallbackProvider.ReportInfo("Creating user account.");
            string name = await UserInputProvider.RequestInput("", null, eUserInputType.kLogin, "new login");
            string password = await UserInputProvider.RequestInput(name, null, eUserInputType.kPassword, "new password");
            Tuple<string, byte[]> accountTuple = KeyStoreUtils.CreateProfile(name, password, privKey, ProfilesDir);
            Profile profile = new KeyStoreProfile(name, new HoardID(accountTuple.Item1), accountTuple.Item2);
            accountTuple.Item2.Initialize();
            return profile;
        }

        /// <summary>
        /// Deletes profile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="passwordNeeded"></param>
        /// <returns></returns>
        public async Task DeleteProfile(HoardID id, bool passwordNeeded = false)
        {
            await KeyStoreUtils.DeleteProfile(UserInputProvider, id, ProfilesDir, passwordNeeded);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addressOrName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<Profile> RequestProfile(string addressOrName, string password)
        {
            ErrorCallbackProvider.ReportInfo("Requesting user account.");
            KeyStoreUtils.ProfileDesc accountDesc = await KeyStoreUtils.RequestProfile(UserInputProvider, addressOrName, ProfilesDir, password);
            if (accountDesc != null)
            {
                return new KeyStoreProfile(accountDesc.Name, new HoardID(accountDesc.Address), accountDesc.PrivKey);
            }
            return null;
        }

        /// <summary>
        /// Retrieves profile stored in profile folder for particular user name.
        /// Found accounts will be stored in User object.
        /// </summary>
        /// <param name="addressOrName">user address or name to retrieve profile for</param>
        /// <returns>true if at least one profile has been properly loaded</returns>
        public async Task<Profile> RequestProfile(string addressOrName)
        {
            return await RequestProfile(addressOrName, null);
        }

        /// <summary>
        /// Saves account to selected directory.
        /// </summary>
        /// <param name="profilesDir"></param>
        /// <param name="profileData"></param>
        /// <returns></returns>
        public static async Task<string> SaveProfileAsync(string profilesDir, string profileData)
        {
            var accountJsonObject = JObject.Parse(profileData);
            if (accountJsonObject == null)
            {
                return null;
            }

            string id = accountJsonObject["id"].Value<string>();
            var fileName = id + ".keystore";

            //save the File
            using (var newfile = File.CreateText(Path.Combine(profilesDir, fileName)))
            {
                await newfile.WriteAsync(profileData);
                await newfile.FlushAsync();
            }

            return fileName;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profilesDir"></param>
        /// <param name="encryptedKey"></param>
        /// <returns></returns>
        public static string SaveProfile(string profilesDir, string encryptedKey)
        {
            if (!Directory.Exists(profilesDir))
            {
                Directory.CreateDirectory(profilesDir);
            }

            var keystoreJsonObject = JObject.Parse(encryptedKey);
            if (keystoreJsonObject == null)
            {
                return null;
            }

            string id = keystoreJsonObject["id"].Value<string>();
            var fileName = id + ".keystore";

            //save the File
            using (var newfile = File.CreateText(Path.Combine(profilesDir, fileName)))
            {
                newfile.Write(encryptedKey);
                newfile.Flush();
            }

            return fileName;
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
    }
}
