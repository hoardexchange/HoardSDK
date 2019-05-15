using Device.Net;
using Hid.Net;
using System;
using System.Threading.Tasks;

namespace Hoard.HW.Trezor.Ethereum
{
    /// <summary>
    /// Implementation of TrezorWallet class with ethereum compatible signing methods
    /// </summary>
    internal class EthTrezorWallet : TrezorWallet
    {
        //TODO: Profile should keep derivation and key path, wallet should not have any account related state!
        private class HDWalletProfile : Profile
        {
            private EthTrezorWallet Wallet;

            public HDWalletProfile(string name, HoardID id, EthTrezorWallet wallet)
                : base(name, id)
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
        private KeyPath keyPath;
        private byte[] derivation;
        private uint[] indices;

        //TODO: Profile should keep derivation and key path, wallet should not have any account related state!
        public EthTrezorWallet(IDevice hidDevice, string derivationPath, IUserInputProvider pinInputProvider, uint index = 0) 
            : base(hidDevice, derivationPath, pinInputProvider)
        {
            keyPath = new KeyPath(derivationPath).Derive(index);
            indices = keyPath.Indices;
            derivation = keyPath.ToBytes();
        }

        public override async Task<Profile> RequestProfile(string name)
        {
            var output = await SendRequestAsync(EthGetAddress.Request(indices));
            var address = new HoardID(EthGetAddress.GetAddress(output));
            return new HDWalletProfile(name, address, this);
        }

        private async Task<string> SignTransaction(byte[] rlpEncodedTransaction, Profile profile)
        {
            var output = await SendRequestAsync(EthSignTransaction.Request(indices, rlpEncodedTransaction));
            return EthSignTransaction.GetRLPEncoded(output, rlpEncodedTransaction);
        }

        private async Task<string> SignMessage(byte[] message, Profile profile)
        {
            var output = await SendRequestAsync(EthSignMessage.Request(indices, message));
            return EthSignMessage.GetRLPEncoded(output, message);
        }
    }
}
