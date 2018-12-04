using System.Threading.Tasks;

namespace Hoard
{
    public enum eUserInputType
    {
        kLogin = 0,
        kPassword,
        kPIN,
        kEmail,
        kCustom,

        kMaxUserInputValue
    }

    public interface IUserInputProvider
    {
        Task<string> RequestInput(User user, eUserInputType type, string description);
    }
}
