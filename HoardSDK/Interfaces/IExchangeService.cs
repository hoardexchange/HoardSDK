using Hoard.ExchangeServices;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// TODO: document this class
    /// </summary>
    public interface IExchangeService
    {
        bool Init();

        Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive, AccountInfo account);

        Task<bool> Deposit(AccountInfo account, GameItem item, ulong amount);

        Task<bool> Order(AccountInfo account, GameItem getItem, GameItem giveItem, ulong blockTimeDuration);

        Task<bool> Trade(AccountInfo account, Order order);

        Task<bool> Withdraw(AccountInfo account, GameItem item);
    }
}
