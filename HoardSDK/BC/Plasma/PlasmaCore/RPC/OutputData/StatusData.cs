using Newtonsoft.Json;
using System.Numerics;

namespace PlasmaCore.RPC.OutputData
{
    /// <summary>
    /// RPC status data 
    /// </summary>
    public class StatusData
    {
        /// <summary>
        /// Events data
        /// </summary>
        public class ByzantineEventsData
        {
            /// <summary>
            /// Event details
            /// </summary>
            public class EventDetailsData
            {
                /// <summary>
                /// Block number on ethereum chain
                /// </summary>
                [JsonProperty(propertyName: "eth_height")]
                public ulong EthHeight { get; private set; }

                /// <summary>
                /// Position of the UTXO
                /// </summary>
                [JsonProperty(propertyName: "utxo_pos")]
                public BigInteger Position { get; private set; }

                /// <summary>
                /// Owner of UTXO
                /// </summary>
                [JsonProperty(propertyName: "owner")]
                public string Owner { get; private set; }

                /// <summary>
                /// Currency being exchanged
                /// </summary>
                [JsonProperty(propertyName: "currency")]
                public string Currency { get; private set; }

                /// <summary>
                /// Amount being exchanged
                /// </summary>
                [JsonProperty(propertyName: "amount")]
                public string Amount { get; private set; }
            }

            /// <summary>
            /// Event object
            /// </summary>
            [JsonProperty(propertyName: "event")]
            public string Event { get; private set; }

            /// <summary>
            /// Details of the event
            /// </summary>
            [JsonProperty(propertyName: "details")]
            public EventDetailsData Details { get; private set; }
        }

        /// <summary>
        /// RPC message concerning InFlight transaction
        /// </summary>
        public class InFlightTxsData
        {
            /// <summary>
            /// Hash of the transaction
            /// </summary>
            [JsonProperty(propertyName: "txhash")]
            public string TxHash { get; private set; }

            /// <summary>
            /// Transaction data in bytes
            /// </summary>
            [JsonProperty(propertyName: "txbytes")]
            public string TxBytes { get; private set; }

            /// <summary>
            /// Input address
            /// </summary>
            [JsonProperty(propertyName: "input_addresses")]
            public string[] InputAddresses { get; private set; }

            /// <summary>
            /// Output address
            /// </summary>
            [JsonProperty(propertyName: "ouput_addresses")]
            public string[] OutputAddresses { get; private set; }
        }

        /// <summary>
        /// RPC message concerning InFlight exit
        /// </summary>
        public class InFlightExitsData
        {
            /// <summary>
            /// Hash of the transaction
            /// </summary>
            [JsonProperty(propertyName: "txhash")]
            public string TxHash { get; private set; }

            /// <summary>
            /// Transaction data in bytes
            /// </summary>
            [JsonProperty(propertyName: "txbytes")]
            public string TxBytes { get; private set; }

            /// <summary>
            /// Block number of ethereum chain
            /// </summary>
            [JsonProperty(propertyName: "eth_height")]
            public ulong EthHeight { get; private set; }

            /// <summary>
            /// List of inputs exiting
            /// </summary>
            [JsonProperty(propertyName: "piggybacked_inputs")]
            public ulong[] PiggybackedInputs { get; private set; }

            /// <summary>
            /// List of outputs exiting
            /// </summary>
            [JsonProperty(propertyName: "piggybacked_outputs")]
            public ulong[] PiggybackedOutputs { get; private set; }
        }

        /// <summary>
        /// RPC message about synchronization with ethereum chain
        /// </summary>
        public class ServicesSyncedHeightsData
        {
            /// <summary>
            /// The service synchronized
            /// </summary>
            [JsonProperty(propertyName: "service")]
            public string Service { get; private set; }

            /// <summary>
            /// Synchronized block number of ethereum chain
            /// </summary>
            [JsonProperty(propertyName: "height")]
            public ulong Height { get; private set; }
        }

        /// <summary>
        /// Timestamp of last validated block number on child chain
        /// </summary>
        [JsonProperty(propertyName: "last_validated_child_block_timestamp")]
        public ulong LastValidatedChildBlockTimestamp { get; private set; }

        /// <summary>
        /// Number of last validated block number on child chain
        /// </summary>
        [JsonProperty(propertyName: "last_validated_child_block_number")]
        public ulong LastValidatedChildBlockNumber { get; private set; }

        /// <summary>
        /// Timestamp of last mined block number on child chain
        /// </summary>
        [JsonProperty(propertyName: "last_mined_child_block_timestamp")]
        public ulong LastMinedChildBlockTimestamp { get; private set; }

        /// <summary>
        /// Number of last mined block number on child chain
        /// </summary>
        [JsonProperty(propertyName: "last_mined_child_block_number")]
        public ulong LastMinedChildBlockNumber { get; private set; }

        /// <summary>
        /// Timestamp of last seen block number on child chain
        /// </summary>
        [JsonProperty(propertyName: "last_seen_eth_block_timestamp")]
        public ulong LastSeenEthBlockTimestamp { get; private set; }

        /// <summary>
        /// Number of last seen block number on child chain
        /// </summary>
        [JsonProperty(propertyName: "last_seen_eth_block_number")]
        public ulong LastSeenEthBlockNumber { get; private set; }

        /// <summary>
        /// Plasma contract address
        /// </summary>
        [JsonProperty(propertyName: "contract_addr")]
        public string ContractAddr { get; private set; }

        /// <summary>
        /// If syncing with ethereum
        /// </summary>
        [JsonProperty(propertyName: "eth_syncing")]
        public bool EthSyncing { get; private set; }

        /// <summary>
        /// List of events
        /// </summary>
        [JsonProperty(propertyName: "byzantine_events")]
        public ByzantineEventsData[] ByzantineEvents { get; private set; }

        /// <summary>
        /// InFlight transactions
        /// </summary>
        [JsonProperty(propertyName: "in_flight_txs")]
        public InFlightTxsData[] InFlightTxs { get; private set; }

        /// <summary>
        /// InFlight exitst
        /// </summary>
        [JsonProperty(propertyName: "in_flight_exits")]
        public InFlightExitsData[] InFlightExits { get; private set; }

        /// <summary>
        /// Synchronization data list
        /// </summary>
        [JsonProperty(propertyName: "services_synced_heights")]
        public ServicesSyncedHeightsData[] ServicesSyncedHeights { get; private set; }
    }
}
