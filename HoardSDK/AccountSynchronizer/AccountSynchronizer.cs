using Nethereum.Signer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Hoard
{
    /// <summary>
    /// Class for transfer and synchronize accounts between different devices
    /// </summary>
    public class AccountSynchronizer
    {
        public static byte[] Encrypt(byte[] data, AsymmetricKeyParameter pubKeyParam)
        {
            X9ECParameters ecParams = ECNamedCurveTable.GetByName("Secp256k1");
            ECDomainParameters ecSpec = new ECDomainParameters(ecParams.Curve, ecParams.G, ecParams.N, ecParams.H);

            // generate ephemeral key pair
            IAsymmetricCipherKeyPairGenerator g = GeneratorUtilities.GetKeyPairGenerator("ECIES");
            g.Init(new ECKeyGenerationParameters(ecSpec, new SecureRandom()));
            AsymmetricCipherKeyPair generatedKeyPair = g.GenerateKeyPair();

            // create encryption engine
            IesEngine iesEngine = CreateIesEngine(true, generatedKeyPair.Private, pubKeyParam);

            // encrypt data
            byte[] encryptedData = iesEngine.ProcessBlock(data, 0, data.Length);

            // encode public key that must be used for decryption together with private key
            byte[] pubEnc = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(generatedKeyPair.Public).GetDerEncoded();

            // write public key length
            byte[] lengthBytes = new byte[4];
            lengthBytes[0] = (byte)(pubEnc.Length >> 24);
            lengthBytes[1] = (byte)(pubEnc.Length >> 16);
            lengthBytes[2] = (byte)(pubEnc.Length >> 8);
            lengthBytes[3] = (byte)pubEnc.Length;

            // write: public key length, public key, encrypted data
            byte[] outData = new byte[4 + pubEnc.Length + encryptedData.Length];
            System.Buffer.BlockCopy(lengthBytes, 0, outData, 0, 4);
            System.Buffer.BlockCopy(pubEnc, 0, outData, 4, pubEnc.Length);
            System.Buffer.BlockCopy(encryptedData, 0, outData, 4 + pubEnc.Length, encryptedData.Length);

            return outData;
        }

        public static byte[] Decrypt(byte[] data, AsymmetricKeyParameter privKeyParam)
        {
            // obtain public key length
            uint lengthBytes = (((uint)data[0]) << 24) | (((uint)data[1]) << 16) | (((uint)data[2]) << 8) | (uint)data[3];
            byte[] pubEnc = new byte[lengthBytes];
            Array.Copy(data, 4, pubEnc, 0, lengthBytes);

            // instantiate public key
            ECPublicKeyParameters pubKeyParam = (ECPublicKeyParameters)PublicKeyFactory.CreateKey(pubEnc);

            // get encrypted data
            long dataSize = data.Length - 4 - lengthBytes;
            byte[] dataEncrypted = new byte[dataSize];
            Array.Copy(data, 4 + lengthBytes, dataEncrypted, 0, dataSize);

            // create encryption engine 
            IesEngine iesEngine = CreateIesEngine(false, privKeyParam, pubKeyParam);

            // decrypt data
            byte[] plainData = iesEngine.ProcessBlock(dataEncrypted, 0, dataEncrypted.Length);

            return plainData;
        }

        static IesEngine CreateIesEngine(bool forEncryption, AsymmetricKeyParameter privKeyParam, AsymmetricKeyParameter pubKeyParam)
        {
            BufferedBlockCipher cipher = new PaddedBufferedBlockCipher(
                new CbcBlockCipher(new TwofishEngine()));
            IesEngine iesEngine = new IesEngine(
                new ECDHBasicAgreement(),
                new Kdf2BytesGenerator(new Sha1Digest()),
                new HMac(new Sha1Digest()),
                cipher);
            byte[] d = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            byte[] e = new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 };
            IesParameters p = new IesWithCipherParameters(d, e, 64, 128);

            iesEngine.Init(forEncryption, privKeyParam, pubKeyParam, p);
            return iesEngine;
        }

        /// <summary>
        /// Internal message data
        /// </summary>
        public class InternalData
        {
            /// <summary>
            /// Internal messages
            /// </summary>
            public enum InternalMessageId
            {
                /// Confirmation Pin
                ConfirmationPin = 0,

                /// Encryption key generation
                GenerateEncryptionKey,

                /// Transfer keystore
                TransferKeystore,
            }

            /// Internal message id
            public InternalMessageId id;

            /// message data length
            public int length;

            /// message data
            public string data;
        }

        /// <summary>
        /// Requested key message
        /// </summary>
        public class KeyRequestData
        {
            /// message data length
            public string EncryptionKeyPublicAddress;
        }

        static private int MaxRange = 10;

        /// <summary>
        /// Time out in seconds
        /// </summary>
        static protected int MessageTimeOut = 30;

        /// <summary>
        /// Maximal time in seconds to be spent on proof of work
        /// </summary>
        static protected int MaximalProofOfWorkTime = 8;

        /// <summary>
        /// Minimal PoW target required for this message
        /// </summary>
        static protected float MinimalPowTarget = 3.03f;

        /// <summary>
        /// 
        /// </summary>
        protected WhisperService WhisperService = null;

        /// <summary>
        /// 
        /// </summary>
        protected string SymKeyId = "";

        /// <summary>
        /// 
        /// </summary>
        protected string OriginalPin = "";

        /// Confirmation Pin
        public string ConfirmationPin
        {
            get;
            protected set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AccountSynchronizer(string url)
        {
            WhisperService = new WhisperService(url);
            ConfirmationPin = "";
        }

        private bool IsFairRandom(byte number, byte range)
        {
            int fullSetOfValues = Byte.MaxValue / range;
            return number < range * fullSetOfValues;
        }

        private int GenerateDigit(int range)
        {
            Debug.Assert(range > 0);
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            byte bRange = (byte)range;
            byte[] arr = new byte[1];
            do
            {
                provider.GetBytes(arr);
            }
            while (!IsFairRandom(arr[0], bRange));
            return (int)((arr[0] % bRange));
        }

        /// <summary>
        /// Establishes connection with geth
        /// --wsorigins="*" --ws --wsport "port" --shh --rpc --rpcport "port" --rpcapi personal,db,eth,net,web3,shh
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Initialize()
        {
            await WhisperService.Connect();
            //bool res = WhisperService.CheckConnection().Result;
            //res = WhisperService.SetMaxMessageSize(1024 * 1024 * 8).Result;
            //res = WhisperService.CheckConnection().Result;
            return true;
        }

        /// <summary>
        /// Closes connection with geth
        /// </summary>
        public async Task Shutdown()
        {
            await WhisperService.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected byte[] BuildMessage(InternalData.InternalMessageId id, byte[] data)
        {
            InternalData internalMsg = new InternalData();
            internalMsg.id = id;
            internalMsg.length = data.Length;
            internalMsg.data = Encoding.ASCII.GetString(data);
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(internalMsg));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="internalMessage"></param>
        protected virtual void OnTranslateMessage(InternalData internalMessage)
        {
        }

        private void TranslateMessage(WhisperService.ReceivedData msg)
        {
            try
            {
                byte[] data = msg.GetDecodedMessage();
                string textData = Encoding.ASCII.GetString(data);
                InternalData internalMessage = JsonConvert.DeserializeObject<InternalData>(textData);
                OnTranslateMessage(internalMessage);
            }
            catch(Exception e)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        protected string ConvertPinToTopic(string pin)
        {
            return "0x" + pin;
        }

        ///
        protected virtual void OnClear()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected static EthECKey GenerateKey(byte[] seed)
        {
            SecureRandom secureRandom = SecureRandom.GetInstance("SHA256PRNG", false);
            secureRandom.SetSeed(seed);
            var gen = new ECKeyPairGenerator();
            var keyGenParam = new KeyGenerationParameters(secureRandom, 256);
            gen.Init(keyGenParam);
            var keyPair = gen.GenerateKeyPair();
            var privateBytes = ((ECPrivateKeyParameters)keyPair.Private).D.ToByteArray();
            if (privateBytes.Length != 32)
            {
                byte[] newSeed = new byte[seed.Length + 1];
                Array.Copy(seed, newSeed, seed.Length);
                newSeed[seed.Length] = newSeed[seed.Length - 1];
                return GenerateKey(newSeed);
            }
            return new EthECKey(privateBytes, true);
        }

        /// <summary>
        /// Generates 8-digits pin
        /// </summary>
        /// <returns></returns>
        public string GeneratePin()
        {
            OnClear();

            int digitsToGenerate = 8;
            string pin = "";
            while (digitsToGenerate > 0)
            {
                pin += GenerateDigit(MaxRange).ToString();
                digitsToGenerate--;
            }
            return pin;
        }

        /// <summary>
        /// Registers message filter generated from pin
        /// </summary>
        /// <param name="pin">Pin</param>
        /// <returns></returns>
        public async Task<string> RegisterMessageFilter(string pin)
        {
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(pin);
            OriginalPin = pin;
            SHA256 sha256 = new SHA256Managed();
            var hashedPin = sha256.ComputeHash(Encoding.ASCII.GetBytes(pin));
            SymKeyId = await WhisperService.GenerateSymetricKeyFromPassword(Encoding.ASCII.GetString(hashedPin));
            WhisperService.SubscriptionCriteria msgCriteria = new WhisperService.SubscriptionCriteria(SymKeyId, "", "", 2.01f, topic, false);
            return await WhisperService.CreateNewMessageFilter(msgCriteria);
        }

        /// <summary>
        /// Unregisters message filter
        /// <param name="filter">Message filter</param>
        /// </summary>
        public async Task UnregisterMessageFilter(string filter)
        {
            bool res = await WhisperService.DeleteMessageFilter(filter);
            res = await WhisperService.DeleteSymetricKey(SymKeyId);
        }

        /// <summary>
        /// Gathers messages
        /// </summary>
        /// <param name="filter">Message filter</param>
        /// <returns></returns>
        public async Task<bool> Update(string filter)
        {
            List<WhisperService.ReceivedData> objects = await WhisperService.ReceiveMessage(filter);
            foreach (WhisperService.ReceivedData obj in objects)
            {
                TranslateMessage(obj);
            }
            return true;
        }

        //public bool SendPin(string pin)
        //{
        //    SHA256 sha256 = new SHA256Managed();
        //    var hashedPin = sha256.ComputeHash(Encoding.ASCII.GetBytes(pin));
        //    bool res = WhisperService.CheckConnection().Result;
        //    if (res)
        //    {
        //        string[] topic = { "0x07678231" };
        //        string privKey = "";// WhisperService.GetPrivateKey(ActualKeyId);
        //        string sig = "";// WhisperService.GetPublicKey(ActualKeyId);
        //        string symKeyId = WhisperService.GenerateSymetricKeyFromPassword("dupa").Result;
        //        WhisperService.SubscriptionCriteria msgCriteria = new WhisperService.SubscriptionCriteria(symKeyId, privKey, sig, 2.01f, topic, false);
        //        //string filter = WhisperService.Subscribe(msgCriteria);
        //        string filter = WhisperService.CreateNewMessageFilter(msgCriteria).Result;

        //        string symKeyId2 = WhisperService.GenerateSymetricKeyFromPassword("dupa").Result;
        //        WhisperService.SubscriptionCriteria msgCriteria2 = new WhisperService.SubscriptionCriteria(symKeyId2, privKey, sig, 2.01f, topic, false);
        //        string filter2 = WhisperService.CreateNewMessageFilter(msgCriteria2).Result;
        //        WhisperService.MessageDesc msg = new WhisperService.MessageDesc(symKeyId, "", "", 7, topic[0], hashedPin, "", 2, 2.01f, "");
        //        string resStr = WhisperService.SendMessage(msg).Result;
        //        List<WhisperService.ReceivedData> objects = WhisperService.ReceiveMessage(filter2).Result;
        //        foreach(WhisperService.ReceivedData obj in objects)
        //        {
        //            byte[] data = obj.GetDecodedMessage();
        //            string str = Encoding.ASCII.GetString(data);
        //        }
        //    }
        //    return true;
        //}
    }
}
