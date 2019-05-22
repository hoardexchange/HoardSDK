﻿using Newtonsoft.Json.Linq;
using PlasmaCore.RPC.OutputData;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace PlasmaCore.RPC.UTXO
{
    /// <summary>
    /// RPC request handler - fetches exit data for a given utxo
    /// </summary>
    public class GetExitData : RPCRequestHandlerBase
    {
        static private string route = "utxo.get_exit_data";

        /// <summary>
        /// Constructs RPC request handler
        /// </summary>
        /// <param name="client">RPC client</param>
        public GetExitData(IClient client) : base(client) { }

        /// <summary>
        /// Returns exit data for a given utxo
        /// </summary>
        /// <param name="position">utxo position</param>
        /// <returns></returns>
        public async Task<ExitData> SendRequestAsync(BigInteger position)
        {
            if (position == null) throw new ArgumentNullException(nameof(position));
            
            RPCRequest request = new RPCRequest(route);
            request.Parameters.Add("utxo_pos", new JValue(position));

            return await client.SendRequestAsync<ExitData>(request);
        }
    }
}
