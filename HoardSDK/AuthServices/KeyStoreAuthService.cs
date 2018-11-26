using Hoard.Utils;
using Nethereum.Web3.Accounts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    public class KeyStoreAuthService : IAuthService
    {
        public string Name { get; } = "KeyStoreAuthService";

        private HoardServiceOptions Options = null;

        public KeyStoreAuthService(HoardServiceOptions options)
        {
            Options = options;
        }

        public async Task<User> LoginUser()
        {
            //ask for user login
            string userId = await Options.UserInputProvider.RequestInput(null, eUserInputType.kLogin, "login");

            User user = new User(userId);

            bool foundUserAccount = false;

            KeyStoreUtils.EnumerateAccounts(userId, (string accountId) =>
            {
                foundUserAccount = true;
            });

            if (!foundUserAccount)
            {
                //ask for password
                string password = await Options.UserInputProvider.RequestInput(null, eUserInputType.kPassword, "password");

                //ask for name
                string name = "default";

                //no accounts so let's create new one
                KeyStoreUtils.CreateAccount(user, name, password);
            }

            return user;
        }
    }
}
