using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Hoard.Eth
{
    public class Utils
    {
        public const string EMPTY_ADDRESS = "0x0000000000000000000000000000000000000000";

        static public BigInteger Mine(string challenge, BigInteger difficulty)
        {
            Byte[] b = new Byte[8];

            var sha3 = new KeccakDigest(512);

            var rnd = new Random(0);
            rnd.NextBytes(b);
            b = b.Reverse().Concat(new byte[] { 0x0 }).ToArray();

            var nonce = new BigInteger(b);

            var challengeBI = BigInteger.Parse(challenge, NumberStyles.AllowHexSpecifier);

            while (true)
            {
                byte[] hashb = new byte[sha3.GetDigestSize()];
                byte[] value = challengeBI.ToByteArray().Reverse().Concat(nonce.ToByteArray().Reverse()).ToArray();
                sha3.BlockUpdate(value, 0, value.Length);
                sha3.DoFinal(hashb, 0);

                hashb = hashb.Reverse().Concat(new byte[] { 0x0 }).ToArray();
                var v = new BigInteger(hashb);
                if (v.CompareTo(difficulty) < 0)
                    break;

                nonce = (BigInteger.Add(nonce, BigInteger.One)) % ((new BigInteger(2)) << 64);
            }

            return nonce;
        }

        static public string Sign(string msg, string privateKey)
        {
            var dataBytes = Encoding.ASCII.GetBytes(msg);

            var signer = new Nethereum.Signer.EthereumMessageSigner();

            var ecKey = new Nethereum.Signer.EthECKey(privateKey);

            var signature = signer.Sign(dataBytes, ecKey);

            return signature;
        }
    }
}
