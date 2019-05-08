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

        public AccountSynchronizerTest()
        {
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task TransferKey()
        {
            bool res = false;
            AccountSynchronizerKeeper keeper = new AccountSynchronizerKeeper(NodeUrl);
            AccountSynchronizerApplicant applicant = new AccountSynchronizerApplicant(NodeUrl);
            res = await keeper.Initialize();
            Assert.True(res);
            res = await applicant.Initialize();
            Assert.True(res);

            string pin = keeper.PublicKey.ToHex(false).Substring(0, 8);

                string filterApplicant = await applicant.RegisterMessageFilter(pin);
                string filterKeeper = await keeper.RegisterMessageFilter(pin);

                //1. applicant sends its public key Pa
                await applicant.SendPublicKey();
                //2. keeper sends its public key Pk
                await keeper.SendPublicKey();

                //2. keeper waits for applicant's public key Pa
                while (true)
                {
                    await keeper.ProcessMessage(filterKeeper);

                    if (keeper.ApplicantPublicKeyReceived())
                    {
                        break;
                    }
                }
                string hashApplicant = keeper.GetConfirmationHash();

                //3. applicant waits for keeper's public key Pk

                while (true)
                {
                    await applicant.ProcessMessage(filterApplicant);

                    if (applicant.KeeperPublicKeyReceived())
                    {
                        break;
                    }
                }

                string hashKeeper = applicant.GetConfirmationHash();

                Assert.Equal(hashApplicant, hashKeeper);

                //4. keeper sends keystore file
                string keyStoreData = "{'crypto':{'cipher':'aes-128-ctr','ciphertext':'8fe0507d2858178a8832c3b921f994ddb43d3ba727786841d3499b94fdcaaf90','cipherparams':{'iv':'fad9089caee2003792ce6fec6d74f399'},'kdf':'scrypt','mac':'0da29fcf2ccfa9327cd5bb2a5f7e2a4b4a01ab6ba61954b174fdeeae46b228ab','kdfparams':{'n':262144,'r':1,'p':8,'dklen':32,'salt':'472c9a8bb1898a8abacca45ebb560427621004914edb78dfed4f82163d7fd2a2'}},'id':'1543aac7-c474-4819-98ee-af104528a91f','address':'0x167ba0a6918321b69d5792022ccb99dbeeb0f49a','version':3}";
                string retMsg = await keeper.EncryptAndTransferKeystore(Encoding.UTF8.GetBytes(keyStoreData));

                //5. applicant receives keystore
                while (true)
                {
                    await applicant.ProcessMessage(filterApplicant);

                    if (applicant.IsKeyStoreReceived())
                    {
                        break;
                    }
                }

                string data = applicant.GetKeystoreReceivedData();

                Assert.Equal(keyStoreData, data);

                await applicant.UnregisterMessageFilter(filterApplicant);
                await keeper.UnregisterMessageFilter(filterKeeper);

            await applicant.Shutdown();
            await keeper.Shutdown();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task TransferKeyWhisperJS()
        {
            //AccountSynchronizerKeeper AccountSyncKeeper = new AccountSynchronizerKeeper("ws://localhost:8546");
            //bool res = AccountSyncKeeper.Initialize().Result;
            //if (res)
            //{
            //    string msg = "";
            //    string pin = "12345678";// AccountSynchronizer.GeneratePin();
            //    string filterFrom = AccountSyncKeeper.RegisterMessageFilter(pin).Result;
            //    while (true)
            //    {
            //        AccountSyncKeeper.ProcessMessage();
            //        if (AccountSyncKeeper.ConfirmationPinReceived())
            //        {
            //            msg = AccountSyncKeeper.GenerateEncryptionKey().Result;
            //            break;
            //        }
            //        Thread.Sleep(1000);
            //    }

            //    int confirmation = 0;
            //    while (true)
            //    {
            //        AccountSyncKeeper.ProcessMessage();
            //        confirmation = AccountSyncKeeper.GetConfirmationStatus();
            //        if (confirmation != 0)
            //        {
            //            break;
            //        }
            //    }

            //    if (confirmation == 1)
            //    {
            //        string keyStoreData = "{'crypto':{'cipher':'aes-128-ctr','ciphertext':'8fe0507d2858178a8832c3b921f994ddb43d3ba727786841d3499b94fdcaaf90','cipherparams':{'iv':'fad9089caee2003792ce6fec6d74f399'},'kdf':'scrypt','mac':'0da29fcf2ccfa9327cd5bb2a5f7e2a4b4a01ab6ba61954b174fdeeae46b228ab','kdfparams':{'n':262144,'r':1,'p':8,'dklen':32,'salt':'472c9a8bb1898a8abacca45ebb560427621004914edb78dfed4f82163d7fd2a2'}},'id':'1543aac7-c474-4819-98ee-af104528a91f','address':'0x167ba0a6918321b69d5792022ccb99dbeeb0f49a','version':3}";
            //        msg = AccountSyncKeeper.EncryptAndTransferKeystore(keyStoreData).Result;
            //    }

            //    await AccountSyncKeeper.UnregisterMessageFilter(filterFrom);
            //}
            //await AccountSyncKeeper.Shutdown();
        }

        [Fact]
        [Trait("Category", "Unit")]
        public async Task ReceiveKeyWhisperJS()
        {
            //AccountSynchronizerApplicant AccountSyncApplicant = new AccountSynchronizerApplicant(NodeUrl);
            //bool res = AccountSyncApplicant.Initialize().Result;
            //if (res)
            //{
            //    string pin = "12345678";// AccountSynchronizer.GeneratePin();
            //    string filterTo = AccountSyncApplicant.RegisterMessageFilter(pin).Result;
            //    string confirmationPin = "12345678";// AccountSynchronizer.GeneratePin();
            //    string msg = AccountSyncApplicant.SendConfirmationPin(confirmationPin).Result;
            //    while (true)
            //    {
            //        AccountSyncApplicant.ProcessMessage();
            //        if (AccountSyncApplicant.IsKeyStoreReceived())
            //        {
            //            string data = AccountSyncApplicant.GetKeystoreReceivedData();
            //            break;
            //        }
            //    }
            //    await AccountSyncApplicant.UnregisterMessageFilter(filterTo);
            //}
            //await AccountSyncApplicant.Shutdown();
        }
    }
}
