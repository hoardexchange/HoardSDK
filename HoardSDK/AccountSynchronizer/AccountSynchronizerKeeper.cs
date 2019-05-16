using Nethereum.Hex.HexConvertors.Extensions;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Class for transfer and synchronize accounts between different devices
    /// </summary>
    public class AccountSynchronizerKeeper : AccountSynchronizer
    {
        private int senderKeyReceived;

        private byte[] applicantPublicKey = new byte[0];
        
        /// <summary>
        /// Constructor
        /// </summary>
        public AccountSynchronizerKeeper(IWebSocketProvider webSocketProvider) : base(webSocketProvider)
        {
            Interlocked.Exchange(ref senderKeyReceived, 0);
        }

        /// <summary>
        /// Checks if key keeper received confirmation pin
        /// </summary>
        /// <returns></returns>
        public bool IsSenderKeyReceived()
        {
            return (Interlocked.CompareExchange(ref senderKeyReceived, senderKeyReceived, 0) != 0);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="internalMessage"></param>
        protected override void OnTranslateMessage(InternalData internalMessage)
        {
            switch (internalMessage.id)
            {
                case InternalData.InternalMessageId.ApplicantPublicKey:
                    {
                        applicantPublicKey = internalMessage.data;
                        Interlocked.Exchange(ref senderKeyReceived, 1);
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Waits for public key from keeper and returns confirmation hash
        /// </summary>
        /// <returns>confirmation hash</returns>
        public async Task<string> AcquireConfirmationHash(CancellationToken token)
        {
            while (!IsSenderKeyReceived())
            {
                var msg = await WhisperService.ReceiveMessages(token);
                TranslateMessage(msg);
            }
            return GetConfirmationHash();
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnClear()
        {
            Interlocked.Exchange(ref senderKeyReceived, 0);
        }

        /// <summary>
        /// Sends exchange key
        /// </summary>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<string> SendPublicKey(CancellationToken ctoken)
        {
            byte[] data = BuildMessage(InternalData.InternalMessageId.KeeperPublicKey, publicKey);
            return await SendMessage(data, ctoken);
        }

        /// <summary>
        /// Excrypts selected keystore and sends it to applicant
        /// </summary>
        /// <param name="keyStoreData"></param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<string> EncryptAndTransferKeystore(byte[] keyStoreData, CancellationToken ctoken)
        {
            return await SendEncryptedData(InternalData.InternalMessageId.TransferKeystoreAnswer, keyStoreData, ctoken);
        }

        /// <summary>
        /// Sends custom data
        /// </summary>
        /// <param name="customData">Custom data</param>
        /// <param name="ctoken">cancellation token</param>
        /// <returns></returns>
        public async Task<string> SendEncryptedData(byte[] customData, CancellationToken ctoken)
        {
            return await SendEncryptedData(InternalData.InternalMessageId.TransferCustomData, customData, ctoken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetConfirmationHash()
        {
            SHA256 sha256 = new SHA256Managed();
            byte[] hash = sha256.ComputeHash(GetSymmetricKey(applicantPublicKey));
            return hash.ToHex(false).Substring(0, 10);
        }

        private async Task<string> SendEncryptedData(InternalData.InternalMessageId messageId, byte[] customData, CancellationToken ctoken)
        {
            byte[] encryptedData = EncryptData(applicantPublicKey, customData);

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
                byte[] data = BuildMessage(messageId, dtsData);

                await SendMessage(data, ctoken);
            }
            return "Message sent";
        }

        private int SplitMessage(byte[] fullMessage, int chunkSize, ref List<byte[]> outChunks)
        {
            int offset = 0;
            if ((offset + chunkSize) > fullMessage.Length)
            {
                chunkSize = fullMessage.Length - offset;
            }
            outChunks.Add(new byte[chunkSize]);
            Buffer.BlockCopy(fullMessage, offset, outChunks[outChunks.Count - 1], 0, chunkSize);
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
    }
}