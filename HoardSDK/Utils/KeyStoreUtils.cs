﻿using Hoard.Exceptions;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Hoard.Utils
{
    /// <summary>
    /// Utilitites for key store management
    /// </summary>
    public class KeyStoreUtils
    {
        /// <summary>
        /// Description for creating an account
        /// </summary>
        public class ProfileDesc
        {
            /// <summary>
            /// 
            /// </summary>
            public string Address;

            /// <summary>
            /// 
            /// </summary>
            public byte[] PrivKey;

            /// <summary>
            /// 
            /// </summary>
            public string Name;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="name">Account name</param>
            /// <param name="address">Account public address</param>
            /// <param name="privKey">Account decrypted private key</param>
            public ProfileDesc(string name, string address, byte[] privKey)
            {
                Name = name;
                Address = address;
                PrivKey = privKey;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="profilesDir"></param>
        /// <param name="enumFunc"></param>
        /// <returns></returns>
        public static async Task EnumerateProfiles(string profilesDir, Func<string, Task> enumFunc)
        {
            ErrorCallbackProvider.ReportInfo(string.Format("Loading profiles from path: {0}", profilesDir));

            if (!Directory.Exists(profilesDir))
            {
                ErrorCallbackProvider.ReportWarning("Not found any profile files.");
                return;
            }

            var accountsFiles = Directory.GetFiles(profilesDir, "*.keystore");
            if (accountsFiles.Length == 0)
            {
                ErrorCallbackProvider.ReportWarning("Not found any profiles files.");
                return;
            }

            foreach (var fullPath in accountsFiles)
            {
                string fileName = Path.GetFileName(fullPath);
                if ((fileName != null) && (fileName != System.String.Empty))
                {
                    await enumFunc(fileName);
                }
            }
        }

        /// <summary>
        /// Loads account information for the user
        /// </summary>
        /// <param name="userInputProvider">Provider with user credentials</param>
        /// <param name="filename">filename of the file with account to load</param>
        /// <param name="profilesDir">folder where the key store files are stored</param>
        /// <returns>description making an account</returns>
        public static async Task<ProfileDesc> LoadProfile(IUserInputProvider userInputProvider, string filename, string profilesDir)
        {
            if (!Directory.Exists(profilesDir))
            {
                throw new HoardException(string.Format("Profile doesn't exists: {0:1}", profilesDir, filename));
            }

            var profileFiles = Directory.GetFiles(profilesDir, filename);
            if (profileFiles.Length == 0)
            {
                throw new HoardException(string.Format("Profile doesn't exists: {0:1}", profilesDir, filename));
            }
            ErrorCallbackProvider.ReportInfo(string.Format("Loading profiles {0}", profileFiles[0]));

            string json = null;
            using (var reader = File.OpenText(profileFiles[0]))
            {
                json = await reader.ReadToEndAsync();
            }
            var details = JObject.Parse(json);
            if (details == null)
            {
                throw new HoardException(string.Format("Can't parse json: {0}", json));
            }

            string address = details["address"].Value<string>();
            string name = "";
            if (details["name"] != null)
            {
                name = details["name"].Value<string>();
            }
            string password = await userInputProvider.RequestInput(name, new HoardID(address), eUserInputType.kPassword, address);
            var keyStoreService = new Nethereum.KeyStore.KeyStoreService();
            Nethereum.Signer.EthECKey key = null;

            try
            {
                key = new Nethereum.Signer.EthECKey(keyStoreService.DecryptKeyStoreFromJson(password, json), true);
                return new ProfileDesc(name, key.GetPublicAddress(), key.GetPrivateKeyAsBytes());
            }
            catch (Exception e)
            {
                throw new HoardException("Incorrect password", e);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="profilesDir"></param>
        /// <returns></returns>
        public static async Task<string> LoadEncryptedProfileAsync(HoardID id, string profilesDir)
        {
            if (!Directory.Exists(profilesDir))
            {
                throw new HoardException(string.Format("Profile doesn't exists: {0}", id.ToString()));
            }

            var profileFiles = Directory.GetFiles(profilesDir, "*.keystore");
            if (profileFiles.Length == 0)
            {
                throw new HoardException(string.Format("Profile doesn't exists: {0}", id.ToString()));
            }

            foreach (var fullPath in profileFiles)
            {
                string fileName = Path.GetFileName(fullPath);
                if ((fileName != null) && (fileName != System.String.Empty))
                {
                    string json = null;
                    using (var reader = File.OpenText(fullPath))
                    {
                        json = await reader.ReadToEndAsync();
                    }
                    var details = JObject.Parse(json);
                    if (details == null)
                    {
                        continue;
                    }
                    string address = details["address"].Value<string>();
                    if (new HoardID(address) == id)
                    {
                        return json;
                    }
                }
            }

            throw new HoardException(string.Format("Profile doesn't exists: {0}", id.ToString()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="profilesDir"></param>
        /// <returns></returns>
        public static string LoadEncryptedProfile(HoardID id, string profilesDir)
        {
            if (!Directory.Exists(profilesDir))
            {
                throw new HoardException(string.Format("Profile doesn't exists: {0}", id.ToString()));
            }

            var profileFiles = Directory.GetFiles(profilesDir, "*.keystore");
            if (profileFiles.Length == 0)
            {
                throw new HoardException(string.Format("Profile doesn't exists: {0}", id.ToString()));
            }

            foreach (var fullPath in profileFiles)
            {
                string fileName = Path.GetFileName(fullPath);
                if ((fileName != null) && (fileName != System.String.Empty))
                {
                    string json = File.ReadAllText(fullPath);
                    var details = JObject.Parse(json);
                    if (details == null)
                    {
                        continue;
                    }
                    string address = details["address"].Value<string>();
                    if (new HoardID(address) == id)
                    {
                        return json;
                    }
                }
            }

            throw new HoardException(string.Format("Profile doesn't exists: {0}", id.ToString()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userInputProvider"></param>
        /// <param name="addressOrName"></param>
        /// <param name="profilesDir"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<ProfileDesc> RequestProfile(IUserInputProvider userInputProvider, string addressOrName, string profilesDir, string password = null)
        {
            if (!Directory.Exists(profilesDir))
            {
                throw new HoardException(string.Format("Profile doesn't exists: {0}", addressOrName));
            }

            var profileFiles = Directory.GetFiles(profilesDir, "*.keystore");
            if (profileFiles.Length == 0)
            {
                throw new HoardException(string.Format("Profile doesn't exists: {0}", addressOrName));
            }

            string providedAddress = addressOrName;
            if (!providedAddress.StartsWith("0x"))
            {
                providedAddress = "0x" + providedAddress;
            }
            bool isValidAddress = Nethereum.Util.AddressUtil.Current.IsValidEthereumAddressHexFormat(providedAddress);
            if (isValidAddress == false)
            {
                throw new HoardException(string.Format("{0} is not a valid ethereum address", providedAddress));
            }

            foreach (var fullPath in profileFiles)
            {
                string fileName = Path.GetFileName(fullPath);
                if ((fileName != null) && (fileName != System.String.Empty))
                {
                    string json = File.ReadAllText(fullPath);
                    var details = JObject.Parse(json);
                    if (details == null)
                    {
                        continue;
                    }
                    string address = details["address"].Value<string>();
                    string profileName = "";
                    if (details["name"] != null)
                    {
                        profileName = details["name"].Value<string>();
                    }
                    if (((isValidAddress == true) && (address == providedAddress)) || ((isValidAddress == false) && (profileName == addressOrName)))
                    {
                        ErrorCallbackProvider.ReportInfo(string.Format("Loading account {0}", fileName));
                        string pswd = null;
                        if (password == null)
                        {
                            pswd = await userInputProvider.RequestInput(profileName, new HoardID(address), eUserInputType.kPassword, address);
                        }
                        else
                        {
                            pswd = password;
                        }
                        var keyStoreService = new Nethereum.KeyStore.KeyStoreService();
                        Nethereum.Signer.EthECKey key = null;
                        try
                        {
                            key = new Nethereum.Signer.EthECKey(keyStoreService.DecryptKeyStoreFromJson(pswd, json), true);
                            return new ProfileDesc(profileName, key.GetPublicAddress(), key.GetPrivateKeyAsBytes());
                        }
                        catch (Exception e)
                        {
                            throw new HoardException("Incorrect password", e);
                        }
                    }
                }
            }

            throw new HoardException(string.Format("Failed to request profile: {0}", providedAddress));
        }

        /// <summary>
        /// Create new profile.
        /// </summary>
        /// <param name="name">Name of new profile</param>
        /// <param name="password">Password for encrypting the profile</param>
        /// <param name="profilesDir">folder where to store key store data</param>
        /// <returns></returns>
        public static Tuple<string, byte[]> CreateProfile(string name, string password, string profilesDir)
        {
            if (!Directory.Exists(profilesDir))
            {
                Directory.CreateDirectory(profilesDir);
            }

            //generate new secure random key
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            string accountFile = CreateAccountKeyStoreFile(ecKey, password, name, profilesDir);
            return new Tuple<string, byte[]>(ecKey.GetPublicAddress(), ecKey.GetPrivateKeyAsBytes());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="privKey"></param>
        /// <param name="profilesDir"></param>
        /// <returns></returns>
        public static Tuple<string, byte[]> CreateProfile(string name, string password, string privKey, string profilesDir)
        {
            if (!Directory.Exists(profilesDir))
            {
                Directory.CreateDirectory(profilesDir);
            }

            var ecKey = new Nethereum.Signer.EthECKey(privKey);
            string accountFile = CreateAccountKeyStoreFile(ecKey, password, name, profilesDir);
            return new Tuple<string, byte[]>(ecKey.GetPublicAddress(), ecKey.GetPrivateKeyAsBytes());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userInputProvider"></param>
        /// <param name="id"></param>
        /// <param name="profilesDir"></param>
        /// <param name="passwordNeeded"></param>
        /// <returns></returns>
        public static async Task DeleteProfile(IUserInputProvider userInputProvider, HoardID id, string profilesDir, bool passwordNeeded)
        {
            if (!Directory.Exists(profilesDir))
            {
                throw new HoardException(string.Format("Profile doesn't exists: {0}", id.ToString()));
            }

            string[] files = Directory.GetFiles(profilesDir, "*.keystore");
            var keyStoreService = new Nethereum.KeyStore.KeyStoreService();
            foreach (string file in files)
            {
                StreamReader jsonReader = new StreamReader(file);
                JObject jobj = JObject.Parse(jsonReader.ReadToEnd());
                jsonReader.Close();
                JToken valueAddress;
                if (jobj.TryGetValue("address", out valueAddress))
                {
                    HoardID actualId = new HoardID(valueAddress.Value<string>());
                    if (id == actualId)
                    {
                        Nethereum.Signer.EthECKey key = null;
                        if (passwordNeeded)
                        {
                            string password = await userInputProvider.RequestInput(null, id, eUserInputType.kPassword, valueAddress.Value<string>());
                            try
                            {
                                key = new Nethereum.Signer.EthECKey(keyStoreService.DecryptKeyStoreFromJson(password, jobj.ToString()), true);
                            }
                            catch (Exception e)
                            {
                                throw new HoardException("Incorrect password", e);
                            }
                        }
                        File.Delete(file);
                        return;
                    }
                }
            }

            throw new HoardException(string.Format("Profile doesn't exists: {0}", id.ToString()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <param name="profilesDir"></param>
        /// <returns></returns>
        public static string ChangePassword(HoardID id, string oldPassword, string newPassword, string profilesDir)
        {
            if (!Directory.Exists(profilesDir))
            {
                throw new HoardException(string.Format("Profile doesn't exists: {0}", id.ToString()));
            }

            string[] files = Directory.GetFiles(profilesDir, "*.keystore");
            var keyStoreService = new Nethereum.KeyStore.KeyStoreService();
            foreach (string file in files)
            {
                StreamReader jsonReader = new StreamReader(file);
                JObject jobj = JObject.Parse(jsonReader.ReadToEnd());
                jsonReader.Close();
                JToken valueAddress;
                JToken name;
                if (jobj.TryGetValue("address", out valueAddress) && jobj.TryGetValue("name", out name))
                {
                    HoardID actualId = new HoardID(valueAddress.Value<string>());
                    if (id == actualId)
                    {
                        string newFile = null;
                        try
                        {
                            var key = new Nethereum.Signer.EthECKey(keyStoreService.DecryptKeyStoreFromJson(oldPassword, jobj.ToString()), true);
                            newFile = CreateAccountKeyStoreFile(key, newPassword, name.Value<string>(), profilesDir);
                        }
                        catch(Exception e)
                        {
                            throw new HoardException("Incorrect password", e);
                        }
                        if (newFile != null)
                        {
                            File.Delete(file);
                        }
                        return newFile;
                    }
                }
            }
            throw new HoardException(string.Format("Profile doesn't exists: {0}", id.ToString()));
        }

        private static string CreateAccountKeyStoreFile(Nethereum.Signer.EthECKey ecKey, string password, string name, string path)
        {
            //Get the public address (derivied from the public key)
            var address = ecKey.GetPublicAddress();

            //Create a store service, to encrypt and save the file using the web3 standard
            var service = new HoardKeyStoreScryptService();
            var encryptedKey = service.EncryptAndGenerateKeyStoreAsJson(password, ecKey.GetPrivateKeyAsBytes(), address).ToLower();
            if (encryptedKey == null)
            {
                throw new HoardException("Failed to encrypt kyestore");
            }
            var keystoreJsonObject = JObject.Parse(encryptedKey);
            if (keystoreJsonObject == null)
            {
                throw new HoardException("Failed to parse json file: " + encryptedKey);
            }
            keystoreJsonObject.Add("name", name);
            encryptedKey = keystoreJsonObject.ToString();
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
