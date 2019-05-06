﻿using Hoard.Interfaces;
using System;
using System.Diagnostics;

namespace Hoard.BC
{
    /// <summary>
    /// Factory of blockchain communication channels
    /// </summary>
    public class BCCommFactory
    {
        /// <summary>
        /// Creates blockchain communication channel based on given Hoard service options
        /// </summary>
        /// <param name="options">Hoard service options containing blockchain communication data</param>
        /// <returns>Specialized blockchain communication instance</returns>
        public static IBCComm Create(HoardServiceOptions options)
        {
            if (options.BCClientOptions is PlasmaClientOptions)
            {
                var clientOpts = options.BCClientOptions as PlasmaClientOptions;
                var bcComm = new BCComm(clientOpts.RpcClient, options.GameCenterContract);
                return new PlasmaComm(bcComm, clientOpts.ChildChainUrl, clientOpts.WatcherUrl);
            }
            else if (options.BCClientOptions is EthereumClientOptions)
            {
                var clientOpts = options.BCClientOptions as EthereumClientOptions;
                return new BCComm(clientOpts.RpcClient, options.GameCenterContract);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}