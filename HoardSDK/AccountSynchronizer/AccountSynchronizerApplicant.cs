using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
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
        /// <summary>
        /// Constructor
        /// </summary>
        public AccountSynchronizerApplicant(string url) : base(url)
        {
            WhisperService = new WhisperService(url);
            ConfirmationPin = "";
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
        /// <param name="originalPin">Pin</param>
        /// <param name="confirmationPin">Confirmation Pin</param>
        /// <returns></returns>
        public async Task<string> SendConfirmationPin(string originalPin, string confirmationPin)
        {
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(originalPin);
            byte[] data = BuildMessage(InternalData.InternalMessageId.ConfirmationPin, Encoding.ASCII.GetBytes(confirmationPin));
            WhisperService.MessageDesc msg = new WhisperService.MessageDesc(SymKeyId, "", "", 7, topic[0], data, "", 2, 2.01f, "");
            return await WhisperService.SendMessage(msg);
        }

        /// <summary>
        /// Generates temporary key to encrypt keystore that will be transfered and send request to generate the same key on the paired device
        /// </summary>
        /// <param name="originalPin">Pin</param>
        /// <returns></returns>
        public async Task GenerateEncryptionKey(string originalPin)
        {
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(originalPin);
        }
    }
}
