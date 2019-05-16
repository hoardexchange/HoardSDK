using Nethereum.Hex.HexConvertors.Extensions;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Class for transfer and synchronize accounts between different devices
    /// </summary>
    public abstract class AccountSynchronizer
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
                /// 
                KeeperPublicKey = 0,

                /// 
                ApplicantPublicKey,
                
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
        /// Time out in seconds
        /// </summary>
        static public int MessageTimeOut = (WhisperService.MAX_WAIT_TIME_IN_MS / 1000);

        /// <summary>
        /// Maximal time in seconds to be spent on proof of work
        /// </summary>
        static protected int MaximalProofOfWorkTime = 8;

        /// <summary>
        /// Maximal message chunk size
        /// </summary>
        static protected int ChunkSize = 128;

        /// <summary>
        /// Minimal PoW target required for this message
        /// </summary>
        static protected float MinimalPowTarget = 3.03f;

        /// <summary>
        /// Internal WhisperService
        /// </summary>
        protected WhisperService WhisperService = null;

        /// <summary>
        /// Symmetric key generated on Whisper node
        /// </summary>
        private string SymKeyId = string.Empty;

        /// <summary>
        /// Subscription ID from Whisper node
        /// </summary>
        private string SubscriptionId = string.Empty;

        /// <summary>
        /// Public key
        /// </summary>
        protected byte[] publicKey = new byte[0];

        /// <summary>
        /// Publick key
        /// </summary>
        public byte[] PublicKey { get { return publicKey; } }

        private AsymmetricCipherKeyPair KeyPair = null;
        private string topic = "";
        private X9ECParameters X9 = null;
        private ECDomainParameters EcSpec = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public AccountSynchronizer(IWebSocketProvider webSocketProvider)
        {
            WhisperService = new WhisperService(webSocketProvider);
            X9 = ECNamedCurveTable.GetByName("prime239v1");
            GenerateKeyPair();
        }

        /// <summary>
        /// Establishes connection with geth. Call Subscribe or Registerfilter manually
        /// --wsorigins="*" --ws --wsport "port" --shh --rpc --rpcport "port" --rpcapi personal,db,eth,net,web3,shh
        /// </summary>
        /// <param name="ctoken">cancelation token</param>
        /// <returns></returns>
        public async Task<bool> Initialize(CancellationToken ctoken)
        {
            bool result = await WhisperService.Connect(ctoken);
            return result;
        }

        /// <summary>
        /// Establishes connection with geth and subscribes for messages with given pin
        /// --wsorigins="*" --ws --wsport "port" --shh --rpc --rpcport "port" --rpcapi personal,db,eth,net,web3,shh
        /// </summary>
        /// <param name="pin">pin to subscribe to.</param>
        /// <param name="ctoken">cancelation token</param>
        /// <returns></returns>
        public async Task<bool> Initialize(string pin, CancellationToken ctoken)
        {
            bool result = await WhisperService.Connect(ctoken);
            if (result)
            {
                SubscriptionId = await Subscribe(pin, ctoken);
                result = !string.IsNullOrEmpty(SubscriptionId);
            }
            return result;
        }

        /// <summary>
        /// Closes connection with geth and unsubscribes from node
        /// </summary>
        public async Task Shutdown(CancellationToken ctoken)
        {
            if (WhisperService.IsConnected)
            {
                if (!string.IsNullOrEmpty(SubscriptionId))
                    await Unsubscribe(SubscriptionId, ctoken);
                if (!string.IsNullOrEmpty(SymKeyId))
                    await WhisperService.DeleteSymetricKey(SymKeyId, ctoken);
            }
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
        protected abstract void OnTranslateMessage(InternalData internalMessage);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        protected void TranslateMessage(WhisperService.SubscriptionResponse msg)
        {
            byte[] data = msg.GetDecodedMessage();
            string textData = Encoding.UTF8.GetString(data);
            InternalData internalMessage = JsonConvert.DeserializeObject<InternalData>(textData);
            OnTranslateMessage(internalMessage);
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

            Debug.Assert(hashedPin.Length >= 4);
            byte[] packedPin = new byte[4];
            Array.Copy(hashedPin, packedPin, packedPin.Length);

            return packedPin.ToHex(true);
        }

        ///
        protected virtual void OnClear()
        {
        }

        /// <summary>
        /// Subscribes for messages on with topic generated from pin
        /// </summary>
        /// <param name="pin">pin to create topic from</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns>subscription id</returns>
        public async Task<string> Subscribe(string pin, CancellationToken ctoken)
        {            
            topic = ConvertPinToTopic(pin);
            SHA256 sha256 = new SHA256Managed();
            var hashedPin = sha256.ComputeHash(Encoding.UTF8.GetBytes(pin));
            if (string.IsNullOrEmpty(SymKeyId))
                SymKeyId = await WhisperService.GenerateSymetricKeyFromPassword(Encoding.UTF8.GetString(hashedPin), ctoken);
            WhisperService.SubscriptionCriteria msgCriteria = new WhisperService.SubscriptionCriteria(SymKeyId, "", "", 2.01f, new string[] { topic }, true);
            WhisperService.OnSubscriptionMessage += TranslateMessage;
            SubscriptionId = await WhisperService.Subscribe(msgCriteria, ctoken);
            return SubscriptionId;
        }

        /// <summary>
        /// Unsubscribe from listening on given id
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<bool> Unsubscribe(string subscriptionId, CancellationToken ctoken)
        {
            bool res = true;
            if (!string.IsNullOrEmpty(SymKeyId))
            {
                res = await WhisperService.DeleteSymetricKey(SymKeyId, ctoken);
            }
            return res && await WhisperService.Unsubscribe(subscriptionId, ctoken);
        }

        /// <summary>
        /// Registers message filter generated from pin
        /// </summary>
        /// <param name="pin">Pin</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<string> RegisterMessageFilter(string pin, CancellationToken ctoken)
        {
            topic = ConvertPinToTopic(pin);
            SHA256 sha256 = new SHA256Managed();
            var hashedPin = sha256.ComputeHash(Encoding.UTF8.GetBytes(pin));
            if (string.IsNullOrEmpty(SymKeyId))
                SymKeyId = await WhisperService.GenerateSymetricKeyFromPassword(Encoding.UTF8.GetString(hashedPin), ctoken);
            WhisperService.SubscriptionCriteria msgCriteria = new WhisperService.SubscriptionCriteria(SymKeyId, "", "", 2.01f, new string[] { topic }, true);
            return await WhisperService.CreateNewMessageFilter(msgCriteria, ctoken);
        }

        /// <summary>
        /// Unregisters message filter
        /// <param name="filter">Message filter</param>
        /// <param name="ctoken">cancellation token</param>
        /// </summary>
        public async Task UnregisterMessageFilter(string filter, CancellationToken ctoken)
        {
            bool res = true;
            if (!string.IsNullOrEmpty(filter))
            {
                res = await WhisperService.DeleteMessageFilter(filter, ctoken);
                filter = string.Empty;
            }
            if (!string.IsNullOrEmpty(SymKeyId))
            {
                res = await WhisperService.DeleteSymetricKey(SymKeyId, ctoken);
                SymKeyId = string.Empty;
            }
        }

        private void GenerateKeyPair()
        {
            IAsymmetricCipherKeyPairGenerator g = GeneratorUtilities.GetKeyPairGenerator("ECIES");
            //TODO ?
            EcSpec = new ECDomainParameters(X9.Curve, X9.G, X9.N, X9.H);

            g.Init(new ECKeyGenerationParameters(EcSpec, new SecureRandom()));

            KeyPair = g.GenerateKeyPair();

            if (KeyPair != null)
            {
                var q = ((ECPublicKeyParameters)KeyPair.Public).Q.Normalize();
                publicKey = X9.Curve.CreatePoint(q.XCoord.ToBigInteger(), q.YCoord.ToBigInteger()).GetEncoded();
            }
        }

        /// <summary>
        /// Encrypts data using receiver public key and own keypair
        /// </summary>
        /// <param name="receiverPublicKeyBytes">key to encrypt with</param>
        /// <param name="data">data to encrypt</param>
        /// <returns>encrypted data</returns>
        protected byte[] EncryptData(byte[] receiverPublicKeyBytes, byte[] data)
        {
            IesEngine iesEngine = CreateIesEngine(true, receiverPublicKeyBytes);
            return iesEngine.ProcessBlock(data, 0, data.Length);
        }

        /// <summary>
        /// Decrypts data using receiver public key and own keypair
        /// </summary>
        /// <param name="senderPublicKeyBytes">key to decrypt with</param>
        /// <param name="encodedData">data to decrypt</param>
        /// <returns>decrypted data</returns>
        protected byte[] DecryptData(byte[] senderPublicKeyBytes, byte[] encodedData)
        {
            IesEngine iesEngine = CreateIesEngine(false, senderPublicKeyBytes);
            return iesEngine.ProcessBlock(encodedData, 0, encodedData.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="forEncryption"></param>
        /// <param name="publicKeyBytes"></param>
        /// <returns></returns>
        protected IesEngine CreateIesEngine(bool forEncryption, byte[] publicKeyBytes)
        {
            // TODO ensure parameters are safe enough
            IesEngine iesEngine = new IesEngine(new ECDHBasicAgreement(), new Kdf2BytesGenerator(new Sha1Digest()), new HMac(new Sha1Digest()));
            byte[] d = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            byte[] e = new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 };
            IesParameters parameters = new IesParameters(d, e, 64);

            ECPublicKeyParameters publicKey = new ECPublicKeyParameters(X9.Curve.DecodePoint(publicKeyBytes), EcSpec);
            iesEngine.Init(forEncryption, KeyPair.Private, (AsymmetricKeyParameter)publicKey, parameters);
            return iesEngine;
        }

        /// <summary>
        /// Creates symmetric key from sender's public key and own key pair
        /// </summary>
        /// <param name="publicKeyBytes"></param>
        /// <returns></returns>
        protected byte[] GetSymmetricKey(byte[] publicKeyBytes)
        {
            ECPublicKeyParameters publicKey = new ECPublicKeyParameters(X9.Curve.DecodePoint(publicKeyBytes), EcSpec);

            var agree = new ECDHBasicAgreement();
            agree.Init(KeyPair.Private);
            BigInteger z = agree.CalculateAgreement((AsymmetricKeyParameter)publicKey);
            return BigIntegers.AsUnsignedByteArray(agree.GetFieldSize(), z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<string> SendMessage(byte[] data, CancellationToken ctoken)
        {
            var msg = new WhisperService.MessageDesc(SymKeyId, "", "", MessageTimeOut, topic, data, "", MaximalProofOfWorkTime, MinimalPowTarget, "");
            return await WhisperService.SendMessage(msg, ctoken);
        }
    }
}
