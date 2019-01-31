using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
            }

            /// Internal message id
            public InternalMessageId id;

            /// message data length
            public int length;

            /// message data
            public string data;
        }

        static private int MaxRange = 10;

        /// <summary>
        /// 
        /// </summary>
        protected WhisperService WhisperService = null;

        /// <summary>
        /// 
        /// </summary>
        protected string SymKeyId = "";

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
