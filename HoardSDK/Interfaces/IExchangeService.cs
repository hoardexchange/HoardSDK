using Hoard;
using HoardSDK.ExchangeServices;
using System.Threading.Tasks;

namespace HoardSDK.Interfaces
{
    public interface IExchangeService
    {
        User User { get; set; }

        bool Init();

        Task<bool> Deposit(GameItem item, ulong amount);

        Task<Order[]> ListOrdersAsync(GameItem gaGet, GameItem gaGive, AccountInfo account);

        Task<bool> Order(GameItem getItem, GameItem giveItem, ulong blockTimeDuration);

        Task<bool> Trade(Order order);

        Task<bool> Withdraw(GameItem item);
    }
}
