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
            }

            /// Internal message id
            public InternalMessageId id;

            /// message data length
            public int length;

            /// message data
            public string data;
        }

        static private int MaxRange = 10;

        private WhisperService WhisperService = null;
        private string SymKeyIdFrom = "";
        private string SymKeyIdTo = "";
        private string ConfirmationPin = "";

        /// <summary>
        /// Constructor
        /// </summary>
        public AccountSynchronizer(string url)
        {
            WhisperService = new WhisperService(url);
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
        /// <returns></returns>
        public async Task Shutdown()
        {
            await WhisperService.Close();
        }

        private byte[] BuildMessage(InternalData.InternalMessageId id, byte[] data)
        {
            InternalData internalMsg = new InternalData();
            internalMsg.id = id;
            internalMsg.length = data.Length;
            internalMsg.data = Encoding.ASCII.GetString(data);
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(internalMsg));
        }

        private void TranslateMessage(WhisperService.ReceivedData msg)
        {
            try
            {
                byte[] data = msg.GetDecodedMessage();
                string textData = Encoding.ASCII.GetString(data);
                InternalData internalMessage = JsonConvert.DeserializeObject<InternalData>(textData);
                switch(internalMessage.id)
                {
                    case InternalData.InternalMessageId.ConfirmationPin:
                        ConfirmationPin = internalMessage.data;
                        break;
                    default:
                        break;
                }
            }
            catch(Exception e)
            {
            }
        }

        private string ConvertPinToTopic(string pin)
        {
            return "0x" + pin;
        }

        /// <summary>
        /// Generates 8-digits pin
        /// </summary>
        /// <returns></returns>
        public string GeneratePin()
        {
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
        /// Clears connection
        /// </summary>
        public async Task<string> RegisterMessageFilterFrom(string pin)
        {
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(pin);
            SHA256 sha256 = new SHA256Managed();
            var hashedPin = sha256.ComputeHash(Encoding.ASCII.GetBytes(pin));
            SymKeyIdFrom = await WhisperService.GenerateSymetricKeyFromPassword(Encoding.ASCII.GetString(hashedPin));
            WhisperService.SubscriptionCriteria msgCriteria = new WhisperService.SubscriptionCriteria(SymKeyIdFrom, "", "", 2.01f, topic, false);
            return await WhisperService.CreateNewMessageFilter(msgCriteria);
        }

        /// <summary>
        /// Clears connection
        /// </summary>
        public async Task UnregisterMessageFilterFrom(string filter)
        {
            bool res = await WhisperService.DeleteMessageFilter(filter);
            res = await WhisperService.DeleteSymetricKey(SymKeyIdFrom);
        }

        /// <summary>
        /// Clears connection
        /// </summary>
        public async Task<string> RegisterMessageFilterTo(string pin)
        {
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(pin);
            SHA256 sha256 = new SHA256Managed();
            var hashedPin = sha256.ComputeHash(Encoding.ASCII.GetBytes(pin));
            SymKeyIdTo = await WhisperService.GenerateSymetricKeyFromPassword(Encoding.ASCII.GetString(hashedPin));
            WhisperService.SubscriptionCriteria msgCriteria = new WhisperService.SubscriptionCriteria(SymKeyIdTo, "", "", 2.01f, topic, false);
            return await WhisperService.CreateNewMessageFilter(msgCriteria);
        }

        /// <summary>
        /// Clears connection
        /// </summary>
        public async Task UnregisterMessageFilterTo(string filter)
        {
            bool res = await WhisperService.DeleteMessageFilter(filter);
            res = await WhisperService.DeleteSymetricKey(SymKeyIdTo);
        }

        /// <summary>
        /// Clears connection
        /// </summary>
        public async Task<string> SendConfirmationPin(string originalPin, string confirmationPin)
        {
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(originalPin);
            byte[] data = BuildMessage(InternalData.InternalMessageId.ConfirmationPin, Encoding.ASCII.GetBytes(confirmationPin));
            WhisperService.MessageDesc msg = new WhisperService.MessageDesc(SymKeyIdTo, "", "", 7, topic[0], data, "", 2, 2.01f, "");
            return await WhisperService.SendMessage(msg);
        }

        /// <summary>
        /// Clears connection
        /// </summary>
        public async Task<bool> Update(string filter)
        {
            List<WhisperService.ReceivedData> objects = await WhisperService.ReceiveMessage(filter);
            foreach (WhisperService.ReceivedData obj in objects)
            {
                TranslateMessage(obj);
            }
            return true;
        }

        /// <summary>
        /// Clears connection
        /// </summary>
        public async Task<bool> TransferAccountFrom(string pin)
        {
            return false;
        }

        /// <summary>
        /// Clears connection
        /// </summary>
        public async Task<bool> TransferAccountTo(string pin)
        {
            return false;
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
