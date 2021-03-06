﻿using Hoard.ExchangeServices;
using System.Numerics;
using System.Threading.Tasks;

namespace Hoard
{
    /// <summary>
    /// Access to Hoard Exchange services
    /// </summary>
    public interface IExchangeService
    {
        /// <summary>
        /// Connects to Exchange service
        /// </summary>
        /// <returns></returns>
        Task Init();

        /// <summary>
        /// TODO: document this! make parameters human readable!!!
        /// </summary>
        /// <param name="gaGet"></param>
        /// <param name="gaGive"></param>
        /// <param name="account"></param>
        /// <returns></returns>
        Task<Order[]> ListOrders(GameItem gaGet, GameItem gaGive, Profile account);

        /// <summary>
        /// Deposit Item to be sold.
        /// </summary>
        /// <param name="profile">seller account</param>
        /// <param name="item">Item to sell</param>
        /// <param name="amount">amount of Items to sell (for NFT it must be 1)</param>
        /// <returns></returns>
        Task<bool> Deposit(Profile profile, GameItem item, BigInteger amount);

        /// <summary>
        /// Creates a trade order for tokens in currency expressed in ERC233 tokens
        /// </summary>
        /// <param name="profile">seller profile</param>
        /// <param name="getItem">currency token</param>
        /// <param name="giveItem">item to sell</param>
        /// <param name="blockTimeDuration">duration of order</param>
        /// <returns>true if order has been created, false otherwise</returns>
        Task<bool> Order(Profile profile, GameItem getItem, GameItem giveItem, ulong blockTimeDuration);

        /// <summary>
        /// Accepts an order by paying the price
        /// </summary>
        /// <param name="profile">buyer profile</param>
        /// <param name="order">order to pay for</param>
        /// <returns></returns>
        Task<bool> Trade(Profile profile, Order order);

        /// <summary>
        /// Retrieves the payed amount back to the seller
        /// </summary>
        /// <param name="profile">creator of order</param>
        /// <param name="item">amount to withdraw</param>
        /// <returns></returns>
        Task<bool> Withdraw(Profile profile, GameItem item);
    }
}
