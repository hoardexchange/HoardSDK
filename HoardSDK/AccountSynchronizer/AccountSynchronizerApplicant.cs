using Hoard.Utils.Base58Check;
using Nethereum.Signer;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Class for transfer and synchronize accounts between different devices
    /// </summary>
    public class AccountSynchronizerApplicant : AccountSynchronizer
    {
        private EthECKey DecryptionKey;

        /// <summary>
        /// Constructor
        /// </summary>
        public AccountSynchronizerApplicant(string url) : base(url)
        {
            WhisperService = new WhisperService(url);
            ConfirmationPin = "";
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

        private void DecryptKeystore(EthECKey key, string data)
        {
            BigInteger privKeyInt = new BigInteger(key.GetPrivateKeyAsBytes());
            X9ECParameters ecParams = ECNamedCurveTable.GetByName("Secp256k1");
            ECDomainParameters ecSpec = new ECDomainParameters(ecParams.Curve, ecParams.G, ecParams.N, ecParams.H);
            ECPrivateKeyParameters privKeyParam = new ECPrivateKeyParameters(privKeyInt, ecSpec);
            byte[] decrypted = Decrypt(WhisperService.HexStringToByteArray(data), privKeyParam);
            string decryptedData = Encoding.ASCII.GetString(decrypted);
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
                        DecryptKeystore(DecryptionKey, internalMessage.data);
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
    }
}
