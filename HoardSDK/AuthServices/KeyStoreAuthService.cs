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

        public async Task<User> CreateUser()
        {
            //ask for user login
            string userId = await Options.UserInputProvider.RequestInput(null, eUserInputType.kLogin, "login");
            return new User(userId);
        }

        public async Task<User> LoginUser()
        {
            //ask for user login
            string userId = await Options.UserInputProvider.RequestInput(null, eUserInputType.kLogin, "login");
            string hashedName = Helper.SHA256HexHashString(userId);
            //now look for existing account folder (so we know that user exists)
            if (!Directory.Exists(Options.AccountsDir))
            {
                Trace.TraceWarning("Not found Hoard user directory.");
                return null;
            }

            var accountsDirs = Directory.GetDirectories(Options.AccountsDir);
            if (accountsDirs.Length == 0)
            {
                Trace.TraceWarning("Not found any Hoard users.");
                return null;
            }

            foreach (var dirName in accountsDirs)
            {
                //if found return user with proper id
                string[] parts = dirName.Split(Path.DirectorySeparatorChar);
                if (parts.Length>0 && parts.Last() == hashedName)
                    return new User(userId);
            }
            
            //if not found return null
            return null;
        }
    }
}
