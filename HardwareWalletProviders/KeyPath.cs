using System;
using System.IO;
using System.Linq;

namespace Hoard.HW
{
    /// <summary>
    /// Helper class with a set of known derivation paths used to obtain wallets from hardware wallets
    /// </summary>
    public static class DerivationPath
    {
        /// <summary>
        /// Known types of chain (based on that a different chain should be accesssed)
        /// </summary>
        public enum ChainType
        {
            /// <summary>
            /// Bitcoin
            /// </summary>
            kBTC = 0,
            /// <summary>
            /// Ethereum
            /// </summary>
            kETH = 60,
            /// <summary>
            /// xDai
            /// </summary>
            kXDAI = 700,
        }

        /// <summary>
        /// Default BIP44 path
        /// </summary>
        public static string DefaultBIP44
        {
            get
            {
                return CreateBIP44Path(ChainType.kETH, 0, 0);
            }
        }

        /// <summary>
        /// Creates derivatin path for Trezor and Ledger wallets in BIP44 format m/44'/coinType'/account'/subchain
        /// </summary>
        /// <param name="chainType">type of coin (60 for ETH used as default)</param>
        /// <param name="accountIdx">account index, 0 as default</param>
        /// <param name="subchain">0 for external (default), 1 for internal</param>
        /// <returns>BIP44 derivation path</returns>
        public static string CreateBIP44Path(int chainType, int accountIdx, int subchain)
        {
            return string.Format("m/44'/{0}'/{1}'/{2}", chainType, accountIdx, subchain);
        }

        /// <summary>
        /// Creates derivatin path for Trezor and Ledger wallets in BIP44 format m/44'/coinType'/account'/subchain
        /// </summary>
        /// <param name="chainType">type of coin (kETH used as default)</param>
        /// <param name="accountIdx">account index, 0 as default</param>
        /// <param name="subchain">0 for external (default), 1 for internal</param>
        /// <returns>BIP44 derivation path</returns>
        public static string CreateBIP44Path(ChainType chainType, int accountIdx, int subchain)
        {
            return CreateBIP44Path((int)chainType, accountIdx, subchain);
        }
    }

    internal class KeyPath
    {
        public uint[] Indices { get; private set; }

        public KeyPath(string path)
        {
            Indices = path.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(p => p != "m")
                .Select(i =>
                {
                    bool hardened = i.EndsWith("'");
                    var nonhardened = hardened ? i.Substring(0, i.Length - 1) : i;
                    var idx = uint.Parse(nonhardened);
                    return hardened ? idx | 0x80000000u : idx;
                })
                .ToArray();

            if (Indices.Length > 10)
            {
                throw new ArgumentOutOfRangeException("keypath", "The key path should have a maximum size of 10 derivations");
            }
        }

        public KeyPath(params uint[] _indices)
        {
            Indices = _indices;
        }

        public KeyPath Derive(uint index)
        {
            return Derive(new KeyPath(index));
        }

        public KeyPath Derive(KeyPath derivation)
        {
            return new KeyPath(Indices.Concat(derivation.Indices).ToArray());
        }

        public byte[] ToBytes()
        {
            byte[] data;
            using (var memory = new MemoryStream())
            {
                memory.WriteByte((byte)Indices.Length);
                for (var i = 0; i < Indices.Length; i++)
                {
                    var bytes = Helpers.ToBytes(Indices[i]);
                    memory.Write(bytes, 0, bytes.Length);

                }
                data = memory.ToArray();
            }
            return data;
        }
    }
}
