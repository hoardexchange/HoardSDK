using Newtonsoft.Json;
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
    public class AccountSynchronizerKeeper : AccountSynchronizer
    {
        private int ConfirmationPinReceiwed;

        /// <summary>
        /// Constructor
        /// </summary>
        public AccountSynchronizerKeeper(string url) : base(url)
        {
            WhisperService = new WhisperService(url);
            ConfirmationPin = "";
            Interlocked.Exchange(ref ConfirmationPinReceiwed, 0);
        }

        /// <summary>
        /// Checks if key keeper received confirmation pin
        /// </summary>
        /// <returns></returns>
        public bool ConfirmationPinReceived()
        {
            return (Interlocked.CompareExchange(ref ConfirmationPinReceiwed, ConfirmationPinReceiwed, 0) != 0);
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
                        ConfirmationPin = internalMessage.data;
                        if (Interlocked.CompareExchange(ref ConfirmationPinReceiwed, ConfirmationPinReceiwed, 0) == 0)
                        {
                            Interlocked.Increment(ref ConfirmationPinReceiwed);
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
            Interlocked.Exchange(ref ConfirmationPinReceiwed, 0);
        }

        /// <summary>
        /// Generates temporary key to encrypt keystore that will be transfered and send request to generate the same key on the paired device
        /// </summary>
        /// <param name="originalPin">Pin</param>
        /// <returns></returns>
        public async Task<string> GenerateEncryptionKey(string originalPin)
        {
            string[] topic = new string[1];
            topic[0] = ConvertPinToTopic(originalPin);
            byte[] msgData = new byte[1];
            msgData[0] = 0x0;
            byte[] data = BuildMessage(InternalData.InternalMessageId.ConfirmationPin, msgData);
            WhisperService.MessageDesc msg = new WhisperService.MessageDesc(SymKeyId, "", "", 7, topic[0], data, "", 2, 2.01f, "");
            return await WhisperService.SendMessage(msg);
        }
    }
}
