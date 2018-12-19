using Hid.Net;
using System;
using System.Threading.Tasks;

namespace Hoard.HW.Trezor.Ethereum
{
    public class EthTrezorWallet : TrezorWallet
    {
        private class HDWalletAccountInfo : AccountInfo
        {
            private EthTrezorWallet Wallet;

            public HDWalletAccountInfo(string name, HoardID id, EthTrezorWallet wallet)
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

        public EthTrezorWallet(IHidDevice hidDevice, string derivationPath, IUserInputProvider pinInputProvider, uint index = 0) 
            : base(hidDevice, derivationPath, pinInputProvider)
        {
            keyPath = new KeyPath(derivationPath).Derive(index);
            indices = keyPath.Indices;
            derivation = keyPath.ToBytes();
        }

        public override async Task<bool> RequestAccounts(User user)
        {
            var output = await SendRequestAsync(EthGetAddress.Request(indices));
            var address = new HoardID(EthGetAddress.GetAddress(output));
            user.Accounts.Add(new HDWalletAccountInfo(AccountInfoName, address, this));
            return true;
        }

        public override async Task<bool> SetActiveAccount(User user, AccountInfo account)
        {
            return await Task.Run(() =>
            {
                return user.SetActiveAccount(account);
            });
        }

        public override async Task<string> SignTransaction(byte[] rlpEncodedTransaction, AccountInfo accountInfo)
        {
            var output = await SendRequestAsync(EthSignTransaction.Request(indices, rlpEncodedTransaction));
            return EthSignTransaction.GetRLPEncoded(output, rlpEncodedTransaction);
        }

        public override async Task<string> SignMessage(byte[] message, AccountInfo accountInfo)
        {
            var output = await SendRequestAsync(EthSignMessage.Request(indices, message));
            return EthSignMessage.GetRLPEncoded(output, message);
        }
    }
}
