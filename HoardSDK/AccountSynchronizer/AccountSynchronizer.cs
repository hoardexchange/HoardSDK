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
using NBitcoin;
using System.IO;
using System.Collections.Concurrent;

namespace Hoard
{
    /// <summary>
    /// Class for transfer and synchronize accounts between different devices
    /// </summary>
    public class AccountSynchronizer
    {
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

                /// Transfer keystore request
                TransferKeystoreRequest,

                /// Transfer keystore answer
                TransferKeystoreAnswer,

                /// Custom data
                TransferCustomData
            }

            /// Internal message id
            public InternalMessageId id;

            /// message data length
            public int length;

            /// message data
            public byte[] data;
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
        static private int KeyStrength = 256;

        /// <summary>
        /// Time out in seconds
        /// </summary>
        static public int MessageTimeOut = (WhisperService.MAX_WAIT_TIME_IN_MS / 1000);

        /// <summary>
        /// Maximal time in seconds to be spent on proof of work
        /// </summary>
        static protected int MaximalProofOfWorkTime = 8;

        /// <summary>
        /// Minimal PoW target required for this message
        /// </summary>
        static protected float MinimalPowTarget = 3.03f;

        /// <summary>
        /// Maximal message chunk size
        /// </summary>
        static protected int ChunkSize = 128;

        /// <summary>
        /// Master key
        /// </summary>
        static public readonly string MasterKey = "xprv9s21ZrQH143K37MjeFycYaN4PVgP7AD6V8pS8mH3UJspeUUfF4pkQdh3gFTY9f1NPTKMEQkCZiE91uoiRDhZh65Kkytn8bkG1Xi5YfstAqH";

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

        /// <summary>
        /// 
        /// </summary>
        protected string mDateTime;

        private string SubscriptionId = "";

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
            mDateTime = "";
        }

        static private bool IsFairRandom(byte number, byte range)
        {
            int fullSetOfValues = Byte.MaxValue / range;
            return number < range * fullSetOfValues;
        }

        static private int GenerateDigit(int range)
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
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] Encrypt(EthECKey key, byte[] data)
        {
            string publicKey = BitConverter.ToString(key.GetPubKeyNoPrefix()).Replace("-", string.Empty).ToLower();
            X9ECParameters ecParams = ECNamedCurveTable.GetByName("Secp256k1");
            ECDomainParameters ecSpec = new ECDomainParameters(ecParams.Curve, ecParams.G, ecParams.N, ecParams.H);
            BigInteger ECPubQX = new BigInteger(publicKey.Substring(0, 64), 16);
            BigInteger ECPubQY = new BigInteger(publicKey.Substring(64, 64), 16);
            ECPublicKeyParameters pubKeyParam = new ECPublicKeyParameters(ecParams.Curve.ValidatePoint(ECPubQX, ECPubQY), ecSpec);

            BigInteger privKeyInt = new BigInteger(key.GetPrivateKeyAsBytes());
            ECPrivateKeyParameters privKeyParam = new ECPrivateKeyParameters(privKeyInt, ecSpec);

            // create encryption engine
            IesEngine iesEngine = CreateIesEngine(true, privKeyParam, pubKeyParam);

            // encrypt data
            byte[] encryptedData = iesEngine.ProcessBlock(data, 0, data.Length);

            // write: public key length, public key, encrypted data
            byte[] outData = new byte[encryptedData.Length];
            System.Buffer.BlockCopy(encryptedData, 0, outData, 0, encryptedData.Length);

            return outData;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="dataEncrypted"></param>
        /// <returns></returns>
        public static byte[] Decrypt(EthECKey key, byte[] dataEncrypted)
        {
            string publicKey = BitConverter.ToString(key.GetPubKeyNoPrefix()).Replace("-", string.Empty).ToLower();
            X9ECParameters ecParams = ECNamedCurveTable.GetByName("Secp256k1");
            ECDomainParameters ecSpec = new ECDomainParameters(ecParams.Curve, ecParams.G, ecParams.N, ecParams.H);
            BigInteger ECPubQX = new BigInteger(publicKey.Substring(0, 64), 16);
            BigInteger ECPubQY = new BigInteger(publicKey.Substring(64, 64), 16);
            ECPublicKeyParameters pubKeyParam = new ECPublicKeyParameters(ecParams.Curve.ValidatePoint(ECPubQX, ECPubQY), ecSpec);

            BigInteger privKeyInt = new BigInteger(key.GetPrivateKeyAsBytes());
            ECPrivateKeyParameters privKeyParam = new ECPrivateKeyParameters(privKeyInt, ecSpec);

            // create encryption engine 
            IesEngine iesEngine = CreateIesEngine(false, privKeyParam, pubKeyParam);

            // decrypt data
            return iesEngine.ProcessBlock(dataEncrypted, 0, dataEncrypted.Length);
        }

        private static IesEngine CreateIesEngine(bool forEncryption, AsymmetricKeyParameter privKeyParam, AsymmetricKeyParameter pubKeyParam)
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
        /// 
        /// </summary>
        /// <param name="privatekey"></param>
        /// <param name="data"></param>
        /// <param name="iv"></param>
        /// <returns></returns>
        public static byte[] AESEncrypt(byte[] privatekey, byte[] data, byte[] iv)
        {
            // Create a new AesManaged.    
            AesManaged aes = new AesManaged();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = KeyStrength;

            // Create encryptor    
            ICryptoTransform encryptor = aes.CreateEncryptor(privatekey, iv);

            // Create MemoryStream    
            MemoryStream ms = new MemoryStream();

            // Create crypto stream using the CryptoStream class. This class is the key to encryption    
            // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream    
            // to encrypt    
            CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Close();

            return ms.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="privatekey"></param>
        /// <param name="iv"></param>
        /// <param name="dataEncrypted"></param>
        /// <returns></returns>
        public static byte[] AESDecrypt(byte[] privatekey, byte[] dataEncrypted, byte[] iv)
        {
            // Create AesManaged    
            AesManaged aes = new AesManaged();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = KeyStrength;

            // Create a decryptor    
            ICryptoTransform decryptor = aes.CreateDecryptor(privatekey, iv);

            // Create the streams used for decryption.    
            MemoryStream ms = new MemoryStream();

            CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write);
            cs.Write(dataEncrypted, 0, dataEncrypted.Length);
            cs.Close();

            return ms.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="publicKey"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        //public static byte[] RSAEncrypt(byte[] publicKey, byte[] data)
        //{
        //    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(); 
        //    rsa.FromXmlString(publicKey);  
        //    return rsa.Encrypt(data, false);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="privateKey"></param>
        /// <param name="encryptedData"></param>
        /// <returns></returns>
        //public static byte[] RSADecrypt(byte[] privateKey, byte[] encryptedData)
        //{
        //    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(); 
        //    rsa.FromXmlString(privateKey);
        //    return rsa.Decrypt(encryptedData, false);
        //}

        /// <summary>
        /// Establishes connection with geth
        /// --wsorigins="*" --ws --wsport "port" --shh --rpc --rpcport "port" --rpcapi personal,db,eth,net,web3,shh
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Initialize()
        {
            return await WhisperService.Connect();
        }

        /// <summary>
        /// Closes connection with geth
        /// </summary>
        public async Task Shutdown()
        {
            await WhisperService.Close();
        }

        /// <summary>
        /// Clears synchronizer state
        /// </summary>
        public void Clear()
        {
            OnClear();
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
            internalMsg.data = data;
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(internalMsg));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="internalMessage"></param>
        protected virtual void OnTranslateMessage(InternalData internalMessage)
        {
        }

        public void TranslateMessage(WhisperService.ReceivedData msg)
        {
            try
            {
                byte[] data = msg.GetDecodedMessage();
                string textData = Encoding.UTF8.GetString(data);
                InternalData internalMessage = JsonConvert.DeserializeObject<InternalData>(textData);
                OnTranslateMessage(internalMessage);
            }
            catch(Exception)
            {
            }
        }

        private string PackHashedPin(byte[] hashedPin)
        {
            Debug.Assert(hashedPin.Length >= 4);
            byte[] packedPin = new byte[4];
            Array.Copy(hashedPin, packedPin, packedPin.Length);
            return BitConverter.ToString(packedPin).Replace("-", string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pin"></param>
        /// <returns></returns>
        protected string ConvertPinToTopic(string pin)
        {
            SHA256 sha256 = new SHA256Managed();
            var hashedPin = sha256.ComputeHash(Encoding.UTF8.GetBytes(pin));
            return "0x" + PackHashedPin(hashedPin);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static byte[] GenerateIV(string pin)
        {
            byte[] iv = new byte[16];
            SHA256 sha256 = new SHA256Managed();
            var hashedPin = sha256.ComputeHash(Encoding.UTF8.GetBytes(pin));
            Debug.Assert(hashedPin.Length >= 16);
            Array.Copy(hashedPin, iv, iv.Length);
            return iv;
        }

        ///
        protected virtual void OnClear()
        {
        }

        private static byte[] CalculateSeed(byte[] seed)
        {
            int val = 0;
            for(int i = 0; i < seed.Length; i++)
            {
                val += seed[i];
            }
            val = val % 255;
            byte[] newSeed = new byte[seed.Length + 1];
            Array.Copy(seed, newSeed, seed.Length);
            newSeed[seed.Length] = (byte)val;
            return newSeed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public byte[] GenerateKey(byte[] seed)
        {
            byte[] newSeed = CalculateSeed(seed);
            string path = "m";
            for(int i = 0; i < newSeed.Length; i++)
            {
                path += "/";
                path += newSeed[i].ToString();
             }
            //SecureRandom secureRandom = SecureRandom.GetInstance("SHA256PRNG", false);
            //secureRandom.SetSeed(newSeed);
            //var gen = new ECKeyPairGenerator();
            //var keyGenParam = new KeyGenerationParameters(secureRandom, KeyStrength);
            //gen.Init(keyGenParam);
            //var keyPair = gen.GenerateKeyPair();
            //var privateBytes = ((ECPrivateKeyParameters)keyPair.Private).D.ToByteArray();
            //Debug.Print("path: " + path + "\n");
            ExtKey childKey = ExtKey.Parse(MasterKey).Derive(new KeyPath(path));
            var privateBytes = childKey.PrivateKey.ToBytes();
            Debug.Assert(privateBytes.Length == 32);
            return privateBytes;
        }

        /// <summary>
        /// Generates 8-digits pin
        /// </summary>
        /// <returns></returns>
        static public string GeneratePin()
        {
            //OnClear(); 

            int digitsToGenerate = 8;
            string pin = "";
            while (digitsToGenerate > 0)
            {
                pin += GenerateDigit(MaxRange).ToString();
                digitsToGenerate--;
            }
            return pin;
        }

        private async Task<string> Subscribe(string pin)
        {
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(pin);
            OriginalPin = pin;
            SHA256 sha256 = new SHA256Managed();
            var hashedPin = sha256.ComputeHash(Encoding.UTF8.GetBytes(pin));
            SymKeyId = await WhisperService.GenerateSymetricKeyFromPassword(Encoding.UTF8.GetString(hashedPin));
            WhisperService.SubscriptionCriteria msgCriteria = new WhisperService.SubscriptionCriteria(SymKeyId, "", "", 2.01f, topic, true);
            return await WhisperService.Subscribe(msgCriteria);
        }

        private async Task<bool> Unsubscribe(string subscriptionId)
        {
            return await WhisperService.Unsubscribe(subscriptionId);
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
            var hashedPin = sha256.ComputeHash(Encoding.UTF8.GetBytes(pin));
            SymKeyId = await WhisperService.GenerateSymetricKeyFromPassword(Encoding.UTF8.GetString(hashedPin));
            WhisperService.SubscriptionCriteria msgCriteria = new WhisperService.SubscriptionCriteria(SymKeyId, "", "", 2.01f, topic, true);
            SubscriptionId = await WhisperService.Subscribe(msgCriteria);
            return await WhisperService.CreateNewMessageFilter(msgCriteria);
        }

        /// <summary>
        /// Unregisters message filter
        /// <param name="filter">Message filter</param>
        /// </summary>
        public async Task UnregisterMessageFilter(string filter)
        {
            bool res = true;
            if (SubscriptionId != "")
            {
                res = await WhisperService.Unsubscribe(SubscriptionId);
            }
            if (filter != "")
            {
                res = await WhisperService.DeleteMessageFilter(filter);
            }
            if (SymKeyId != "")
            {
                res = await WhisperService.DeleteSymetricKey(SymKeyId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        //public async Task<bool> Update(string filter)
        //{
        //    List<WhisperService.ReceivedData> objects = await WhisperService.ReceiveMessage(filter);
        //    if (objects == null)
        //        return false;
        //    foreach (WhisperService.ReceivedData obj in objects)
        //    {
        //        TranslateMessage(obj);
        //    }
        //    return true;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public void ProcessMessage()
        {
            ConcurrentQueue<WhisperService.ReceivedData> receivedMessagesQueue = WhisperService.GetReceivedMessages();
            if (receivedMessagesQueue.Count > 0)
            {
                WhisperService.ReceivedData rd = null;
                if (receivedMessagesQueue.TryDequeue(out rd))
                    TranslateMessage(rd);
            }
        }
    }
}
