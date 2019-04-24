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
        private int keeperPublicKeyReceived;

        private int keystoreReceived;

        private string decryptedKeystoreData;

        private byte[][] encryptedKeystoreData = null;
        private byte[] fullEncryptedKeystoreData = null;

        private byte[] keeperPublicKey = new byte[0];

        /// <summary>
        /// Constructor
        /// </summary>
        public AccountSynchronizerApplicant(string url) : base(url)
        {
            WhisperService = new WhisperService(url);
            decryptedKeystoreData = "";
            Interlocked.Exchange(ref keystoreReceived, 0);
            OnClear();

            GenerateKeyPair();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool KeeperPublicKeyReceived()
        {
            return (Interlocked.CompareExchange(ref keeperPublicKeyReceived, keeperPublicKeyReceived, 0) != 0);
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
                        Interlocked.Exchange(ref keeperPublicKeyReceived, 1);
                    }
                    break;
                case InternalData.InternalMessageId.TransferKeystoreAnswer:
                case InternalData.InternalMessageId.TransferCustomData:
                    {
                        AggregateMessage(internalMessage.data);

                        if (keeperPublicKey != null && fullEncryptedKeystoreData != null)
                        {
                            byte[] decryptedData = DecryptData(keeperPublicKey, fullEncryptedKeystoreData);
                            decryptedKeystoreData = Encoding.UTF8.GetString(decryptedData);
                            Debug.Print("Decrypted Message: " + decryptedKeystoreData);
                            Interlocked.Exchange(ref keystoreReceived, 1);
                        }
                    }
                    break;
                default:
                    break;
            }
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
        /// 
        /// </summary>
        protected override void OnClear()
        {
            Interlocked.Exchange(ref keeperPublicKeyReceived, 0);
            Interlocked.Exchange(ref keystoreReceived, 0);
            decryptedKeystoreData = "";
        }

        /// <summary>
        /// Sends confirmation pin to paired device
        /// </summary>
        /// <returns></returns>
        public async Task<string> SendPublicKey()
        {
            byte[] data = BuildMessage(InternalData.InternalMessageId.ApplicantPublicKey, publicKey);
            return await SendMessage(data);
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
        /// Returns encrypted keystore data
        /// </summary>
        public string GetKeystoreReceivedData()
        {
            return decryptedKeystoreData;
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
