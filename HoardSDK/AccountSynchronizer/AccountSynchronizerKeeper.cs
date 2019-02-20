using Nethereum.Signer;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Class for transfer and synchronize accounts between different devices
    /// </summary>
    public class AccountSynchronizerKeeper : AccountSynchronizer
    {
        private int ConfirmationPinReceiwed;

        // 0 - not checked
        // 1 - confirmed
        // -1 - not confirmed
        private int ConfirmationStatus; 

        private EthECKey EncryptionKey;
        private List<string> MessageChunks;

        /// <summary>
        /// Address of requested key
        /// </summary>
        public string PublicAddressToTransfer
        {
            private get;
            set;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AccountSynchronizerKeeper(string url) : base(url)
        {
            WhisperService = new WhisperService(url);
            ConfirmationPin = "";
            PublicAddressToTransfer = "";
            Interlocked.Exchange(ref ConfirmationPinReceiwed, 0);
            Interlocked.Exchange(ref ConfirmationStatus, 0);
            MessageChunks = new List<string>();
        }

        //private static Tuple<string, Nethereum.Web3.Accounts.Account> FindKeystore(string pass, string userAddress)
        //{
        //    string keystoreFolder = SDKManagerUtils.GetHoardKeyStoreFolder();
        //    if (Directory.Exists(keystoreFolder))
        //    {
        //        var files = Directory.EnumerateFiles(keystoreFolder, "*.keystore");
        //        foreach (string filePath in files)
        //        {
        //            Tuple<string, string> nameAndAddress = SDKManagerUtils.LoadNameAndAddressFromKeyStore(filePath);
        //            if (nameAndAddress.Item2 == userAddress)
        //            {
        //                return SDKManagerUtils.LoadAccountFromKeyStore(filePath, pass);
        //            }
        //        }
        //    }
        //    return null;
        //}

        /// <summary>
        /// Checks if key keeper received confirmation pin
        /// </summary>
        /// <returns></returns>
        public bool ConfirmationPinReceived()
        {
            return (Interlocked.CompareExchange(ref ConfirmationPinReceiwed, ConfirmationPinReceiwed, 0) != 0);
        }

        /// <summary>
        /// Checks if applicant sent proper encryption key public address
        /// </summary>
        /// <returns></returns>
        public int GetConfirmationStatus()
        {
            return ConfirmationStatus;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="internalMessage"></param>
        protected override void OnTranslateMessage(InternalData internalMessage)
        {
            switch (internalMessage.id)
            {
                case InternalData.InternalMessageId.ConfirmationPinRequest:
                    {
                        ConfirmationPin = internalMessage.data;
                        Interlocked.Exchange(ref ConfirmationPinReceiwed, 1);
                    }
                    break;
                case InternalData.InternalMessageId.TransferKeystoreRequest:
                    {
                        byte[] data = WhisperService.HexStringToByteArray(internalMessage.data.Substring(2));
                        string textData = Encoding.ASCII.GetString(data);
                        KeyRequestData keyRequestData = JsonConvert.DeserializeObject<KeyRequestData>(textData);
                        if (EncryptionKey.GetPublicAddress() == keyRequestData.EncryptionKeyPublicAddress)
                        {
                            Interlocked.Exchange(ref ConfirmationStatus, 1);
                        }
                        else
                        {
                            Interlocked.Exchange(ref ConfirmationStatus, -1);
                        }
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
            PublicAddressToTransfer = "";
            Interlocked.Exchange(ref ConfirmationPinReceiwed, 0);
            Interlocked.Exchange(ref ConfirmationStatus, 0);
        }

        /// <summary>
        /// Generates temporary key to encrypt keystore that will be transfered and send request to generate the same key on the paired device
        /// </summary>
        /// <returns></returns>
        public async Task<string> GenerateEncryptionKey()
        {
            EncryptionKey = GenerateKey(Encoding.ASCII.GetBytes(OriginalPin));
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(OriginalPin);
            byte[] msgData = new byte[1];
            msgData[0] = 0x0;
            byte[] data = BuildMessage(InternalData.InternalMessageId.GenerateEncryptionKeyRequest, msgData);
            WhisperService.MessageDesc msg = new WhisperService.MessageDesc(SymKeyId, "", "", MessageTimeOut, topic[0], data, "", MaximalProofOfWorkTime, MinimalPowTarget, "");
            return await WhisperService.SendMessage(msg);
        }

        private int SplitMessage(string fullMessage, int chunkSize, ref List<string> outChunks)
        {
            int startIndex = 0;
            if ((startIndex + chunkSize) > fullMessage.Length)
            {
                chunkSize = fullMessage.Length - startIndex;
            }
            string chunk = fullMessage.Substring(startIndex, chunkSize);
            outChunks.Add(chunk);
            startIndex += chunkSize;
            while (startIndex < fullMessage.Length)
            {
                if ((startIndex + chunkSize) > fullMessage.Length)
                {
                    chunkSize = fullMessage.Length - startIndex;
                }
                if (chunkSize > 0)
                {
                    chunk = fullMessage.Substring(startIndex, chunkSize);
                    outChunks.Add(chunk);
                    startIndex += chunkSize;
                }
            }
            return outChunks.Count;
        }

        /// <summary>
        /// Excrypts selected keystore and sends it to applicant
        /// </summary>
        /// <param name="keyStoreData"></param>
        /// <returns></returns>
        public async Task<string> EncryptAndTransferKeystore(string keyStoreData)
        {
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(OriginalPin);
            byte[] encryptedData = Encrypt(EncryptionKey, Encoding.ASCII.GetBytes(keyStoreData));
            string hexStringData = BitConverter.ToString(encryptedData).Replace("-", string.Empty).ToLower();
            int chunks = SplitMessage(hexStringData, ChunkSize, ref MessageChunks);
            Debug.Assert(MessageChunks.Count <= 255);
            byte[] numChunks = new byte[1];
            byte[] id = new byte[1];
            numChunks[0] = (byte)MessageChunks.Count;
            string maxChunks = BitConverter.ToString(numChunks).Replace("-", string.Empty).ToLower();
            for (int i = 0; i < MessageChunks.Count; i++)
            {
                Debug.Print("chunk[" + i.ToString() + "] " + MessageChunks[i]);
                id[0] = (byte)i;
                string chunkId = BitConverter.ToString(id).Replace("-", string.Empty).ToLower();
                byte[] data = BuildMessage(InternalData.InternalMessageId.TransferKeystoreAnswer, Encoding.ASCII.GetBytes("0x" + chunkId + maxChunks + MessageChunks[i]));
                WhisperService.MessageDesc msg = new WhisperService.MessageDesc(SymKeyId, "", "", MessageTimeOut, topic[0], data, "", MaximalProofOfWorkTime, MinimalPowTarget, "");
                string res = await WhisperService.SendMessage(msg);
            }
            return "Message sent";
        }
    }
}
