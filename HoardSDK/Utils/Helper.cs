using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RLP;
using Nethereum.Signer;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Linq;
using System.Text;

namespace Hoard.Utils
{
    /// <summary>
    /// Utiltity and extension class.
    /// </summary>
    internal static class Helper
    {
        public static TResult GetPropertyValue<TResult>(this object t, string propertyName)
        {
            object val = t.GetType().GetProperties().Single(pi => pi.Name == propertyName).GetValue(t, null);
            return (TResult)val;
        }

        public static void SetPropertyValue<TResult>(this object t, string propertyName, TResult value)
        {
            t.GetType().GetProperties().Single(pi => pi.Name == propertyName).SetValue(t, value, null);
        }

        private static string ToHex(byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
            return result.ToString();
        }

        public static string Keccak256HexHashString(string StringIn)
        {
            var sha3 = new KeccakDigest(256);
            byte[] hash = new byte[sha3.GetDigestSize()];
            byte[] value = Encoding.Default.GetBytes(StringIn);
            sha3.BlockUpdate(value, 0, value.Length);
            sha3.DoFinal(hash, 0);
            return ToHex(hash, false);
        }

        internal static byte[][] ToBytes(this RLPCollection collection)
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
    }
}
