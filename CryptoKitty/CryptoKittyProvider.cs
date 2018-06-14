using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts;
using Nethereum.JsonRpc.Client;
using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Hoard
{
    public class CryptoKitty : GameAsset
    {
        public BigInteger TokenId { get; private set; }

        public CryptoKitty(string symbol, string name, BC.Contracts.GameAssetContract contract, ulong totalSuplly, ulong assetId, string assetType, BigInteger tokenId) :
            base(symbol, name, contract, totalSuplly, assetId, assetType)
        {
            TokenId = tokenId;
        }
    }

    [FunctionOutput]
    public class GetKittyOutput
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
        private IClient client;
        private string contractAddress;
        private string ownerAddress;
        private List<string> tokenIds;

        public static string ABI = @"[    {      'constant': true,      'inputs': [        {          'name': '_interfaceID',          'type': 'bytes4'        }      ],      'name': 'supportsInterface',      'outputs': [        {          'name': '',          'type': 'bool'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'cfoAddress',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_tokenId',          'type': 'uint256'        },        {          'name': '_preferredTransport',          'type': 'string'        }      ],      'name': 'tokenMetadata',      'outputs': [        {          'name': 'infoUrl',          'type': 'string'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'promoCreatedCount',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'name',      'outputs': [        {          'name': '',          'type': 'string'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_to',          'type': 'address'        },        {          'name': '_tokenId',          'type': 'uint256'        }      ],      'name': 'approve',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'ceoAddress',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'GEN0_STARTING_PRICE',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_address',          'type': 'address'        }      ],      'name': 'setSiringAuctionAddress',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'totalSupply',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'pregnantKitties',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_kittyId',          'type': 'uint256'        }      ],      'name': 'isPregnant',      'outputs': [        {          'name': '',          'type': 'bool'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'GEN0_AUCTION_DURATION',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'siringAuction',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_from',          'type': 'address'        },        {          'name': '_to',          'type': 'address'        },        {          'name': '_tokenId',          'type': 'uint256'        }      ],      'name': 'transferFrom',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_address',          'type': 'address'        }      ],      'name': 'setGeneScienceAddress',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_newCEO',          'type': 'address'        }      ],      'name': 'setCEO',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_newCOO',          'type': 'address'        }      ],      'name': 'setCOO',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_kittyId',          'type': 'uint256'        },        {          'name': '_startingPrice',          'type': 'uint256'        },        {          'name': '_endingPrice',          'type': 'uint256'        },        {          'name': '_duration',          'type': 'uint256'        }      ],      'name': 'createSaleAuction',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '',          'type': 'uint256'        }      ],      'name': 'sireAllowedToAddress',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_matronId',          'type': 'uint256'        },        {          'name': '_sireId',          'type': 'uint256'        }      ],      'name': 'canBreedWith',      'outputs': [        {          'name': '',          'type': 'bool'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '',          'type': 'uint256'        }      ],      'name': 'kittyIndexToApproved',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_kittyId',          'type': 'uint256'        },        {          'name': '_startingPrice',          'type': 'uint256'        },        {          'name': '_endingPrice',          'type': 'uint256'        },        {          'name': '_duration',          'type': 'uint256'        }      ],      'name': 'createSiringAuction',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': 'val',          'type': 'uint256'        }      ],      'name': 'setAutoBirthFee',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_addr',          'type': 'address'        },        {          'name': '_sireId',          'type': 'uint256'        }      ],      'name': 'approveSiring',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_newCFO',          'type': 'address'        }      ],      'name': 'setCFO',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_genes',          'type': 'uint256'        },        {          'name': '_owner',          'type': 'address'        }      ],      'name': 'createPromoKitty',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': 'secs',          'type': 'uint256'        }      ],      'name': 'setSecondsPerBlock',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'paused',      'outputs': [        {          'name': '',          'type': 'bool'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_tokenId',          'type': 'uint256'        }      ],      'name': 'ownerOf',      'outputs': [        {          'name': 'owner',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'GEN0_CREATION_LIMIT',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'newContractAddress',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_address',          'type': 'address'        }      ],      'name': 'setSaleAuctionAddress',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_owner',          'type': 'address'        }      ],      'name': 'balanceOf',      'outputs': [        {          'name': 'count',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'secondsPerBlock',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [],      'name': 'pause',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_owner',          'type': 'address'        }      ],      'name': 'tokensOfOwner',      'outputs': [        {          'name': 'ownerTokens',          'type': 'uint256[]'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_matronId',          'type': 'uint256'        }      ],      'name': 'giveBirth',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [],      'name': 'withdrawAuctionBalances',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'symbol',      'outputs': [        {          'name': '',          'type': 'string'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '',          'type': 'uint256'        }      ],      'name': 'cooldowns',      'outputs': [        {          'name': '',          'type': 'uint32'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '',          'type': 'uint256'        }      ],      'name': 'kittyIndexToOwner',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_to',          'type': 'address'        },        {          'name': '_tokenId',          'type': 'uint256'        }      ],      'name': 'transfer',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'cooAddress',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'autoBirthFee',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'erc721Metadata',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_genes',          'type': 'uint256'        }      ],      'name': 'createGen0Auction',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_kittyId',          'type': 'uint256'        }      ],      'name': 'isReadyToBreed',      'outputs': [        {          'name': '',          'type': 'bool'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'PROMO_CREATION_LIMIT',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_contractAddress',          'type': 'address'        }      ],      'name': 'setMetadataAddress',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'saleAuction',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_sireId',          'type': 'uint256'        },        {          'name': '_matronId',          'type': 'uint256'        }      ],      'name': 'bidOnSiringAuction',      'outputs': [],      'payable': true,      'stateMutability': 'payable',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'gen0CreatedCount',      'outputs': [        {          'name': '',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': true,      'inputs': [],      'name': 'geneScience',      'outputs': [        {          'name': '',          'type': 'address'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [        {          'name': '_matronId',          'type': 'uint256'        },        {          'name': '_sireId',          'type': 'uint256'        }      ],      'name': 'breedWithAuto',      'outputs': [],      'payable': true,      'stateMutability': 'payable',      'type': 'function'    },    {      'inputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'constructor'    },    {      'payable': true,      'stateMutability': 'payable',      'type': 'fallback'    },    {      'anonymous': false,      'inputs': [        {          'indexed': false,          'name': 'owner',          'type': 'address'        },        {          'indexed': false,          'name': 'matronId',          'type': 'uint256'        },        {          'indexed': false,          'name': 'sireId',          'type': 'uint256'        },        {          'indexed': false,          'name': 'cooldownEndBlock',          'type': 'uint256'        }      ],      'name': 'Pregnant',      'type': 'event'    },    {      'anonymous': false,      'inputs': [        {          'indexed': false,          'name': 'from',          'type': 'address'        },        {          'indexed': false,          'name': 'to',          'type': 'address'        },        {          'indexed': false,          'name': 'tokenId',          'type': 'uint256'        }      ],      'name': 'Transfer',      'type': 'event'    },    {      'anonymous': false,      'inputs': [        {          'indexed': false,          'name': 'owner',          'type': 'address'        },        {          'indexed': false,          'name': 'approved',          'type': 'address'        },        {          'indexed': false,          'name': 'tokenId',          'type': 'uint256'        }      ],      'name': 'Approval',      'type': 'event'    },    {      'anonymous': false,      'inputs': [        {          'indexed': false,          'name': 'owner',          'type': 'address'        },        {          'indexed': false,          'name': 'kittyId',          'type': 'uint256'        },        {          'indexed': false,          'name': 'matronId',          'type': 'uint256'        },        {          'indexed': false,          'name': 'sireId',          'type': 'uint256'        },        {          'indexed': false,          'name': 'genes',          'type': 'uint256'        }      ],      'name': 'Birth',      'type': 'event'    },    {      'anonymous': false,      'inputs': [        {          'indexed': false,          'name': 'newContract',          'type': 'address'        }      ],      'name': 'ContractUpgrade',      'type': 'event'    },    {      'constant': false,      'inputs': [        {          'name': '_v2Address',          'type': 'address'        }      ],      'name': 'setNewAddress',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': true,      'inputs': [        {          'name': '_id',          'type': 'uint256'        }      ],      'name': 'getKitty',      'outputs': [        {          'name': 'isGestating',          'type': 'bool'        },        {          'name': 'isReady',          'type': 'bool'        },        {          'name': 'cooldownIndex',          'type': 'uint256'        },        {          'name': 'nextActionAt',          'type': 'uint256'        },        {          'name': 'siringWithId',          'type': 'uint256'        },        {          'name': 'birthTime',          'type': 'uint256'        },        {          'name': 'matronId',          'type': 'uint256'        },        {          'name': 'sireId',          'type': 'uint256'        },        {          'name': 'generation',          'type': 'uint256'        },        {          'name': 'genes',          'type': 'uint256'        }      ],      'payable': false,      'stateMutability': 'view',      'type': 'function'    },    {      'constant': false,      'inputs': [],      'name': 'unpause',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    },    {      'constant': false,      'inputs': [],      'name': 'withdrawBalance',      'outputs': [],      'payable': false,      'stateMutability': 'nonpayable',      'type': 'function'    }  ]";

        private string[] properties = new string[1] { "genotype" };

        private Web3 web3 = null;
        private Contract contract = null;

        public CryptoKittyProvider(Nethereum.JsonRpc.Client.IClient _client, string _contractAddress, string _tokenId)
        {
            client = _client;
            contractAddress = _contractAddress;
            ownerAddress = "";
            tokenIds = new List<string>();
            tokenIds.Add(_tokenId);

            web3 = new Web3(client);
            contract = web3.Eth.GetContract(ABI, contractAddress);
        }

        override public string[] getPropertyNames()
        {
            return properties;
        }

        override public Result getItems(out List<GameAsset> items)
        {
            items = new List<GameAsset>();

            // its not possible to get tokenId's by owner, cryptokitty "tokensOfOwner" is very inefficient and timeouts
            /*
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

            if (tokens==null)
            {
                return new Result("unable to retrive tokensOfOwner");
            }

            if (tokens.Count > 0)
            {
                for (int i = 0; i < tokens.Count; ++i)
                {
                    var gameAsset = new CryptoKitty(
                        "CK", //symbol
                        "CryptoKitties", //name
                        null, //TODO: contract
                        1, //totalSupply
                        1000 + (ulong)tokens[0], //assetId // TODO !!!! currently assetId must be unique in hoard service
                        "cryptokitty", //assetType
                        tokens[i] // tokenId
                    );
                       
                    items.Add(gameAsset);
                }
            }
            */

            Function totallSupplyFunc = contract.GetFunction("totalSupply");
            var task1 = totallSupplyFunc.CallAsync<ulong>();
            ulong totallSupply = task1.Result;
            BigInteger totallSupplyBigInt = new BigInteger(totallSupply);

            foreach (var token in tokenIds)
            {
                BigInteger tokenBigInt;
                if (BigInteger.TryParse(token, out tokenBigInt))
                {
                    if (tokenBigInt <= totallSupplyBigInt)
                    {
                        var gameAsset = new CryptoKitty(
                            "CK", //symbol
                            "CryptoKitties", //name
                            null, //TODO: contract
                            1, //totalSupply
                            1000 + (ulong)tokenBigInt, //assetId // TODO !!!! currently assetId must be unique in hoard service
                            "cryptokitty", //assetType
                            tokenBigInt // tokenId
                        );

                        items.Add(gameAsset);
                        getProperties(gameAsset);
                    }
                }
            }

            return new Result();
        }

        override public Result getProperties(GameAsset item)
        {
            if (item.AssetType == "cryptokitty")
            {
                CryptoKitty cryptoKitty = item as CryptoKitty;
                if (cryptoKitty != null)
                {
                    Function getFunc = contract.GetFunction("getKitty");
                    var task2 = getFunc.CallDeserializingToObjectAsync<GetKittyOutput>(cryptoKitty.TokenId);
                    var result2 = task2.Result;
                    BigInteger genes = result2.genes;

                    cryptoKitty.Properties[properties[0]] = genes.ToString();

                    return new Result();
                }
                else
                    return new Result("not cryptokitty item");
            }

            return new Result("no props");
        }
    }
}
