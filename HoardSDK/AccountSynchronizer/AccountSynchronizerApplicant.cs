using Hoard.Utils.Base58Check;
using Nethereum.Signer;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Class for transfer and synchronize accounts between different devices
    /// </summary>
    public class AccountSynchronizerApplicant : AccountSynchronizer
    {
        private byte[] DecryptionKey;
        private byte[][] EncryptedKeystoreData;
        private int KeystoreReceiwed;
        private string DecryptedKeystoreData;

        /// <summary>
        /// Constructor
        /// </summary>
        public AccountSynchronizerApplicant(string url) : base(url)
        {
            WhisperService = new WhisperService(url);
            ConfirmationPin = "";
            DecryptedKeystoreData = "";
            Interlocked.Exchange(ref KeystoreReceiwed, 0);
            OnClear();
        }

        private byte[] GenerateDecryptionKey()
        {
            return GenerateKey(Encoding.UTF8.GetBytes(OriginalPin + mDateTime));
        }

        private string SendTransferRequest(byte[] key)
        {
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(OriginalPin);
            KeyRequestData keyRequestData = new KeyRequestData();
            SHA256 sha256 = new SHA256Managed();
            keyRequestData.EncryptionKeyPublicAddress = BitConverter.ToString(sha256.ComputeHash(key)).Replace("-", string.Empty).ToLower();
            string requestDataText = JsonConvert.SerializeObject(keyRequestData);
            byte[] data = BuildMessage(InternalData.InternalMessageId.TransferKeystoreRequest, Encoding.UTF8.GetBytes(requestDataText));
            WhisperService.MessageDesc msg = new WhisperService.MessageDesc(SymKeyId, "", "", MessageTimeOut, topic[0], data, "", MaximalProofOfWorkTime, MinimalPowTarget, "");
            return WhisperService.SendMessage(msg).Result;
        }

        private void AggregateMessage(byte[] data)
        {
            UInt32 id = BitConverter.ToUInt32(data, 0);
            UInt32 chunks = BitConverter.ToUInt32(data, 4);
            UInt32 length = BitConverter.ToUInt32(data, 8);
            if (EncryptedKeystoreData == null)
            {
                Debug.Assert(chunks > 0);
                EncryptedKeystoreData = new byte[chunks][];
            }
            Debug.Assert(id < EncryptedKeystoreData.Length);
            EncryptedKeystoreData[id] = new byte[length];
            Buffer.BlockCopy(data, 12, EncryptedKeystoreData[id], 0, EncryptedKeystoreData[id].Length);

            bool messageNotFinished = false;
            int fullDataSize = 0;
            for(int  i = 0; i < EncryptedKeystoreData.Length; i++)
            {
                if (EncryptedKeystoreData[i] == null)
                {
                    messageNotFinished = true;
                    break;
                }
                else
                {
                    Debug.Assert(EncryptedKeystoreData[i] != null);
                    fullDataSize += EncryptedKeystoreData[i].Length;
                }
            }
            if (messageNotFinished)
            {
                return;
            }

            byte[] fullEncryptedData = new byte[fullDataSize];
            int offset = 0;
            for (int i = 0; i < EncryptedKeystoreData.Length; i++)
            {
                Buffer.BlockCopy(EncryptedKeystoreData[i], 0, fullEncryptedData, offset, EncryptedKeystoreData[i].Length);
                offset += EncryptedKeystoreData[i].Length;
            }
            byte[] decrypted = AESDecrypt(DecryptionKey, fullEncryptedData, GenerateIV(OriginalPin));
            DecryptedKeystoreData = Encoding.UTF8.GetString(decrypted);
            Debug.Print("Decrypted Message: " + DecryptedKeystoreData);
            Interlocked.Exchange(ref KeystoreReceiwed, 1);
            EncryptedKeystoreData = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="internalMessage"></param>
        protected override void OnTranslateMessage(InternalData internalMessage)
        {
            switch (internalMessage.id)
            {
                case InternalData.InternalMessageId.GenerateEncryptionKey:
                    {
                        DecryptionKey = GenerateDecryptionKey();
                        string msg = SendTransferRequest(DecryptionKey);
                    }
                    break;
                case InternalData.InternalMessageId.TransferKeystoreAnswer:
                case InternalData.InternalMessageId.TransferCustomData:
                    {
                        AggregateMessage(internalMessage.data);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnClear()
        {
            ConfirmationPin = "";
            EncryptedKeystoreData = null;
            DecryptedKeystoreData = "";
            mDateTime = "";
            Interlocked.Exchange(ref KeystoreReceiwed, 0);
        }

        /// <summary>
        /// Sends confirmation pin to paired device
        /// </summary>
        /// <param name="confirmationPin">Confirmation Pin</param>
        /// <returns></returns>
        public async Task<string> SendConfirmationPin(string confirmationPin)
        {
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(OriginalPin);
            mDateTime = DateTime.Now.ToString();
            string pinAndDate = confirmationPin + "|" + mDateTime;
            byte[] data = BuildMessage(InternalData.InternalMessageId.ConfirmationPin, Encoding.UTF8.GetBytes(pinAndDate));
            WhisperService.MessageDesc msg = new WhisperService.MessageDesc(SymKeyId, "", "", MessageTimeOut, topic[0], data, "", MaximalProofOfWorkTime, MinimalPowTarget, "");
            return await WhisperService.SendMessage(msg);
        }

        /// <summary>
        /// Checks if keystore was received
        /// </summary>
        /// <returns></returns>
        public bool IsKeyStoreReceived()
        {
            return (Interlocked.CompareExchange(ref KeystoreReceiwed, KeystoreReceiwed, 0) != 0);
        }

        /// <summary>
        /// Returns encrypted keystore data
        /// </summary>
        public string GetKeystoreReceivedData()
        {
            return DecryptedKeystoreData;
        }
    }
}
