using System;
using System.Threading.Tasks;

namespace Hoard.HW.Ledger.Ethereum
{
    /// <summary>
    /// Implementation of LedgerWallet class with ethereum compatible signing methods
    /// </summary>
    internal class EthLedgerWallet : LedgerWallet
    {
        private class HDWalletProfile : Profile
        {
            private EthLedgerWallet Wallet;
            public byte[] DerivationData { get; private set; }

            public HDWalletProfile(string name, HoardID id, byte[] data, EthLedgerWallet wallet)
                :base(name, id)
            {
                Wallet = wallet;
                DerivationData = data;
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

        public EthLedgerWallet(Device.Net.IDevice hidDevice, string derivationPath) : base(hidDevice, derivationPath)
        {
            
        }

        public override async Task<Profile> CreateProfile(string name)
        {
            uint accountIndex = 0;
            if (!uint.TryParse(name, out accountIndex))
                throw new ArgumentException("Name argument should be an index of account in range [0-UINT_MAX]!");
            return await RequestProfile(IndexToProfileName(accountIndex));
        }

        public override async Task<Profile> RequestProfile(string name)
        {
            uint accountIndex = ProfileNameToIndex(name);
            var keyPath = new KeyPath(DerivationPath).Derive(accountIndex);
            byte[] derivationData = keyPath.ToBytes();
            var output = await SendRequestAsync(EthGetAddress.Request(derivationData));
            if(IsSuccess(output.StatusCode))
            {
                var address = new HoardID(EthGetAddress.GetAddress(output.Data));
                return new HDWalletProfile(name, address, derivationData, this);
            }

            throw new Exception("HD wallet returned error: " + output.StatusCode);
        }

        private async Task<string> SignTransaction(byte[] rlpEncodedTransaction, HDWalletProfile profile)
        {
            uint txLength = (uint)rlpEncodedTransaction.Length;
            uint bytesToCopy = Math.Min(0xff - (uint)profile.DerivationData.Length, txLength);

            var txChunk = new byte[bytesToCopy];
            Array.Copy(rlpEncodedTransaction, 0, txChunk, 0, bytesToCopy);
            var output = await SendRequestAsync(EthSignTransaction.Request(profile.DerivationData, txChunk, true));

            txLength -= bytesToCopy;
            uint pos = bytesToCopy;
            while (txLength > 0 && IsSuccess(output.StatusCode))
            {
                bytesToCopy = Math.Min(0xff, txLength);
                txChunk = new byte[bytesToCopy];

                Array.Copy(rlpEncodedTransaction, pos, txChunk, 0, bytesToCopy);
                output = await SendRequestAsync(EthSignTransaction.Request(profile.DerivationData, txChunk, false));

                txLength -= bytesToCopy;
                pos += bytesToCopy;
            }

            if (IsSuccess(output.StatusCode))
            {
                return EthSignTransaction.GetSignature(output.Data);
            }

            return null;
        }

        private async Task<string> SignMessage(byte[] message, HDWalletProfile profile)
        {
            uint msgLength = (uint)message.Length;
            uint bytesToCopy = Math.Min(0xff - (uint)profile.DerivationData.Length - sizeof(int), msgLength);

            var messageChunk = new byte[bytesToCopy + sizeof(int)];
            msgLength.ToBytes().CopyTo(messageChunk, 0);

            Array.Copy(message, 0, messageChunk, sizeof(int), bytesToCopy);
            var output = await SendRequestAsync(EthSignMessage.Request(profile.DerivationData, messageChunk, true));

            msgLength -= bytesToCopy;
            uint pos = bytesToCopy;
            while (msgLength > 0 && IsSuccess(output.StatusCode))
            {
                bytesToCopy = Math.Min(0xff, msgLength);
                messageChunk = new byte[bytesToCopy];

                Array.Copy(message, pos, messageChunk, 0, bytesToCopy);
                output = await SendRequestAsync(EthSignMessage.Request(profile.DerivationData, messageChunk, false));

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