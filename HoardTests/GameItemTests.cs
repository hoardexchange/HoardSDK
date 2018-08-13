using Hoard;
using Hoard.BC.Contracts;
using Hoard.DistributedStorage;
using HoardTests.Fixtures;
using Nethereum.Contracts;
using Nethereum.Web3;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Numerics;
using System;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.JsonRpc.Client;
using Nethereum.Hex.HexTypes;

namespace HoardTests
{
    public class SwordProperties : ItemProperties, IEquatable<SwordProperties>
    {
        public UInt32 Endurance { get { return UInt32.Parse(this.GetItemProperty("endurance").Value.ToString()); } }
        public UInt32 Strength { get { return UInt32.Parse(this.GetItemProperty("strength").Value.ToString()); } }
        public UInt32 Dexterity { get { return UInt32.Parse(this.GetItemProperty("dexterity").Value.ToString()); } }

        public SwordProperties()
        {
        }

        public SwordProperties(uint endurance, uint strength, uint dexterity)
        {
            Add("endurance", endurance, "uint32");
            Add("strength", strength, "uint32");
            Add("dexterity", dexterity, "uint32");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SwordProperties)obj);
        }

        public bool Equals(SwordProperties other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return this.Endurance.Equals(other.Endurance) &&
                this.Strength.Equals(other.Strength) &&
                this.Dexterity.Equals(other.Dexterity);
        }
    }

    public class TestGameERC721TokenContract : ERC721GameItemContract
    {
        public static string ABI = "";

        public TestGameERC721TokenContract(Web3 web3, string address) : base(web3, address, ABI) { }
        
        private Function GetFunctionMintToken()
        {
            return contract.GetFunction("mintToken");
        }

        public Task<TransactionReceipt> MintToken(string ownerAddress, BigInteger tokenID, byte[] tokenState)
        {
            Function function = GetFunctionMintToken();
            HexBigInteger gas = function.EstimateGasAsync(ownerAddress, new HexBigInteger(300000), new HexBigInteger(0), ownerAddress, tokenID, tokenState).Result;
            return function.SendTransactionAndWaitForReceiptAsync(ownerAddress, gas, new HexBigInteger(0), null, ownerAddress, tokenID, tokenState);
        }
    }

    public class GameItemTests : IClassFixture<HoardServiceFixture>, IClassFixture<IPFSFixture>
    {
        HoardServiceFixture hoardFixture;
        IPFSFixture ipfsFixture;

        HoardService hoardService;
        IPFSClient client;

        TestGameERC721TokenContract erc721Contract;

        public GameItemTests(HoardServiceFixture hoardFixture, IPFSFixture ipfsFixture)
        {
            this.hoardFixture = hoardFixture;
            this.ipfsFixture = ipfsFixture;

            this.hoardService = hoardFixture.HoardService;
            this.client = ipfsFixture.Client;

            TestGameERC721TokenContract.ABI = hoardFixture.DeployedABIs["TestGameERC721Token"];
            erc721Contract = hoardService.BCComm.GetContract<TestGameERC721TokenContract>(hoardFixture.DeployedAddresses["TestGameERC721Token"]);
        }

        [Fact]
        public void UploadDownloadState()
        {
            SwordProperties props = new SwordProperties(10, 5, 20);
            string ownerAddress = hoardFixture.Accounts[3];
            BigInteger tokenID = new BigInteger(12345);

            string propsJson = JsonConvert.SerializeObject(props);
            byte[] ipfsAddress = client.UploadAsync(Encoding.ASCII.GetBytes(propsJson)).Result;

            Assert.Equal(erc721Contract.GetBalanceOf(ownerAddress).Result, new BigInteger(0));

            TransactionReceipt receipt = erc721Contract.MintToken(ownerAddress, tokenID, ipfsAddress).Result;

            Assert.Equal(erc721Contract.GetBalanceOf(ownerAddress).Result, new BigInteger(1));

            byte[] tokenState = erc721Contract.GetTokenState(tokenID).Result;

            Assert.Equal(tokenState, ipfsAddress);

            string downloadedPropsJson = Encoding.ASCII.GetString(client.DownloadBytesAsync(ipfsAddress).Result);
            SwordProperties downloadedProps = JsonConvert.DeserializeObject<SwordProperties>(downloadedPropsJson);

            Assert.Equal(props, downloadedProps);
        }
    }
}
