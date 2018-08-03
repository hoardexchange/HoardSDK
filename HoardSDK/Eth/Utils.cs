using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Util;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Math;

namespace Hoard.Eth
{
    public class Utils
    {
        static public BigInteger Mine(string challenge, BigInteger difficulty)
        {
            Byte[] b = new Byte[8];

            var sha3 = new KeccakDigest(512);

            var rnd = new Random();
            rnd.NextBytes(b);

            var nonce = new BigInteger(1, b);

            var challengeBI = new BigInteger(challenge, 16);

            while (true)
            {
                byte[] hashb = new byte[sha3.GetDigestSize()];
                byte[] value = challengeBI.ToByteArrayUnsigned().Concat(nonce.ToByteArrayUnsigned()).ToArray();
                sha3.BlockUpdate(value, 0, value.Length);
                sha3.DoFinal(hashb, 0);
                var v = new BigInteger(1, hashb.ToArray());
                if (v.CompareTo(difficulty) < 0)
                    break;
                nonce = nonce.Add(BigInteger.One).Mod(BigInteger.Two.ShiftLeft(64));
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
