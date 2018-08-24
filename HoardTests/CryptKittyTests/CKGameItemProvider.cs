﻿using Hoard;
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

        public bool Connect()
        {
            Client = new RestClient("https://api.cryptokitties.co");
            Client.AutomaticDecompression = false;

            return true;
        }

        public string[] GetItemTypes()
        {
            return new string[] {"CryptoKitty"};
        }

        public GameItem[] GetPlayerItems(PlayerID playerID)
        {
            List<GameItem> items = new List<GameItem>();

            var request = new RestRequest("kitties?owner_wallet_address=" + playerID.ID + "&limit=10&offset=0", Method.GET);
            request.AddDecompressionMethod(System.Net.DecompressionMethods.None);
            var response = Client.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                System.Diagnostics.Debug.Fail("unable to get kitties by owner from " + Client.BaseUrl + ", Request: " + "kitties?owner_wallet_address=" + playerID.ID + "&limit=1&offset=0" + ", response: " + response.ErrorMessage + " StatusCode: " + response.StatusCode);
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
                if (validateOwnerOnBC(playerID, kitty.id))
                {
                    GameItem gi = new GameItem(Game, "CK", new Metadata(playerID.ID, ulong.Parse(kitty.id), "CK_DNA"));
                    gi.State = Encoding.Unicode.GetBytes(kitty.image_url);//this should be dna!
                }
            }

            return items.ToArray();

        }

        public GameItem[] GetPlayerItems(PlayerID playerID, string itemType)
        {
            return null;
        }

        private bool validateOwnerOnBC(PlayerID player, string tokenId)
        {
            BigInteger tokenBigInt = new BigInteger(Encoding.Unicode.GetBytes(tokenId));

            ERC721GameItemContract contract =  HoardService.Instance.BCComm.GetContract<ERC721GameItemContract>("0x06012c8cf97BEaD5deAe237070F9587f8E7A266d");

            BigInteger owner = contract.OwnerOf(tokenBigInt).Result;
            
            return (player.ID == owner.ToString());
        }


        public Task<bool> Transfer(PlayerID recipient, GameItem item)
        {
            throw new NotImplementedException();
        }
    }
}