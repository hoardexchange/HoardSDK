using Hoard;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Nethereum.Hex.HexConvertors.Extensions;

namespace HoardTests.AccountTools
{
    /// <summary>
    ///  geth parameters
    ///  geth --identity "MyTestNetNode" --nodiscover --datadir "d:\hoard" --wsorigins="*" --ws --wsport "8546" --wsaddr 0.0.0.0 --networkid 15 --shh --rpc --rpcport "8545" --rpcapi personal,db,eth,net,web3,shh console
    /// </summary>
    public class AccountSynchronizerTest
    {
        private static readonly string NodeUrl = "ws://ws.eth-rpc.hoard.exchange";
        private static readonly string TestPIN = "1a3b5c78";

        public AccountSynchronizerTest()
        {
            ErrorCallbackProvider.OnReportInfo += (msg) =>
              {
                  System.Diagnostics.Debug.Print("INFO: "+msg);
              };
            ErrorCallbackProvider.OnReportWarning += (msg) =>
            {
                System.Diagnostics.Debug.Print("WARN: "+msg);
            };
            ErrorCallbackProvider.OnReportError += (msg) =>
            {
                System.Diagnostics.Debug.Print("ERROR: "+msg);
            };
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task TransferKey()
        {
            bool res = false;
            AccountSynchronizerKeeper keeper = new AccountSynchronizerKeeper(NodeUrl);
            AccountSynchronizerApplicant applicant = new AccountSynchronizerApplicant(NodeUrl);

            string pin = keeper.PublicKey.ToHex(false).Substring(0, 8);

            res = await keeper.Initialize(pin);//subscribe
            Assert.True(res);
            res = await applicant.Initialize(pin);//subscribe
            Assert.True(res);
            
            //1. applicant sends its public key Pa
            await applicant.SendPublicKey();
            //2. keeper sends its public key Pk
            await keeper.SendPublicKey();

            //3. wait for confirmations
            string hashKeeper = await keeper.AcquireConfirmationHash();
            string hashApplicant = await applicant.AcquireConfirmationHash();
            

            Assert.Equal(hashApplicant, hashKeeper);

            //4. keeper sends keystore file
            string keyStoreData = "{'crypto':{'cipher':'aes-128-ctr','ciphertext':'8fe0507d2858178a8832c3b921f994ddb43d3ba727786841d3499b94fdcaaf90','cipherparams':{'iv':'fad9089caee2003792ce6fec6d74f399'},'kdf':'scrypt','mac':'0da29fcf2ccfa9327cd5bb2a5f7e2a4b4a01ab6ba61954b174fdeeae46b228ab','kdfparams':{'n':262144,'r':1,'p':8,'dklen':32,'salt':'472c9a8bb1898a8abacca45ebb560427621004914edb78dfed4f82163d7fd2a2'}},'id':'1543aac7-c474-4819-98ee-af104528a91f','address':'0x167ba0a6918321b69d5792022ccb99dbeeb0f49a','version':3}";
            string retMsg = await keeper.EncryptAndTransferKeystore(Encoding.UTF8.GetBytes(keyStoreData));

            //5. applicant receives keystore
            string data = await applicant.AcquireKeystoreData();

            Assert.Equal(keyStoreData, data);

            await keeper.Shutdown();
            await applicant.Shutdown();            
        }

        [Fact]
        //[Trait("Category", "Unit")]
        public async Task TransferKeyWhisperJS()
        {
            AccountSynchronizerKeeper keeper = new AccountSynchronizerKeeper("ws://localhost:8546");
            bool res = await keeper.Initialize(TestPIN);
            if (res)
            {
                await keeper.SendPublicKey();
                string confirmationHash = await keeper.AcquireConfirmationHash();

                if (!string.IsNullOrEmpty(confirmationHash))
                {
                    string keyStoreData = "{'crypto':{'cipher':'aes-128-ctr','ciphertext':'8fe0507d2858178a8832c3b921f994ddb43d3ba727786841d3499b94fdcaaf90','cipherparams':{'iv':'fad9089caee2003792ce6fec6d74f399'},'kdf':'scrypt','mac':'0da29fcf2ccfa9327cd5bb2a5f7e2a4b4a01ab6ba61954b174fdeeae46b228ab','kdfparams':{'n':262144,'r':1,'p':8,'dklen':32,'salt':'472c9a8bb1898a8abacca45ebb560427621004914edb78dfed4f82163d7fd2a2'}},'id':'1543aac7-c474-4819-98ee-af104528a91f','address':'0x167ba0a6918321b69d5792022ccb99dbeeb0f49a','version':3}";
                    await keeper.EncryptAndTransferKeystore(Encoding.UTF8.GetBytes(keyStoreData));
                }
            }
            await keeper.Shutdown();
        }

        [Fact]
        //[Trait("Category", "Unit")]
        public async Task ReceiveKeyWhisperJS()
        {
            AccountSynchronizerApplicant applicant = new AccountSynchronizerApplicant(NodeUrl);
            bool res = await applicant.Initialize(TestPIN);
            if (res)
            {
                await applicant.SendPublicKey();
                string confirmationHash = await applicant.AcquireConfirmationHash();
                string keystore = await applicant.AcquireKeystoreData();
            }
            await applicant.Shutdown();
        }
    }
}
