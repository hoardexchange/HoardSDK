using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace Hoard
{
    public class HoardAccountService : IAccountService
    {
        RestClient authClient = null;
        HoardServiceOptions Options = null;
        Dictionary<User, AuthToken> userAuthTokens = new Dictionary<User, AuthToken>();

        class AuthToken
        {
            public string access_token;
            public string token_type;
            public string expires_in;

            public bool IsValid()
            {
                return true;//TODO: implement
            }
        }

        public HoardAccountService(HoardServiceOptions options)
        {
            Options = options;
            authClient = new RestClient(options.HoardAuthServiceUrl);
        }

        public Task<bool> CreateAccount(string name, User user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RequestAccounts(User user)
        {
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
                return true;
            }

            return false;
        }

        public Task<string> Sign(byte[] input, AccountInfo signature)
        {
            throw new NotImplementedException();
        }

        private async Task<AuthToken> RequestAuthToken(User user)
        {
            string password = await Options.UserInputProvider.RequestInput(user, eUserInputType.kPassword, "password");

            var tokenRequest = new RestRequest("/token", Method.POST);
            tokenRequest.AddParameter("grant_type", "password");
            tokenRequest.AddParameter("username", user.UserName);
            tokenRequest.AddParameter("password", password);

            var tokenResponse = await authClient.ExecuteTaskAsync(tokenRequest);

            if (tokenResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                AuthToken authToken = JsonConvert.DeserializeObject<AuthToken>(tokenResponse.Content);
                return authToken;
            }

            System.Diagnostics.Trace.Fail("could not authenticate user!");

            return null;
        }
    }
}
