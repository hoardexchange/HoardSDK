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
        private static readonly byte[] IV = { 0xa5, 0x7f, 0xb2, 0xcc, 0xce, 0xe7, 0x77, 0xea, 0xdb, 0xb9, 0xc0, 0x52, 0x1a, 0x3d, 0xdd, 0xde };

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
                byte[] encryptionKey = AccountSynchronizer.GenerateKey(ProvideEncryptionPhrase());
                byte[] encryptedData = AccountSynchronizer.AESEncrypt(encryptionKey, Encoding.UTF8.GetBytes(password), ProvideIV());
                string hashedId = Hoard.Utils.Helper.Keccak256HexHashString(id.ToString());
                EncryptedData data = new EncryptedData();
                data.pswd = BitConverter.ToString(encryptedData).Replace("-", string.Empty);
                return data;
            }
            catch (Exception)
            {
                ErrorCallbackProvider.ReportError("Can't encrypt password for id " + id.ToString() + " failed!");
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
                byte[] decryptionKey = AccountSynchronizer.GenerateKey(ProvideEncryptionPhrase());
                byte[] decrypted = AccountSynchronizer.AESDecrypt(decryptionKey, WhisperService.HexStringToByteArray(encryptedData.pswd), ProvideIV());
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
