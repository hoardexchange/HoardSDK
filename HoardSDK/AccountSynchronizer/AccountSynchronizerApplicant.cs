using Hoard.Utils.Base58Check;
using Nethereum.Signer;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System;
using System.Diagnostics;
using System.IO;
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
        private EthECKey DecryptionKey;
        private byte[][] EncryptedKeystoreData;
        private int KeystoreReceiwed;
        private string KeyStoreEncryptedData;

        /// <summary>
        /// Constructor
        /// </summary>
        public AccountSynchronizerApplicant(string url) : base(url)
        {
            WhisperService = new WhisperService(url);
            ConfirmationPin = "";
            KeyStoreEncryptedData = "";
            Interlocked.Exchange(ref KeystoreReceiwed, 0);
        }

        private EthECKey GenerateDecryptionKey()
        {
            return GenerateKey(Encoding.ASCII.GetBytes(OriginalPin));
        }

        private string SendTransferRequest(EthECKey key)
        {
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(OriginalPin);
            KeyRequestData keyRequestData = new KeyRequestData();
            keyRequestData.EncryptionKeyPublicAddress = key.GetPublicAddress();
            string requestDataText = JsonConvert.SerializeObject(keyRequestData);
            string subData = "0x" + BitConverter.ToString(Encoding.ASCII.GetBytes(requestDataText)).Replace("-", string.Empty);
            byte[] data = BuildMessage(InternalData.InternalMessageId.TransferKeystore, Encoding.ASCII.GetBytes(subData));
            WhisperService.MessageDesc msg = new WhisperService.MessageDesc(SymKeyId, "", "", MessageTimeOut, topic[0], data, "", MaximalProofOfWorkTime, MinimalPowTarget, "");
            return WhisperService.SendMessage(msg).Result;
        }

        private void AggregateMessage(string data)
        {
            byte[] chunk = WhisperService.HexStringToByteArray(data.Substring(2));
            byte id = chunk[0];
            byte chunks = chunk[1];
            if (EncryptedKeystoreData == null)
            {
                EncryptedKeystoreData = new byte[chunks][];
            }
            Debug.Assert(id < EncryptedKeystoreData.Length);
            EncryptedKeystoreData[id] = new byte[chunk.Length - 2];
            Buffer.BlockCopy(chunk, 2, EncryptedKeystoreData[id], 0, EncryptedKeystoreData[id].Length);

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
            byte[] decrypted = Decrypt(DecryptionKey, fullEncryptedData);
            KeyStoreEncryptedData = Encoding.ASCII.GetString(decrypted);
            Debug.Print("Decrypted Message: " + KeyStoreEncryptedData);
            Interlocked.Exchange(ref KeystoreReceiwed, 0);
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
                case InternalData.InternalMessageId.TransferKeystore:
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
            KeyStoreEncryptedData = "";
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
            byte[] data = BuildMessage(InternalData.InternalMessageId.ConfirmationPin, Encoding.ASCII.GetBytes(confirmationPin));
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
            return KeyStoreEncryptedData;
        }
    }
}
