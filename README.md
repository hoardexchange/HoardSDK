# HoardCsSDK

HoardSDK C# interface

The HoardSDK library is used to work with the Hoard platform.

This is a multiplatform library that works in generic C# projects based on either .NET framework 4.6 or later, .NETStandard 2.0, Unity 3D with full support for Windows PC, MacOS, Android and iOS (and possibly other AOT platforms). It is dependant on well known libraries like Newtonsoft JSON (Hoard fork for full AOT support), Nethereum (Hoard fork for full AOT support) and RestSharp.

The solution contains also a HoardSDKDocumentation project that creates an chtml file with API documentation.

All code is compiled using Microsoft Visual Studio 2017 Community version.

The Hoard team will provide some examples/samples of how to use the library in the future. For now reader is encouraged to check HoardTests library with a lot of usage examples.

The main entry point for any library usage is the HoardService object that needs to be initialized:

The minimal code is:

```csharp
HoardServiceConfig config = HoardServiceConfig.Load(configPath);

BCClientOptions clientOpts = null;
if (config.BCClient is EthereumClientConfig)
{
    var ethConfig = config.BCClient as EthereumClientConfig;
    clientOpts = new EthereumClientOptions(
        new Nethereum.JsonRpc.Client.RpcClient(new Uri(ethConfig.ClientUrl))
    );
}
HoardServiceOptions options = new HoardServiceOptions(config, clientOpts);

HoardService = HoardService.Instance;

//init
HoardService.Initialize(options);

//some logic

//shutdown
HoardService.Shutdown();
```

The sample configuration file that gets access to our testnet on Rinkeby
```javascript
{
  "GameID": "618593968041a02c4bfb6c1da4103875ceeee6ae8754b717690cf24313509269",
  "GameBackendUrl": "https://plasmadog.hoard.exchange/",
  "BCClient": {
    "Type": "Ethereum",
    "ClientUrl": "https://rinkeby.infura.io/v3/f7144cb8b8dc4522afb8ad054154b083"
  },
  "AccountsDir": null,
  "GameCenterContract": "0x54f337a4d1a1024ea181e940b96551c11ba42c6d",
  "ExchangeServiceUrl": "http://",
  "HoardAuthServiceUrl": null,
  "HoardAuthServiceClientId": null,
  "WhisperAddress": "ws://ws.eth-rpc.hoard.exchange"
}
```
