using Hid.Net;
using System;
using System.Threading.Tasks;

namespace Hoard.HW.Ledger.Ethereum
{
    /// <summary>
    /// Implementation of LedgerWallet class with ethereum compatible signing methods
    /// </summary>
    internal class EthLedgerWallet : LedgerWallet
    {
        //TODO: Profile should keep derivation and key path, wallet should not have any account related state!
        private class HDWalletProfile : Profile
        {
            private EthLedgerWallet Wallet;

            public HDWalletProfile(string name, HoardID id, EthLedgerWallet wallet)
                :base(name, id)
            {
                Wallet = wallet;
            }

            public override async Task<string> SignMessage(byte[] input)
            {
                return await Wallet.SignMessage(input, this);
            }

            public override async Task<string> SignTransaction(byte[] input)
            {
                return await Wallet.SignTransaction(input, this);
            }
        }

        /// <summary>
        /// TODO: move this to profile
        /// </summary>
        private KeyPath keyPath;        
        /// <summary>
        /// TODO: move this to profile
        /// </summary>
        private byte[] derivation;

        //TODO: Profile should keep derivation and key path and index , wallet should not have any account related state!
        public EthLedgerWallet(Device.Net.IDevice hidDevice, string derivationPath, uint index = 0) : base(hidDevice, derivationPath)
        {
            keyPath = new KeyPath(derivationPath).Derive(index);
            derivation = keyPath.ToBytes();
        }

        public override async Task<Profile> RequestProfile(string name)
        {
            var output = await SendRequestAsync(EthGetAddress.Request(derivation));
            if(IsSuccess(output.StatusCode))
            {
                var address = new HoardID(EthGetAddress.GetAddress(output.Data));
                return new HDWalletProfile(AccountInfoName, address, this);
            }

            return null;
        }

        public override async Task<string> SignTransaction(byte[] rlpEncodedTransaction, Profile profile)
        {
            uint txLength = (uint)rlpEncodedTransaction.Length;
            uint bytesToCopy = Math.Min(0xff - (uint)derivation.Length, txLength);

            var txChunk = new byte[bytesToCopy];
            Array.Copy(rlpEncodedTransaction, 0, txChunk, 0, bytesToCopy);
            var output = await SendRequestAsync(EthSignTransaction.Request(derivation, txChunk, true));

            txLength -= bytesToCopy;
            uint pos = bytesToCopy;
            while (txLength > 0 && IsSuccess(output.StatusCode))
            {
                bytesToCopy = Math.Min(0xff, txLength);
                txChunk = new byte[bytesToCopy];

                Array.Copy(rlpEncodedTransaction, pos, txChunk, 0, bytesToCopy);
                output = await SendRequestAsync(EthSignTransaction.Request(derivation, txChunk, false));

                txLength -= bytesToCopy;
                pos += bytesToCopy;
            }

            if (IsSuccess(output.StatusCode))
            {
                return EthSignTransaction.GetRLPEncoded(output.Data, rlpEncodedTransaction);
            }

            return null;
        }

        public override async Task<string> SignMessage(byte[] message, Profile profile)
        {
            uint msgLength = (uint)message.Length;
            uint bytesToCopy = Math.Min(0xff - (uint)derivation.Length - sizeof(int), msgLength);

            var messageChunk = new byte[bytesToCopy + sizeof(int)];
            msgLength.ToBytes().CopyTo(messageChunk, 0);

            Array.Copy(message, 0, messageChunk, sizeof(int), bytesToCopy);
            var output = await SendRequestAsync(EthSignMessage.Request(derivation, messageChunk, true));

            msgLength -= bytesToCopy;
            uint pos = bytesToCopy;
            while (msgLength > 0 && IsSuccess(output.StatusCode))
            {
                bytesToCopy = Math.Min(0xff, msgLength);
                messageChunk = new byte[bytesToCopy];

                Array.Copy(message, pos, messageChunk, 0, bytesToCopy);
                output = await SendRequestAsync(EthSignMessage.Request(derivation, messageChunk, false));

                msgLength -= bytesToCopy;
                pos += bytesToCopy;
            }

            if (IsSuccess(output.StatusCode))
            {
                return EthSignMessage.GetStringSignature(output.Data);
            }

            return null;
        }
    }
}