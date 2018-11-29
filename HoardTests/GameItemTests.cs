﻿using Hoard;
using Hoard.BC;
using Hoard.BC.Contracts;
using Hoard.GameItemProviders;
using HoardTests.Fixtures;
using Nethereum.Contracts;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using Newtonsoft.Json;
using System;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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

        public override int GetHashCode()
        {
            var hashCode = -1621467;
            hashCode = hashCode * -1521134295 + Endurance.GetHashCode();
            hashCode = hashCode * -1521134295 + Strength.GetHashCode();
            hashCode = hashCode * -1521134295 + Dexterity.GetHashCode();
            return hashCode;
        }

    }

    public class ERC721GameItemMockContract : ERC721GameItemContract
    {
        public new const string InterfaceID = "0x90ac58cd";

        public static new string ABI = HoardABIConfig.ERC721TokenMockABI;

        public ERC721GameItemMockContract(GameID game, Web3 web3, string address) : base(game, web3, address, ABI) { }
        
        private Function GetFunctionMintToken()
        {
            return contract.GetFunction("mintToken");
        }

        private Function GetFunctionSetTokenState()
        {
            return contract.GetFunction("setTokenState");
        }

        public async Task<TransactionReceipt> MintToken(string ownerAddress, BigInteger tokenID, byte[] tokenState, AccountInfo account)
        {
            Function function = GetFunctionMintToken();
            return await HoardService.Instance.BCComm.EvaluateOnBC(account, function, ownerAddress, tokenID, tokenState);
        }

        public async Task<TransactionReceipt> SetTokenState(BigInteger tokenID, byte[] tokenState, AccountInfo account)
        {
            Function function = GetFunctionSetTokenState();
            return await HoardService.Instance.BCComm.EvaluateOnBC(account, function, tokenID, tokenState);
        }
    }

    public class BCGameItemMockProvider : BCGameItemProvider
    {
        public BCGameItemMockProvider(GameID game, BCComm comm) : base(game, comm)
        {
            RegisterContractInterfaceID(ERC721GameItemMockContract.InterfaceID, typeof(ERC721GameItemMockContract));
        }

        public bool UpdateItemState(GameItem gameItem, AccountInfo account)
        {
            ERC721GameItemMockContract contract = (ERC721GameItemMockContract)BCComm.GetGameItemContract(gameItem.Game, itemContracts[gameItem.Symbol].Address, typeof(ERC721GameItemMockContract));
            if (contract != null)
            {
                contract.SetTokenState(((ERC721GameItemContract.Metadata)gameItem.Metadata).ItemId, gameItem.State, account).Wait();
                return true;
            }
            return false;
        }
    }

    public class GameItemTests : IClassFixture<HoardServiceFixture>, IClassFixture<IPFSFixture>
    {
        static string HoardGameTestName = "HoardGame";

        HoardServiceFixture hoardFixture;
        IPFSFixture ipfsFixture;

        BCGameItemMockProvider gameItemProvider = null;
        User DefaultPlayer = null;

        public GameItemTests(HoardServiceFixture _hoardFixture, IPFSFixture _ipfsFixture)
        {
            hoardFixture = _hoardFixture;
            ipfsFixture = _ipfsFixture;

            hoardFixture.Initialize(HoardGameTestName);

            GameID[] games = hoardFixture.HoardService.QueryHoardGames().Result;
            Assert.NotEmpty(games);

            GameID game = games[0];
            HoardGameItemProvider hoardItemProvider = new HoardGameItemProvider(game);
            gameItemProvider = new BCGameItemMockProvider(game, hoardFixture.HoardService.BCComm);
            hoardItemProvider.SecureProvider = gameItemProvider;
            hoardFixture.HoardService.RegisterGame(game, hoardItemProvider);

            Assert.NotNull(gameItemProvider);

            DefaultPlayer = HoardServiceFixture.UserIDs[2];
        }

        [Fact]
        public void UploadDownloadState()
        {
            GameItem swordItem = new GameItem(new GameID("test"), "TM721", null);
            swordItem.Properties = new SwordProperties(10, 5, 20);

            GameItem[] items = gameItemProvider.GetPlayerItems(DefaultPlayer.ActiveAccount, swordItem.Symbol);
            Assert.Equal(2, items.Length);

            string propsJson = JsonConvert.SerializeObject(swordItem.Properties);
            swordItem.State = ipfsFixture.Client.UploadAsync(Encoding.ASCII.GetBytes(propsJson)).Result;
            swordItem.Metadata = items[0].Metadata;

            gameItemProvider.UpdateItemState(swordItem, DefaultPlayer.ActiveAccount);

            items = gameItemProvider.GetPlayerItems(DefaultPlayer.ActiveAccount, swordItem.Symbol);
            GameItem downloadedSwordItem = items[0];
            hoardFixture.HoardService.FetchItemProperties(downloadedSwordItem);

            Assert.Equal(swordItem.State, downloadedSwordItem.State);

            string downloadedPropsJson = Encoding.ASCII.GetString(ipfsFixture.Client.DownloadBytesAsync(downloadedSwordItem.State).Result);
            SwordProperties downloadedProps = JsonConvert.DeserializeObject<SwordProperties>(downloadedPropsJson);

            Assert.Equal((SwordProperties)swordItem.Properties, downloadedProps);
        }
    }
}
