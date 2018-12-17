using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace Hoard
{
    public class HoardAccountService : IAccountService
    {
        public class HoardAccount : AccountInfo
        {
            private HoardAccountService HoardSigner;

            public HoardAccount(string name, HoardAccountService signer)
                : base(name, "")
            {
                HoardSigner = signer;
            }

            public override Task<string> SignTransaction(byte[] input)
            {
                return HoardSigner.SignTransaction(input, this);
            }

            public override Task<string> SignMessage(byte[] input)
            {
                return HoardSigner.SignMessage(input, this);
            }
        }

        RestClient AuthClient = null;
        IUserInputProvider UserInputProvider = null;
        string ClientId = null;
        Dictionary<User, AuthToken> UserAuthTokens = new Dictionary<User, AuthToken>();

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

        public HoardAccountService(string url, string clientId, IUserInputProvider userInputProvider)
        {
            UserInputProvider = userInputProvider;
            ClientId = clientId;
            AuthClient = new RestClient(url);
        }

        public async Task<AccountInfo> CreateAccount(string name, User user)
        {
            System.Diagnostics.Trace.TraceInformation("Generating user account on Hoard Auth Server.");

            string email = user.HoardId;
            if (email == "")
                email = await UserInputProvider.RequestInput(user, eUserInputType.kEmail, "email");

            string password = await UserInputProvider.RequestInput(user, eUserInputType.kPassword, "password");

            var createRequest = new RestRequest("/create_user", Method.POST);
            createRequest.RequestFormat = DataFormat.Json;
            createRequest.AddBody(new { email, password, client_id = ClientId });
            var createResponse = await AuthClient.ExecuteTaskAsync(createRequest);

            if (createResponse.StatusCode == System.Net.HttpStatusCode.Created)
            {
                user.HoardId = email;
                AccountInfo accountInfo = new HoardAccount(email, this);
                user.Accounts.Add(accountInfo);
                return accountInfo;
            }
            else
            {
                string errorMsg = "Unable to create new user account: Hoard Auth Server status code: " + createResponse.StatusCode;
                if (createResponse.Content != null)
                {
                    ErrorResponse errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(createResponse.Content);
                    errorMsg += ", error: " + errorResponse.error;
                }
                System.Diagnostics.Trace.TraceInformation(errorMsg);
            }

            return null;
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
            //check if we have valid auth token
            if (!UserAuthTokens.TryGetValue(user, out token) || !token.IsValid())
            {
                //none found ask for a new one
                token = await RequestAuthToken(user);
                //store
                if (token != null)
                    UserAuthTokens[user] = token;
            }

            if (token != null)
            {
                //pass token to account server
                //server should call /userinfo with access_token to prove it is valid
                //assume it did and returned a valid account or no accounts (which is also valid)
                //TODO: if no accounts, we should perhaps call CreateAccount? (or this should be explicitly done by user?)

                AccountInfo accountInfo = new HoardAccount(user.HoardId, this);
                user.Accounts.Add(accountInfo);

                return true;
            }

            return false;
        }

        private async Task<AuthToken> RequestAuthToken(User user)
        {
            string password = await UserInputProvider.RequestInput(user, eUserInputType.kPassword, "password");

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

        public Task<string> SignTransaction(byte[] input, AccountInfo signature)
        {
            throw new NotImplementedException();
        }

        public Task<string> SignMessage(byte[] input, AccountInfo signature)
        {
            throw new NotImplementedException();
        }
    }
}
