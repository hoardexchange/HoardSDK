using Hoard;
using Hoard.BC.Contracts;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HoardTests.CryptKittyTests
{
    class CKGameItemProvider : IGameItemProvider
    {
        public class Metadata : BaseGameItemMetadata
        {
            public string OwnerAddress { get; set; }
            public ulong ItemId { get; set; }
            public string TokenStateType { get; set; }

            public Metadata(string ownerAddress, ulong itemID, string tokenStateType)
            {
                OwnerAddress = ownerAddress;
                ItemId = itemID;
                TokenStateType = tokenStateType;
            }
        }

        private class KittyAPIResult
        {
            public string id = null;
            public string image_url = null;
        }

        private class UserKittiesAPIResult
        {
            public List<KittyAPIResult> kitties = null;
        }

        public GameID Game { get; private set; }

        private RestClient Client = null;

        public CKGameItemProvider(GameID game)
        {
            Game = game;
        }

        public async Task<Result> Connect()
        {
            Client = new RestClient("https://api.cryptokitties.co");
            Client.AutomaticDecompression = false;

            return Result.Ok;
        }

        public string[] GetItemTypes()
        {
            return new string[] {"CryptoKitty"};
        }

        public async Task<GameItem[]> GetPlayerItems(Profile profile)
        {
            List<GameItem> items = new List<GameItem>();

            var request = new RestRequest("kitties?owner_wallet_address=" + profile.ID + "&limit=10&offset=0", Method.GET);
            request.AddDecompressionMethod(DecompressionMethods.None);
            var response = await Client.ExecuteTaskAsync(request).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                System.Diagnostics.Debug.Fail("unable to get kitties by owner from " + Client.BaseUrl + ", Request: " + "kitties?owner_wallet_address=" + profile.ID + "&limit=1&offset=0" + ", response: " + response.ErrorMessage + " StatusCode: " + response.StatusCode);
                return null;
            }

            UserKittiesAPIResult userKitties = JsonConvert.DeserializeObject<UserKittiesAPIResult>(response.Content);

            if (userKitties == null)
            {
                System.Diagnostics.Debug.Fail("unable to parse user kitties response from " + Client.BaseUrl + ", Content: " + response.Content);
                return null;
            }

            foreach (var kitty in userKitties.kitties)
            {
                if (await validateOwnerOnBC(profile, kitty.id))
                {
                    GameItem gi = new GameItem(Game, "CK", new Metadata(profile.ID, ulong.Parse(kitty.id), "CK_DNA"));
                    gi.State = Encoding.Unicode.GetBytes(kitty.image_url);//this should be dna!
                }
            }

            return items.ToArray();

        }

        public async Task<GameItemType> GetItemTypeInfo(string type)
        {
            throw new NotImplementedException();
        }

        public async Task<GameItem[]> GetItems(GameItemsParams[] gameItemsParams)
        {
            throw new NotImplementedException();
        }

        public async Task<GameItem[]> GetPlayerItems(Profile profile, string itemType)
        {
            if (itemType == GetItemTypes()[0])
                return await GetPlayerItems(profile);
            throw new ArgumentException();
        }

        private async Task<bool> validateOwnerOnBC(Profile profile, string tokenId)
        {
            BigInteger tokenBigInt = new BigInteger(Encoding.Unicode.GetBytes(tokenId));

            ERC721GameItemContract contract = (ERC721GameItemContract)HoardService.Instance.BCComm.GetGameItemContract(null, "0x06012c8cf97BEaD5deAe237070F9587f8E7A266d", typeof(ERC721GameItemContract));

            BigInteger owner = await contract.OwnerOf(tokenBigInt);
            
            return (profile.ID == owner.ToString());
        }

        public Task<GameItem[]> GetPlayerItems(Profile profile, string itemType, ulong firstItemIndex, ulong itemsToGather)
        {
            throw new NotImplementedException();
        }

        public Task<ulong> GetPlayerItemsAmount(Profile profile, string itemType)
        {
            throw new NotImplementedException();
        }


        public Task<bool> Transfer(Profile from, string addressTo, GameItem item, BigInteger amount)
        {
            throw new NotImplementedException();
        }
    }
}
