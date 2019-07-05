using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class PasswordProvider
    {
        private readonly byte[] IV = { 0xa5, 0x7f, 0xb2, 0xcc, 0xce, 0xe7, 0x77, 0xea, 0xdb, 0xb9, 0xc0, 0x52, 0x1a, 0x3d, 0xdd, 0xde };

        /// <summary>
        /// 
        /// </summary>
        public class EncryptedData
        {
            /// <summary>
            /// 
            /// </summary>
            public string pswd;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual byte[] ProvideIV()
        {
            return IV;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual byte[] ProvideEncryptionPhrase()
        {
            return Encoding.UTF8.GetBytes("Beam me up scotty");
        }

        private static byte[] CalculateSeed(byte[] seed)
        {
            int val = 0;
            for (int i = 0; i < seed.Length; i++)
            {
                val += seed[i];
            }
            val = val % 255;
            byte[] newSeed = new byte[seed.Length + 1];
            Array.Copy(seed, newSeed, seed.Length);
            newSeed[seed.Length] = (byte)val;
            return newSeed;
        }

        private static byte[] GenerateKey(byte[] seed)
        {
            byte[] newSeed = CalculateSeed(seed);
            string path = "m";
            for (int i = 0; i < newSeed.Length; i++)
            {
                path += "/";
                path += newSeed[i].ToString();
            }

            const string MasterKey = "xprv9s21ZrQH143K37MjeFycYaN4PVgP7AD6V8pS8mH3UJspeUUfF4pkQdh3gFTY9f1NPTKMEQkCZiE91uoiRDhZh65Kkytn8bkG1Xi5YfstAqH";

            ExtKey childKey = ExtKey.Parse(MasterKey).Derive(new KeyPath(path));
            var encryptionKey = childKey.PrivateKey.ToBytes();
            Debug.Assert(encryptionKey.Length == 32);
            return encryptionKey;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public EncryptedData EncryptPassword(HoardID id, string password)
        {
            try
            {
                byte[] encryptionKey = GenerateKey(ProvideEncryptionPhrase());
                byte[] _iv = ProvideIV();
                byte[] encryptedData = Helper.AESEncrypt(encryptionKey, Encoding.UTF8.GetBytes(password), _iv, 256);
                string hashedId = Helper.Keccak256HexHashString(id.ToString());
                EncryptedData data = new EncryptedData();
                data.pswd = BitConverter.ToString(encryptedData).Replace("-", string.Empty);
                return data;
            }
            catch (Exception e)
            {
                ErrorCallbackProvider.ReportError("Can't encrypt password for id " + id.ToString() + " failed! " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="encryptedData"></param>
        /// <returns></returns>
        public string DecryptPassword(HoardID id, EncryptedData encryptedData)
        {
            try
            {
                byte[] decryptionKey = GenerateKey(ProvideEncryptionPhrase());
                byte[] decrypted = Helper.AESDecrypt(decryptionKey, WhisperService.HexStringToByteArray(encryptedData.pswd), ProvideIV(), 256);
                return Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception)
            {
                ErrorCallbackProvider.ReportError("Can't decrypt password for id " + id.ToString());
                return null;
            }
        }
    }
}
