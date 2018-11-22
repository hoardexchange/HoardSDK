using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    public interface IAccountService
    {
        bool CreateAccount(HoardServiceOptions options, string username, string password);
        bool LoadAccounts(HoardServiceOptions options, string username, string password);
        bool LoadAccountInplace(string address, string key, string password);
        List<AccountInfo> GetAccounts();
    }
}
