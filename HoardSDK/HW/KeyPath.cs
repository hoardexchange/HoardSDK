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
        /// Main path for Trezor and Ledger wallets
        /// </summary>
        public const string BIP44 = "m/44'/60'/0'/0";
        /// <summary>
        /// Legacy path for Ledger wallet (use BIP44 instead of this one)
        /// </summary>
        public const string BIP44LedgerLegacy = "m/44'/60'/0'";
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
