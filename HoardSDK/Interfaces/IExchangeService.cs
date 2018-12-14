using Hoard;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// TODO: document this class
    /// </summary>
    public interface IExchangeService
    {
        User User { get; set; }

        bool Init();

        Task<bool> Deposit(GameItem item, ulong amount);

        Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive, AccountInfo account);

        Task<bool> Order(GameItem getItem, GameItem giveItem, ulong blockTimeDuration);

        Task<bool> Trade(Order order);

        Task<bool> Withdraw(GameItem item);
    }
}
