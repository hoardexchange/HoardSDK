using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;

namespace Hoard
{
    [FunctionOutput]
    public class GetKittyBCOutput
    {
        [Parameter("bool", "isGestating", 1)]
        public bool isGestating { get; set; }

        [Parameter("bool", "isReady", 2)]
        public bool isReady { get; set; }

        [Parameter("uint256", "cooldownIndex", 3)]
        public BigInteger cooldownIndex { get; set; }

        [Parameter("uint256", "nextActionAt", 4)]
        public BigInteger nextActionAt { get; set; }

        [Parameter("uint256", "siringWithId", 5)]
        public BigInteger siringWithId { get; set; }

        [Parameter("uint256", "birthTime", 6)]
        public BigInteger birthTime { get; set; }

        [Parameter("uint256", "matronId", 7)]
        public BigInteger matronId { get; set; }

        [Parameter("uint256", "sireId", 8)]
        public BigInteger sireId { get; set; }

        [Parameter("uint256", "generation", 9)]
        public BigInteger generation { get; set; }

        [Parameter("uint256", "genes", 10)]
        public BigInteger genes { get; set; }
    }

    public class CryptoKittyProvider : Provider
    {
        private IClient blockchainClient;
        private bool mainnet;
        private string contractAddress;
        private string ownerAddress;

        public static string ABI = @"[    {      'constant': true,      'inputs': [        {          'name': '_interfaceID',          'type': 'bytes4'        }      ],      'name': 'supportsInterface',      'outputs': [        {          'name': '',          'type': 'bool'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'cfoAddress',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_tokenId',          'type': 'uint256'        },        {          'name': '_preferredTransport',          'type': 'string'        }      ],      'name': 'tokenMetadata',      'outputs': [        {          'name': 'infoUrl',          'type': 'string'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'promoCreatedCount',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'name',      'outputs': [        {          'name': '',          'type': 'string'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_to',          'type': 'address'        },        {          'name': '_tokenId',          'type': 'uint256'        }      ],      'name': 'approve',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'ceoAddress',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'GEN0_STARTING_PRICE',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_address',          'type': 'address'        }      ],      'name': 'setSiringAuctionAddress',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'totalSupply',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'pregnantKitties',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_kittyId',          'type': 'uint256'        }      ],      'name': 'isPregnant',      'outputs': [        {          'name': '',          'type': 'bool'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'GEN0_AUCTION_DURATION',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'siringAuction',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_from',          'type': 'address'        },        {          'name': '_to',          'type': 'address'        },        {          'name': '_tokenId',          'type': 'uint256'        }      ],      'name': 'transferFrom',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_address',          'type': 'address'        }      ],      'name': 'setGeneScienceAddress',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_newCEO',          'type': 'address'        }      ],      'name': 'setCEO',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_newCOO',          'type': 'address'        }      ],      'name': 'setCOO',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_kittyId',          'type': 'uint256'        },        {          'name': '_startingPrice',          'type': 'uint256'        },        {          'name': '_endingPrice',          'type': 'uint256'        },        {          'name': '_duration',          'type': 'uint256'        }      ],      'name': 'createSaleAuction',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '',          'type': 'uint256'        }      ],      'name': 'sireAllowedToAddress',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_matronId',          'type': 'uint256'        },        {          'name': '_sireId',          'type': 'uint256'        }      ],      'name': 'canBreedWith',      'outputs': [        {          'name': '',          'type': 'bool'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '',          'type': 'uint256'        }      ],      'name': 'kittyIndexToApproved',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_kittyId',          'type': 'uint256'        },        {          'name': '_startingPrice',          'type': 'uint256'        },        {          'name': '_endingPrice',          'type': 'uint256'        },        {          'name': '_duration',          'type': 'uint256'        }      ],      'name': 'createSiringAuction',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': 'val',          'type': 'uint256'        }      ],      'name': 'setAutoBirthFee',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_addr',          'type': 'address'        },        {          'name': '_sireId',          'type': 'uint256'        }      ],      'name': 'approveSiring',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_newCFO',          'type': 'address'        }      ],      'name': 'setCFO',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_genes',          'type': 'uint256'        },        {          'name': '_owner',          'type': 'address'        }      ],      'name': 'createPromoKitty',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': 'secs',          'type': 'uint256'        }      ],      'name': 'setSecondsPerBlock',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'paused',      'outputs': [        {          'name': '',          'type': 'bool'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_tokenId',          'type': 'uint256'        }      ],      'name': 'ownerOf',      'outputs': [        {          'name': 'owner',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'GEN0_CREATION_LIMIT',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'newContractAddress',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_address',          'type': 'address'        }      ],      'name': 'setSaleAuctionAddress',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_owner',          'type': 'address'        }      ],      'name': 'balanceOf',      'outputs': [        {          'name': 'count',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'secondsPerBlock',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [],      'name': 'pause',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_owner',          'type': 'address'        }      ],      'name': 'tokensOfOwner',      'outputs': [        {          'name': 'ownerTokens',          'type': 'uint256[]'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_matronId',          'type': 'uint256'        }      ],      'name': 'giveBirth',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [],      'name': 'withdrawAuctionBalances',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'symbol',      'outputs': [        {          'name': '',          'type': 'string'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '',          'type': 'uint256'        }      ],      'name': 'cooldowns',      'outputs': [        {          'name': '',          'type': 'uint32'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '',          'type': 'uint256'        }      ],      'name': 'kittyIndexToOwner',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_to',          'type': 'address'        },        {          'name': '_tokenId',          'type': 'uint256'        }      ],      'name': 'transfer',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'cooAddress',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'autoBirthFee',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'erc721Metadata',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_genes',          'type': 'uint256'        }      ],      'name': 'createGen0Auction',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_kittyId',          'type': 'uint256'        }      ],      'name': 'isReadyToBreed',      'outputs': [        {          'name': '',          'type': 'bool'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'PROMO_CREATION_LIMIT',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_contractAddress',          'type': 'address'        }      ],      'name': 'setMetadataAddress',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'saleAuction',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_sireId',          'type': 'uint256'        },        {          'name': '_matronId',          'type': 'uint256'        }      ],      'name': 'bidOnSiringAuction',      'outputs': [],      'payable': true,      'stateMutability': 'payable',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'gen0CreatedCount',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'geneScience',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_matronId',          'type': 'uint256'        },        {          'name': '_sireId',          'type': 'uint256'        }      ],      'name': 'breedWithAuto',      'outputs': [],      'payable': true,      'stateMutability': 'payable',      'type': 'function'    },    {      'inputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'constructor'    },    {      'payable': true,      'stateMutability': 'payable',      'type': 'fallback'    },    {      'anonymous': false,      'inputs': [        {          'indexed': false,          'name': 'owner',          'type': 'address'        },        {          'indexed': false,          'name': 'matronId',          'type': 'uint256'        },        {          'indexed': false,          'name': 'sireId',          'type': 'uint256'        },        {          'indexed': false,          'name': 'cooldownEndBlock',          'type': 'uint256'        }      ],      'name': 'Pregnant',      'type': 'event'    },    {      'anonymous': false,      'inputs': [        {          'indexed': false,          'name': 'from',          'type': 'address'        },        {          'indexed': false,          'name': 'to',          'type': 'address'        },        {          'indexed': false,          'name': 'tokenId',          'type': 'uint256'        }      ],      'name': 'Transfer',      'type': 'event'    },    {      'anonymous': false,      'inputs': [        {          'indexed': false,          'name': 'owner',          'type': 'address'        },        {          'indexed': false,          'name': 'approved',          'type': 'address'        },        {          'indexed': false,          'name': 'tokenId',          'type': 'uint256'        }      ],      'name': 'Approval',      'type': 'event'    },    {      'anonymous': false,      'inputs': [        {          'indexed': false,          'name': 'owner',          'type': 'address'        },        {          'indexed': false,          'name': 'kittyId',          'type': 'uint256'        },        {          'indexed': false,          'name': 'matronId',          'type': 'uint256'        },        {          'indexed': false,          'name': 'sireId',          'type': 'uint256'        },        {          'indexed': false,          'name': 'genes',          'type': 'uint256'        }      ],      'name': 'Birth',      'type': 'event'    },    {      'anonymous': false,      'inputs': [        {          'indexed': false,          'name': 'newContract',          'type': 'address'        }      ],      'name': 'ContractUpgrade',      'type': 'event'    },    {      'constant': false,      'inputs': [        {          'name': '_v2Address',          'type': 'address'        }      ],      'name': 'setNewAddress',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_id',          'type': 'uint256'        }      ],      'name': 'getKitty',      'outputs': [        {          'name': 'isGestating',          'type': 'bool'        },        {          'name': 'isReady',          'type': 'bool'        },        {          'name': 'cooldownIndex',          'type': 'uint256'        },        {          'name': 'nextActionAt',          'type': 'uint256'        },        {          'name': 'siringWithId',          'type': 'uint256'        },        {          'name': 'birthTime',          'type': 'uint256'        },        {          'name': 'matronId',          'type': 'uint256'        },        {          'name': 'sireId',          'type': 'uint256'        },        {          'name': 'generation',          'type': 'uint256'        },        {          'name': 'genes',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [],      'name': 'unpause',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [],      'name': 'withdrawBalance',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    }  ]";

        private RestClient cryptoKittiesClient;

        private const string property_genotype = "genotype";
        private const string property_image_url = "image_url";
        private const string property_image = "image";
        private string[] properties = new string[3] { property_genotype, property_image_url, property_image };

        private Web3 web3 = null;
        private Contract contract = null;

        public static bool RemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain,
            // look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        continue;
                    }
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                        break;
                    }
                }
            }
            return isOk;
        }

        public CryptoKittyProvider(Nethereum.JsonRpc.Client.IClient _client, bool _mainnet, string _contractAddress, string _ownerAddress)
        {
            blockchainClient = _client;
            mainnet = _mainnet;
            contractAddress = _contractAddress;
            ownerAddress = _ownerAddress;

            web3 = new Web3(blockchainClient);
            contract = web3.Eth.GetContract(ABI, contractAddress);

            // setup ServicePointManager for https
            //System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls;
            System.Net.ServicePointManager.ServerCertificateValidationCallback += RemoteCertificateValidationCallback;

            cryptoKittiesClient = new RestClient("https://api.cryptokitties.co");
        }

        override public string[] getPropertyNames()
        {
            return properties;
        }

        override public Result getItems(out List<GameAsset> items)
        {
            if (mainnet)
            {
                // its not possible to get tokenId's by owner from BC mainnet nodes, cryptokitty "tokensOfOwner" is very inefficient and timeouts
                return getItemsFromCryptoAPIbyOwner(out items);
            }

            return getItemsFromBCbyOwner(out items);
        }

        public class KittyAPIResult
        {
            public string id;
            public string image_url;
        }

        public class UserKittiesAPIResult
        {
            public List<KittyAPIResult> kitties;
        }

        private Result getItemsFromCryptoAPIbyOwner(out List<GameAsset> items)
        {
            items = new List<GameAsset>();

            var request = new RestRequest("kitties?owner_wallet_address="+ownerAddress+"&limit=10&offset=0", Method.GET);
            var response = cryptoKittiesClient.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
                return new Result("unable to get kitties by owner from " + cryptoKittiesClient.BaseUrl + ", Request: " + "kitties?owner_wallet_address=" + ownerAddress + "&limit=1&offset=0" + ", response: " + response.ErrorMessage + " StatusCode: " + response.StatusCode);

            UserKittiesAPIResult userKitties = JsonConvert.DeserializeObject<UserKittiesAPIResult>(response.Content);

            if (userKitties==null)
                return new Result("unable to parse user kitties response from " + cryptoKittiesClient.BaseUrl + ", Content: " + response.Content);

            var item = new GameAsset(
                "CK", //symbol
                "CryptoKitties", //name
                null, //TODO: contract
                1, //totalSupply
                0, //assetId
                "cryptokitty" //assetType
            );

            items.Add(item);

            item.Instances = new Dictionary<string, Instance>();
            foreach (var kitty in userKitties.kitties)
            {
                if (validateOwnerOnBC(kitty.id))
                {
                    Instance kittyInstance = new Instance();
                    item.Instances[kitty.id] = kittyInstance;

                    if (kitty.image_url != null)
                    {
                        kittyInstance.Properties.Set(property_image_url, kitty.image_url);
                    }
                }
            }

            return new Result();
        }

        private bool validateOwnerOnBC(string tokenId)
        {
            BigInteger tokenBigInt;
            if (!BigInteger.TryParse(tokenId, out tokenBigInt))
                return false;

            Function ownerOfFunc = contract.GetFunction("ownerOf");
            var task1 = ownerOfFunc.CallAsync<string>(tokenBigInt);
            string address = task1.Result;

            return (ownerAddress == address);
        }

        private Result getItemsFromBCbyOwner(out List<GameAsset> items)
        {
            items = new List<GameAsset>();
            
            BigInteger owner = new BigInteger(0);
            try
            {
                string address = ownerAddress;
                if (address.StartsWith("0x"))
                    address = "0" + address.Substring(2);
                owner = BigInteger.Parse(address, System.Globalization.NumberStyles.AllowHexSpecifier);
            }
            catch (Exception)
            {
                return new Result("kitty owner address is invalid");
            }

            Function tokensOfOwnerFunc = contract.GetFunction("tokensOfOwner");
            var task1 = tokensOfOwnerFunc.CallAsync<List<BigInteger>>(owner);
            List<BigInteger> tokens = task1.Result;

            if (tokens == null)
            {
                return new Result("unable to retrive tokensOfOwner");
            }
            
            var item = new GameAsset(
                "CK", //symbol
                "CryptoKitties", //name
                null, //TODO: contract
                1, //totalSupply
                0, //assetId
                "cryptokitty" //assetType
            );

            items.Add(item);

            item.Instances = new Dictionary<string, Instance>();
            if (tokens.Count > 0)
            {
                for (int i = 0; i < tokens.Count; ++i)
                {
                    string tokenId = tokens[i].ToString();
                    item.Instances[tokenId] = new Instance();
                }
            }

            return new Result();
        }

        override public Result getProperties(GameAsset item)
        {
            getPropertiesFromExternalSources(item);

            return getPropertiesFromBC(item);
        }

        private void getPropertiesFromExternalSources(GameAsset item)
        {
            // download image if we had image_url

            if (item.AssetType == "cryptokitty")
            {
                foreach (KeyValuePair<string, Instance> entry in item.Instances)
                {
                    object image_url_prop = entry.Value.Properties.Get(property_image_url);
                    if (image_url_prop != null)
                    {
                        string image_url = image_url_prop as string;
                        if (image_url != null)
                        {
                            byte[] image = getImage(image_url);
                            entry.Value.Properties.Set(property_image, image);
                        }
                    }
                }
            }
        }

        private Result getPropertiesFromBC(GameAsset item)
        {
            // get kitty genes from blockchain

            if (item.AssetType == "cryptokitty")
            {
                foreach (KeyValuePair<string, Instance> entry in item.Instances)
                {
                    BigInteger tokenBigInt;
                    if (BigInteger.TryParse(entry.Key, out tokenBigInt))
                    {
                        Function getFunc = contract.GetFunction("getKitty");
                        var task2 = getFunc.CallDeserializingToObjectAsync<GetKittyBCOutput>(tokenBigInt);
                        var result2 = task2.Result;
                        BigInteger genes = result2.genes;

                        entry.Value.Properties.Set(property_genotype, genes.ToString(), PropertyType.String);
                    }
                    else
                        new Result("invalid tokenId");
                }
            }

            return new Result("no props");
        }

        private byte[] getImage(string url)
        {
            var uri = new Uri(url);
            RestClient imageClient = new RestClient(uri.Scheme + "://" + uri.Authority);
            var request = new RestRequest(uri.PathAndQuery, Method.GET);
            byte[] image = imageClient.DownloadData(request);
            return image;
        }
    }
}
