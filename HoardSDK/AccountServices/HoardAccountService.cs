using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nethereum.RLP;
using Newtonsoft.Json;
using RestSharp;
using WebSocketSharp;

namespace Hoard
{
    public class HoardAccountService : IAccountService
    {
        public enum MessageId
        {
            kUnknown = 0,
            kInvalidMessage,
            kAuthenticate,
            kEnumerateAccounts,
            kGiveActiveAccount,
            kSignMessage,
            kSignTransaction,
            kSetActiveAccount,
        }

        public enum Helper
        {
            kUserNameLength = 64,
            kTokenLength = 256,
            kAddressLength = 20,
            kSignature = 65,
            kHash = 32
        }

        public enum ErrorCodes
        {
            errOk = 0x0,
            errInvalidPassword = 0x1,
            errAccountNotFound = 0x2,
            errAuthenticationFailed = 0x3,

            errUnknown = 0xff,
        };

        public class SocketData
        {
            public byte[] ReceivedSignature = new byte[(int)Helper.kSignature];
            public ManualResetEvent ResponseEvent = new ManualResetEvent(false);
            public WebSocket Socket = null;
            public User Owner = null;
            public AccountInfo ActiveAccount = null;

            public SocketData()
            {
            }
            ~SocketData()
            {
                if (Socket != null)
                    Socket.Close();
            }
        }

        private class HoardAccount : AccountInfo
        {
            public User Owner;
            public bool InternalSet = false;

            public HoardAccount(string name, string id, User user)
                : base(name, new HoardID(System.Numerics.BigInteger.Zero))
            {
                Owner = user;
                InternalSet = false;
            }

            public override Task<string> SignTransaction(byte[] input)
            {
                return HoardAccountService.SignTransactionInternal(input, this);
            }

            public override Task<string> SignMessage(byte[] input)
            {
                return HoardAccountService.SignMessageInternal(input, this);
            }

            public override Task<AccountInfo> Activate(User user)
            {
                if (InternalSet)
                {
                    return Task.Run(() =>
                    {
                        return (AccountInfo)this;
                    });
                }
                else
                    return HoardAccountService.ActivateAccount(user, this);
            }
        }

        RestClient AuthClient = null;
        IUserInputProvider UserInputProvider = null;
        string ClientId = null;
        Dictionary<User, AuthToken> UserAuthTokens = new Dictionary<User, AuthToken>();        
        string SignerUrl = "";

        static Dictionary<User, SocketData> SignerClients = new Dictionary<User, SocketData>();
        static UInt32 MessagePrefix = 0x00201801;
        static int MAX_WAIT_TIME_IN_MS = 20000;

        class ErrorResponse
        {
            public string error = null;
        }

        class TokenResponse
        {
            public string access_token = null;
            public string token_type = null;
            public string expires_in = null;
        }

        class AuthToken
        {
            public string AccessToken = null;
            public DateTime ExpireTime;

            public bool IsValid()
            {
                int dateCompare = DateTime.Compare(DateTime.UtcNow, ExpireTime);
                return dateCompare < 0;
            }
        }

        byte[] StringToByteArray(string str, int length)
        {
            return Encoding.ASCII.GetBytes(str.PadRight(length, ' '));
        }

        public HoardAccountService(string url, string signerUrl, string clientId, IUserInputProvider userInputProvider)
        {
            UserInputProvider = userInputProvider;
            ClientId = clientId;
            AuthClient = new RestClient(url);
            SignerUrl = url;
        }

        protected bool ProcessMessage(byte[] msg, SocketData sd)
        {
            MemoryStream ms = new MemoryStream(msg);
            BinaryReader reader = new BinaryReader(ms);
            UInt32 prefix = reader.ReadUInt32();
            if ((prefix & 0x00ffffff) != MessagePrefix)
                return true;
            MessageId id = (MessageId)reader.ReadUInt32();
            UInt32 errorCode = (prefix & 0xff000000) >> 24;
            if ((ErrorCodes)errorCode != ErrorCodes.errOk)
                Debug.WriteLine("Error [" + errorCode.ToString() + "] occurred during receiving message from signer");
            switch (id)
            {
                case MessageId.kInvalidMessage:
                    Debug.WriteLine("Invalid message");
                    return true;
                case MessageId.kAuthenticate:
                    return Msg_Authenticate(reader, sd);
                case MessageId.kEnumerateAccounts:
                    return Msg_EnumerateAccounts(reader, sd);
                case MessageId.kGiveActiveAccount:
                    Debug.WriteLine("Invalid message");
                    return true;
                case MessageId.kSignMessage:
                    return Msg_SignMessage(reader, sd);
                case MessageId.kSignTransaction:
                    return Msg_SignTransaction(reader, sd);
                case MessageId.kSetActiveAccount:
                    return Msg_SetActiveAccount(reader, sd);
                default:
                    Debug.WriteLine("Invalid message id [" + id.ToString() + "]");
                    return true;
            }
        }

        //
        protected bool Msg_Authenticate(BinaryReader reader, SocketData sd)
        {
            UInt32 userAuthenticated = reader.ReadUInt32();
            if (userAuthenticated != 0)
            {
                Debug.WriteLine("Authentication confirmed by signer");
                MemoryStream ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(MessagePrefix);
                writer.Write((UInt32)MessageId.kEnumerateAccounts);
                sd.Socket.Send(ms.ToArray());
                return false;
            }
            else
                Debug.WriteLine("[WARNING] Authentication not confirmed by signer!");
            return true;
        }

        //
        protected bool Msg_EnumerateAccounts(BinaryReader reader, SocketData sd)
        {
            UInt32 numAccounts = reader.ReadUInt32();
            if (numAccounts > 0)
            {
                Int32 activeAccountIndex = reader.ReadInt32();
                for (uint i = 0; i < numAccounts; i++)
                {
                    byte[] address = new byte[(int)Helper.kAddressLength];
                    reader.Read(address, 0, (int)Helper.kAddressLength);
                    string accountName = BitConverter.ToString(address).Replace("-", string.Empty).ToLower();
                    Debug.WriteLine("SignerAccountService: " + accountName + " received");
                    Debug.Assert(sd.Owner != null);
                    HoardAccount accountInfo = new HoardAccount(sd.Owner.HoardId, accountName, sd.Owner);
                    sd.Owner.Accounts.Add(accountInfo);
                }
                if(activeAccountIndex > -1)
                {
                    Debug.Assert(activeAccountIndex >= 0 && activeAccountIndex < sd.Owner.Accounts.Count);
                    ((HoardAccount)sd.Owner.Accounts[activeAccountIndex]).InternalSet = true;
                    sd.Owner.ChangeActiveAccount((HoardAccount)sd.Owner.Accounts[activeAccountIndex]);
                    ((HoardAccount)sd.Owner.Accounts[activeAccountIndex]).InternalSet = false;
                }
            }
            return true;
	    }

        //
        protected bool Msg_SignMessage(BinaryReader reader, SocketData sd)
        {
            reader.Read(sd.ReceivedSignature, 0, (int)Helper.kSignature);
            return true;
        }

        //
        protected bool Msg_SignTransaction(BinaryReader reader, SocketData sd)
        {
            reader.Read(sd.ReceivedSignature, 0, (int)Helper.kSignature);
            return true;
        }

        //
        protected bool Msg_SetActiveAccount(BinaryReader reader, SocketData sd)
        {
            byte[] id = new byte[(int)Helper.kAddressLength];
            reader.Read(id, 0, (int)Helper.kAddressLength);
            HoardID hid = new HoardID(new System.Numerics.BigInteger(id));
            for (int i = 0; i < sd.Owner.Accounts.Count; i++)
            {
                if(sd.Owner.Accounts[i].ID == hid)
                {
                    sd.ActiveAccount = sd.Owner.Accounts[i];
                    break;
                }
            }
            return true;
        }

        public async Task<AccountInfo> CreateAccount(string name, User user)
        {
            //System.Diagnostics.Trace.TraceInformation("Generating user account on Hoard Auth Server.");

            //string email = user.HoardId;
            //if (email == "")
            //    email = await UserInputProvider.RequestInput(user, eUserInputType.kEmail, "email");

            //string password = await UserInputProvider.RequestInput(user, eUserInputType.kPassword, "password");

            //var createRequest = new RestRequest("/create_user", Method.POST);
            //createRequest.RequestFormat = DataFormat.Json;
            //createRequest.AddBody(new { email, password, client_id = ClientId });
            //var createResponse = await AuthClient.ExecuteTaskAsync(createRequest);

            //if (createResponse.StatusCode == System.Net.HttpStatusCode.Created)
            //{
            //    user.HoardId = email;
            //    AccountInfo accountInfo = new HoardAccount(email, this);
            //    user.Accounts.Add(accountInfo);
            //    return accountInfo;
            //}
            //else
            //{
            //    string errorMsg = "Unable to create new user account: Hoard Auth Server status code: " + createResponse.StatusCode;
            //    if (createResponse.Content != null)
            //    {
            //        ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(createResponse.Content);
            //        errorMsg += ", error: " + errorResponse.error;
            //    }
            //    System.Diagnostics.Trace.TraceInformation(errorMsg);
            //}

            return await Task.FromResult<AccountInfo>(null);
        }

        public async Task<bool> RequestAccounts(User user)
        {
            if (user.HoardId == "")
                return false;

            //connect to account server using REST
            //server asks for auth token from HoardAuthService
            //to get this token we should authenticate with HoardAuthService (send password etc.)

            //TODO: assume that our account service is a certified ISteamGameServer (or alike) and allows
            //authentication via Steam API (for example using Session Tickets or Encrypted Application Tickets)
            //in that case it doesn't need auth token from OpenID Connect server
            //But it has to know which form of authentication to choose
            //Ad some kind of HoardAccountService::SetAuthProvider(SteamAuthProvider p) and expose it through SDK
            //Also developer should be able to easily set up which IAccountService is the active one
            //Do sth similar for Switch authentication

            AuthToken token = null;
            string password = await UserInputProvider.RequestInput(user, eUserInputType.kPassword, "password");
            //check if we have valid auth token
            if (!UserAuthTokens.TryGetValue(user, out token) || !token.IsValid())
            {
                //none found ask for a new one
                token = await RequestAuthToken(user, password);
                //store
                if (token != null)
                    UserAuthTokens[user] = token;
            }

            if (token != null)
            {
                //pass token to account server
                //server should call /userinfo with access_token to prove it is valid
                //assume it did and returned a valid account or no accounts (which is also valid)

                SocketData socketData = null;
                if (!SignerClients.TryGetValue(user, out socketData))
                {
                    socketData = new SocketData();
                    socketData.Socket = new WebSocket(SignerUrl, "internal-hoard-protocol");
                    socketData.Socket.OnMessage += (sender, e) =>
                    {
                        Debug.WriteLine("Message received: " + e.Data);
                        if (ProcessMessage(e.RawData, socketData))
                            socketData.ResponseEvent.Set();
                    };
                    socketData.Socket.OnOpen += (sender, e) =>
                    {
                        Debug.WriteLine("Connection established");
                        socketData.ResponseEvent.Set();
                    };
                    socketData.Socket.OnClose += (sender, e) =>
                    {
                        Debug.WriteLine("Connection closed");
                        socketData.ResponseEvent.Set();
                    };
                    socketData.Socket.OnError += (sender, e) =>
                    {
                        Debug.WriteLine("Connection error!");
                        socketData.ResponseEvent.Set();
                    };
                    SignerClients[user] = socketData;
                    socketData.Socket.Connect();
                }

                user.Accounts.Clear();
                MemoryStream ms = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(ms);
                writer.Write(MessagePrefix);
                writer.Write((UInt32)MessageId.kAuthenticate);
                writer.Write(StringToByteArray(user.UserName, (int)Helper.kUserNameLength));
                writer.Write(StringToByteArray(user.UserName, (int)Helper.kUserNameLength));
                writer.Write(StringToByteArray(password, (int)Helper.kTokenLength));
                socketData.Owner = user;
                socketData.ResponseEvent.Reset();
                socketData.Socket.Send(ms.ToArray());
                if(socketData.ResponseEvent.WaitOne(MAX_WAIT_TIME_IN_MS))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task<AuthToken> RequestAuthToken(User user, string password)
        {
            var tokenRequest = new RestRequest("/token", Method.POST);
            tokenRequest.AddParameter("grant_type", "password");
            tokenRequest.AddParameter("username", user.HoardId);
            tokenRequest.AddParameter("password", password);
            tokenRequest.AddParameter("client_id", ClientId);

            DateTime tokenExpireDate = DateTime.UtcNow;

            var tokenResponse = await AuthClient.ExecuteTaskAsync(tokenRequest);

            if (tokenResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                TokenResponse tokenResponseObj = JsonConvert.DeserializeObject<TokenResponse>(tokenResponse.Content);

                AuthToken authToken = new AuthToken();
                authToken.AccessToken = tokenResponseObj.access_token;
                authToken.ExpireTime = tokenExpireDate.AddSeconds(Convert.ToInt32(tokenResponseObj.expires_in));

                return authToken;
            }
            else
            {
                string errorMsg = "Unable to authenticate user account " + user.HoardId  + ": Hoard Auth Server status code: " + tokenResponse.StatusCode;
                if (!string.IsNullOrEmpty(tokenResponse.Content))
                {
                    ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(tokenResponse.Content);
                    errorMsg += ", error: " + errorResponse.error;
                }
                System.Diagnostics.Trace.TraceInformation(errorMsg);
            }

            return null;
        }

        public Task<string> SignMessage(byte[] input, AccountInfo signature)
        {
            return SignMessageInternal(input, signature);
        }

        public Task<string> SignTransaction(byte[] rlpEncodedTransaction, AccountInfo signature)
        {
            return SignTransactionInternal(rlpEncodedTransaction, signature);
        }

        public static Task<string> SignMessageInternal(byte[] input, AccountInfo accountInfo)
        {
            var signer = new Nethereum.Signer.EthereumMessageSigner();
            MemoryStream ms = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(ms);
            writer.Write(MessagePrefix);
            writer.Write((UInt32)MessageId.kSignMessage);
            writer.Write(signer.HashPrefixedMessage(input), 0, (int)Helper.kHash);
            Debug.Assert(((HoardAccount)accountInfo).Owner != null);
            SocketData socketData = null;
            if (SignerClients.TryGetValue(((HoardAccount)accountInfo).Owner, out socketData))
            {
                socketData.Owner = ((HoardAccount)accountInfo).Owner;
                socketData.ResponseEvent.Reset();
                socketData.Socket.Send(ms.ToArray());
                if (socketData.ResponseEvent.WaitOne(MAX_WAIT_TIME_IN_MS))
                    return Task.FromResult<string>(BitConverter.ToString(socketData.ReceivedSignature).Replace("-", string.Empty).ToLower());
            }
            return Task.FromResult<string>("");
        }

        public static Task<string> SignTransactionInternal(byte[] rlpEncodedTransaction, AccountInfo accountInfo)
        {
            //MemoryStream ms = new MemoryStream();
            //BinaryWriter writer = new BinaryWriter(ms);
            //writer.Write(MessagePrefix);
            //writer.Write((UInt32)MessageId.kSignTransaction);

            //var decodedList = RLP.Decode(rlpEncodedTransaction);
            //var decodedRlpCollection = (RLPCollection)decodedList[0];
            //var data = decodedRlpCollection.ToBytes();
            //writer.Write(signer.HashPrefixedMessage(input), 0, (int)Helper.kHash);

            //Debug.Assert(((HoardAccount)signature).Owner != null);
            //SocketData socketData = null;
            //if (SignerClients.TryGetValue(((HoardAccount)signature).Owner, out socketData))
            //{
            //    socketData.Owner = ((HoardAccount)signature).Owner;
            //    socketData.ResponseEvent.Reset();
            //    socketData.Socket.Send(ms.ToArray());
            //    if (socketData.ResponseEvent.WaitOne(MAX_WAIT_TIME_IN_MS))
            //        return Task.FromResult<string>(BitConverter.ToString(socketData.ReceivedSignature).Replace("-", string.Empty).ToLower());
            //}
            //return Task.FromResult<string>("");
            throw new NotImplementedException();
        }

        public static Task<AccountInfo> ActivateAccount(User user, AccountInfo account)
        {
            return Task.Run(() =>
            {
                SocketData socketData = null;
                if (!SignerClients.TryGetValue(user, out socketData))
                {
                    MemoryStream ms = new MemoryStream();
                    BinaryWriter writer = new BinaryWriter(ms);
                    writer.Write(MessagePrefix);
                    writer.Write((UInt32)MessageId.kSetActiveAccount);
                    writer.Write(account.ID.ToHexByteArray());
                    socketData.ActiveAccount = null;
                    socketData.Owner = user;
                    socketData.ResponseEvent.Reset();
                    socketData.Socket.Send(ms.ToArray());
                    if (socketData.ResponseEvent.WaitOne(MAX_WAIT_TIME_IN_MS))
                    {
                        return socketData.ActiveAccount;
                    }
                }
                return null;
            });
        }
    }
}
