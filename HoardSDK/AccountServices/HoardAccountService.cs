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

            public async override Task<string> SignTransaction(byte[] input)
            {
                return await HoardSigner.SignTransaction(input, this);
            }

            public async override Task<string> SignMessage(byte[] input)
            {
                return await HoardSigner.SignMessage(input, this);
            }
        }

        RestClient authClient = null;
        HoardServiceOptions Options = null;
        Dictionary<User, AuthToken> userAuthTokens = new Dictionary<User, AuthToken>();

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

        public HoardAccountService(HoardServiceOptions options)
        {
            Options = options;
            authClient = new RestClient(options.HoardAuthServiceUrl);
        }

        public async Task<AccountInfo> CreateAccount(string name, User user)
        {
            System.Diagnostics.Trace.TraceInformation("Generating user account on Hoard Auth Server.");

            string email = user.HoardId;
            if (email == "")
                email = await Options.UserInputProvider.RequestInput(user, eUserInputType.kEmail, "email");

            string password = await Options.UserInputProvider.RequestInput(user, eUserInputType.kPassword, "password");

            var createRequest = new RestRequest("/create_user", Method.POST);
            createRequest.RequestFormat = DataFormat.Json;
            createRequest.AddBody(new { email, password });
            var createResponse = authClient.Execute(createRequest);

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

            AuthToken token = null;
            //check if we have valid auth token
            if (!userAuthTokens.TryGetValue(user, out token) || !token.IsValid())
            {
                //none found ask for a new one
                token = await RequestAuthToken(user);
                //store
                if (token != null)
                    userAuthTokens[user] = token;
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
            string password = await Options.UserInputProvider.RequestInput(user, eUserInputType.kPassword, "password");

            var tokenRequest = new RestRequest("/token", Method.POST);
            tokenRequest.AddParameter("grant_type", "password");
            tokenRequest.AddParameter("username", user.HoardId);
            tokenRequest.AddParameter("password", password);
            tokenRequest.AddParameter("client_id", Options.HoardAuthServiceClientId);

            DateTime tokenExpireDate = DateTime.UtcNow;

            var tokenResponse = await authClient.ExecuteTaskAsync(tokenRequest);

            if (tokenResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                TokenResponse tokenResponseObj = JsonConvert.DeserializeObject<TokenResponse>(tokenResponse.Content);

                AuthToken authToken = new AuthToken();
                authToken.AccessToken = tokenResponseObj.access_token;
                authToken.ExpireTime = tokenExpireDate.AddSeconds(Convert.ToInt32(tokenResponseObj.expires_in));

                return authToken;
            }

            System.Diagnostics.Trace.Fail("could not authenticate user!");

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
