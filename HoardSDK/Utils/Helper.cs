using Nethereum.RLP;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Hoard.Utils
{
    public static class Helper
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

        public static string SHA256HexHashString(string StringIn)
        {
            string hashString;
            using (var sha256 = SHA256Managed.Create())
            {
                var hash = sha256.ComputeHash(Encoding.Default.GetBytes(StringIn));
                hashString = ToHex(hash, false);
            }

            return hashString;
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
