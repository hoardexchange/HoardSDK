using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard
{
    public interface IAuthService
    {
        User CreateUser(string userName, string password);
        User LoginUser(string userName, string password);
    }
}
