using Nethereum.Hex.HexConvertors.Extensions;
using System;
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
    public class AccountSynchronizerApplicant : AccountSynchronizer
    {
        private int senderKeyReceived;

        private int keystoreReceived;

        /// <summary>
        /// Decrypted key store data
        /// </summary>
        public string DecryptedKeystoreData { get; private set; } = string.Empty;

        private byte[][] encryptedKeystoreData = null;
        private byte[] fullEncryptedKeystoreData = null;

        private byte[] keeperPublicKey = new byte[0];

        /// <summary>
        /// Constructor
        /// </summary>
        public AccountSynchronizerApplicant(IWebSocketProvider webSocketProvider) : base(webSocketProvider)
        {
            Interlocked.Exchange(ref keystoreReceived, 0);
            OnClear();
        }
        
        /// <summary>
        /// 
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
                case InternalData.InternalMessageId.KeeperPublicKey:
                    {
                        keeperPublicKey = internalMessage.data;
                        Interlocked.Exchange(ref senderKeyReceived, 1);
                    }
                    break;
                case InternalData.InternalMessageId.TransferKeystoreAnswer:
                case InternalData.InternalMessageId.TransferCustomData:
                    {
                        AggregateMessage(internalMessage.data);

                        if (keeperPublicKey != null && fullEncryptedKeystoreData != null)
                        {
                            byte[] decryptedData = DecryptData(keeperPublicKey, fullEncryptedKeystoreData);
                            DecryptedKeystoreData = Encoding.UTF8.GetString(decryptedData);
                            Debug.Print("Decrypted Message: " + DecryptedKeystoreData);
                            Interlocked.Exchange(ref keystoreReceived, 1);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Waits for keystore data from keeper
        /// </summary>
        /// <returns>encrypted keystore data</returns>
        public async Task<string> AcquireKeystoreData(CancellationToken token)
        {
            while (!IsKeyStoreReceived())
            {
                var msg = await WhisperService.ReceiveMessages(token);
                TranslateMessage(msg);
            }
            return DecryptedKeystoreData;
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

        private void AggregateMessage(byte[] data)
        {
            UInt32 id = BitConverter.ToUInt32(data, 0);
            UInt32 chunks = BitConverter.ToUInt32(data, 4);
            UInt32 length = BitConverter.ToUInt32(data, 8);
            if (encryptedKeystoreData == null)
            {
                Debug.Assert(chunks > 0);
                encryptedKeystoreData = new byte[chunks][];
            }
            Debug.Assert(id < encryptedKeystoreData.Length);
            encryptedKeystoreData[id] = new byte[length];
            Buffer.BlockCopy(data, 12, encryptedKeystoreData[id], 0, encryptedKeystoreData[id].Length);

            bool messageNotFinished = false;
            int fullDataSize = 0;
            for (int i = 0; i < encryptedKeystoreData.Length; i++)
            {
                if (encryptedKeystoreData[i] == null)
                {
                    messageNotFinished = true;
                    break;
                }
                else
                {
                    Debug.Assert(encryptedKeystoreData[i] != null);
                    fullDataSize += encryptedKeystoreData[i].Length;
                }
            }

            if (messageNotFinished)
            {
                return;
            }

            fullEncryptedKeystoreData = new byte[fullDataSize];
            int offset = 0;
            for (int i = 0; i < encryptedKeystoreData.Length; i++)
            {
                Buffer.BlockCopy(encryptedKeystoreData[i], 0, fullEncryptedKeystoreData, offset, encryptedKeystoreData[i].Length);
                offset += encryptedKeystoreData[i].Length;
            }
        }

        /// <summary>
        /// Clears internal state
        /// </summary>
        protected override void OnClear()
        {
            Interlocked.Exchange(ref senderKeyReceived, 0);
            Interlocked.Exchange(ref keystoreReceived, 0);
            DecryptedKeystoreData = string.Empty;
        }

        /// <summary>
        /// Sends confirmation pin to paired device
        /// </summary>
        /// <returns></returns>
        public async Task<string> SendPublicKey(CancellationToken ctoken)
        {
            byte[] data = BuildMessage(InternalData.InternalMessageId.ApplicantPublicKey, publicKey);
            return await SendMessage(data, ctoken);
        }

        /// <summary>
        /// Checks if keystore was received
        /// </summary>
        /// <returns></returns>
        public bool IsKeyStoreReceived()
        {
            return (Interlocked.CompareExchange(ref keystoreReceived, keystoreReceived, 0) != 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetConfirmationHash()
        {
            SHA256 sha256 = new SHA256Managed();
            byte[] hash = sha256.ComputeHash(GetSymmetricKey(keeperPublicKey));
            return hash.ToHex(false).Substring(0, 10);
        }
    }
}
