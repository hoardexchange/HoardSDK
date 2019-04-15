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
                case InternalData.InternalMessageId.ConfirmationPin:
                    {
                        ConfirmationPin = Encoding.UTF8.GetString(internalMessage.data);
                        int index = ConfirmationPin.IndexOf("|");
                        string pin = ConfirmationPin.Substring(0, index);
                        mDateTime = ConfirmationPin.Substring(index+1);
                        ConfirmationPin = pin;

                        Interlocked.Exchange(ref ConfirmationPinReceiwed, 1);
                    }
                    break;
                case InternalData.InternalMessageId.TransferKeystoreRequest:
                    {
                        string textData = Encoding.UTF8.GetString(internalMessage.data);
                        KeyRequestData keyRequestData = JsonConvert.DeserializeObject<KeyRequestData>(textData);
                        if (EncryptionKey.GetPublicAddress().ToLower() == keyRequestData.EncryptionKeyPublicAddress.ToLower())
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
            mDateTime = "";
            Interlocked.Exchange(ref ConfirmationPinReceiwed, 0);
            Interlocked.Exchange(ref ConfirmationStatus, 0);
        }

        /// <summary>
        /// Generates temporary key to encrypt keystore that will be transfered and send request to generate the same key on the paired device
        /// </summary>
        /// <returns></returns>
        public async Task<string> GenerateEncryptionKey()
        {
            EncryptionKey = GenerateKey(Encoding.UTF8.GetBytes(OriginalPin + mDateTime));
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(OriginalPin);
            byte[] msgData = new byte[1];
            msgData[0] = 0x0;
            byte[] data = BuildMessage(InternalData.InternalMessageId.GenerateEncryptionKey, msgData);
            WhisperService.MessageDesc msg = new WhisperService.MessageDesc(SymKeyId, "", "", MessageTimeOut, topic[0], data, "", MaximalProofOfWorkTime, MinimalPowTarget, "");
            return await WhisperService.SendMessage(msg);
        }

        private int SplitMessage(byte[] fullMessage, int chunkSize, ref List<byte[]> outChunks)
        {
            int offset = 0;
            if ((offset + chunkSize) > fullMessage.Length)
            {
                chunkSize = fullMessage.Length - offset;
            }
            outChunks.Add(new byte[chunkSize]);
            Buffer.BlockCopy(fullMessage, offset, outChunks[outChunks.Count-1], 0, chunkSize);
            offset += chunkSize;
            while (offset < fullMessage.Length)
            {
                if ((offset + chunkSize) > fullMessage.Length)
                {
                    chunkSize = fullMessage.Length - offset;
                }
                if (chunkSize > 0)
                {
                    outChunks.Add(new byte[chunkSize]);
                    Buffer.BlockCopy(fullMessage, offset, outChunks[outChunks.Count - 1], 0, chunkSize);
                    offset += chunkSize;
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
            byte[] encryptedData = AESEncrypt(EncryptionKey, Encoding.UTF8.GetBytes(keyStoreData), GenerateIV(OriginalPin));
            List<byte[]> chunks = new List<byte[]>();
            SplitMessage(encryptedData, ChunkSize, ref chunks);
            byte[] numChunks = BitConverter.GetBytes(chunks.Count);
            for (int i = 0; i < chunks.Count; i++)
            {
                byte[] chunkId = BitConverter.GetBytes(i);
                byte[] length = BitConverter.GetBytes(chunks[i].Length);
                byte[] dtsData = new byte[chunks[i].Length + 4 + 4 + 4];
                Buffer.BlockCopy(chunkId, 0, dtsData, 0, 4);
                Buffer.BlockCopy(numChunks, 0, dtsData, 4, 4);
                Buffer.BlockCopy(length, 0, dtsData, 8, 4);
                Buffer.BlockCopy(chunks[i], 0, dtsData, 12, chunks[i].Length);
                byte[] data = BuildMessage(InternalData.InternalMessageId.TransferKeystoreAnswer, dtsData);
                WhisperService.MessageDesc msg = new WhisperService.MessageDesc(SymKeyId, "", "", MessageTimeOut, topic[0], data, "", MaximalProofOfWorkTime, MinimalPowTarget, "");
                string res = await WhisperService.SendMessage(msg);
            }
            return "Message sent";
        }

        /// <summary>
        /// Sends custom data
        /// </summary>
        /// <param name="data">Custom data</param>
        /// <returns></returns>
        public async Task<string> SendEncryptedData(byte[] data)
        {
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(OriginalPin);
            byte[] encryptedData = AESEncrypt(EncryptionKey, data, GenerateIV(OriginalPin));
            List<byte[]> chunks = new List<byte[]>();
            SplitMessage(encryptedData, ChunkSize, ref chunks);
            byte[] numChunks = BitConverter.GetBytes(chunks.Count);
            for (int i = 0; i < chunks.Count; i++)
            {
                byte[] chunkId = BitConverter.GetBytes(i);
                byte[] length = BitConverter.GetBytes(chunks[i].Length);
                byte[] dtsData = new byte[chunks[i].Length + 4 + 4 + 4];
                Buffer.BlockCopy(chunkId, 0, dtsData, 0, 4);
                Buffer.BlockCopy(numChunks, 0, dtsData, 4, 4);
                Buffer.BlockCopy(length, 0, dtsData, 8, 4);
                Buffer.BlockCopy(chunks[i], 0, dtsData, 12, chunks[i].Length);
                byte[] pack = BuildMessage(InternalData.InternalMessageId.TransferCustomData, dtsData);
                WhisperService.MessageDesc msg = new WhisperService.MessageDesc(SymKeyId, "", "", MessageTimeOut, topic[0], pack, "", MaximalProofOfWorkTime, MinimalPowTarget, "");
                string res = await WhisperService.SendMessage(msg);
            }
            return "Message sent";
        }
    }
}
