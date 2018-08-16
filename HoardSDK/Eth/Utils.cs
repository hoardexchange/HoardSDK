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
        static public BigInteger Mine(string challenge, BigInteger difficulty)
        {
            Byte[] b = new Byte[9];

            var sha3 = new KeccakDigest(512);

            var rnd = new Random();
            rnd.NextBytes(b);
            b[8] = 0x0;

            var nonce = new BigInteger(b);

            if (challenge.StartsWith("0x"))
            {
                challenge.Insert(2, "0");
            }
            else
            {
                challenge.Insert(0, "0");
            }
            var challengeBI = BigInteger.Parse(challenge, NumberStyles.AllowHexSpecifier);

            while (true)
            {
                byte[] hashb = new byte[sha3.GetDigestSize()];
                byte[] value = challengeBI.ToByteArray().Concat(nonce.ToByteArray()).ToArray();
                sha3.BlockUpdate(value, 0, value.Length);
                sha3.DoFinal(hashb, 0);

                byte[] hashb2 = new byte[hashb.Length + 1];
                hashb.CopyTo(hashb2, 0);
                hashb2[hashb.Length] = 0x00;
                var v = new BigInteger(hashb2);
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
