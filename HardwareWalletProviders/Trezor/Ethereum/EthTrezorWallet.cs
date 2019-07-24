using Device.Net;
using System;
using System.Threading.Tasks;

namespace Hoard.HW.Trezor.Ethereum
{
    /// <summary>
    /// Implementation of TrezorWallet class with ethereum compatible signing methods
    /// </summary>
    internal class EthTrezorWallet : TrezorWallet
    {
        private class HDWalletProfile : Profile
        {
            private EthTrezorWallet Wallet;
            public uint[] DerivationIndices { get; private set; }

            public HDWalletProfile(string name, HoardID id, uint[] indices, EthTrezorWallet wallet)
                : base(name, id)
            {
                Wallet = wallet;
                DerivationIndices = indices;
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

        public EthTrezorWallet(IDevice hidDevice, string derivationPath, IUserInputProvider pinInputProvider) 
            : base(hidDevice, derivationPath, pinInputProvider)
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

            var output = await SendRequestAsync(EthGetAddress.Request(keyPath.Indices));
            var address = new HoardID(EthGetAddress.GetAddress(output));
            return new HDWalletProfile(name, address, keyPath.Indices, this);
        }

        private async Task<string> SignTransaction(byte[] rlpEncodedTransaction, HDWalletProfile profile)
        {
            var output = await SendRequestAsync(EthSignTransaction.Request(profile.DerivationIndices, rlpEncodedTransaction));
            return EthSignTransaction.GetSignature(output);
        }

        private async Task<string> SignMessage(byte[] message, HDWalletProfile profile)
        {
            var output = await SendRequestAsync(EthSignMessage.Request(profile.DerivationIndices, message));
            return EthSignMessage.GetRLPEncoded(output, message);
        }
    }
}
