using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Hoard.Utils
{
    /// <summary>
    /// Utiltity and extension class.
    /// </summary>
    public static class Helper
    {
        internal static TResult GetPropertyValue<TResult>(this object t, string propertyName)
        {
            object val = t.GetType().GetProperties().Single(pi => pi.Name == propertyName).GetValue(t, null);
            return (TResult)val;
        }

        internal static void SetPropertyValue<TResult>(this object t, string propertyName, TResult value)
        {
            t.GetType().GetProperties().Single(pi => pi.Name == propertyName).SetValue(t, value, null);
        }

        internal static string ToHex(byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
            return result.ToString();
        }

        /// <summary>
        /// Helper function to convert RLP to byte array
        /// </summary>
        /// <param name="collection">RLP object</param>
        /// <returns></returns>
        public static byte[][] ToBytes(this RLPCollection collection)
        {
            var data = new byte[collection.Count][];
            for (var i = 0; i < collection.Count; ++i)
            {
                if (collection[i].RLPData != null)
                {
                    data[i] = new byte[collection[i].RLPData.Length];
                    collection[i].RLPData.CopyTo(data[i], 0);
                }
            }
            return data;
        }

        /// <summary>
        /// Keccak256 hashing algorithm
        /// </summary>
        /// <param name="StringIn"></param>
        /// <returns></returns>
        public static string Keccak256HexHashString(string StringIn)
        {
            var sha3 = new KeccakDigest(256);
            byte[] hash = new byte[sha3.GetDigestSize()];
            byte[] value = Encoding.Default.GetBytes(StringIn);
            sha3.BlockUpdate(value, 0, value.Length);
            sha3.DoFinal(hash, 0);
            return ToHex(hash, false);
        }

        /// <summary>AES encryption algorithm</summary>
        /// <param name="privatekey"></param>
        /// <param name="data"></param>
        /// <param name="iv"></param>
        /// <param name="keyStrength"></param>
        /// <returns></returns>
        public static byte[] AESEncrypt(byte[] privatekey, byte[] data, byte[] iv, int keyStrength)
        {
            // Create a new AesManaged.    
            AesManaged aes = new AesManaged();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = keyStrength;

            // Create encryptor    
            ICryptoTransform encryptor = aes.CreateEncryptor(privatekey, iv);

            // Create MemoryStream    
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            // Create crypto stream using the CryptoStream class. This class is the key to encryption    
            // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream    
            // to encrypt    
            CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Close();

            return ms.ToArray();
        }

        /// <summary>
        /// AES decryption algorithm
        /// </summary>
        /// <param name="privatekey"></param>
        /// <param name="iv"></param>
        /// <param name="dataEncrypted"></param>
        /// <param name="keyStrength">key strtength</param>
        /// <returns></returns>
        public static byte[] AESDecrypt(byte[] privatekey, byte[] dataEncrypted, byte[] iv, int keyStrength)
        {
            // Create AesManaged    
            AesManaged aes = new AesManaged();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = keyStrength;

            // Create a decryptor    
            ICryptoTransform decryptor = aes.CreateDecryptor(privatekey, iv);

            // Create the streams used for decryption.    
            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
            cs.Write(dataEncrypted, 0, dataEncrypted.Length);
            cs.Close();

            return ms.ToArray();
        }

        /// <summary>
        /// Returns HoardID that signed this transaction
        /// </summary>
        /// <param name="signatureStr"></param>
        /// <param name="rlpEncodedTransaction"></param>
        /// <returns>Signer of this transaction</returns>
        public static HoardID RecoverHoardIdFromTransaction(string signatureStr, byte[] rlpEncodedTransaction)
        {
            if (string.IsNullOrEmpty(signatureStr))
                return null;

            var signature = EthECDSASignatureFactory.ExtractECDSASignature(signatureStr);

            var rawHash = new Nethereum.Util.Sha3Keccack().CalculateHash(rlpEncodedTransaction);

            return new HoardID(EthECKey.RecoverFromSignature(signature, rawHash).GetPublicAddress());
        }

        /// <summary>
        /// Returns HoardID that signed given message
        /// </summary>
        /// <param name="message">message</param>
        /// <param name="signature">signature string (from Profile.SignMessage)</param>
        /// <returns>Signer of the message</returns>
        public static HoardID RecoverHoardIdFromMessage(byte[] message, string signature)
        {
            var msgSigner = new EthereumMessageSigner();
            return new HoardID(msgSigner.EcRecover(message, signature));
        }
    }
}
