using Newtonsoft.Json;
using System.Numerics;

namespace PlasmaCore.RPC.OutputData
{
    public class StatusData
    {
        public class ByzantineEventsData
        {
            public class EventDetailsData
            {
                [JsonProperty(propertyName: "eth_height")]
                public ulong EthHeight { get; private set; }

                [JsonProperty(propertyName: "utxo_pos")]
                public BigInteger Position { get; private set; }

                [JsonProperty(propertyName: "owner")]
                public string Owner { get; private set; }

                [JsonProperty(propertyName: "currency")]
                public string Currency { get; private set; }

                [JsonProperty(propertyName: "amount")]
                public string Amount { get; private set; }
            }

            [JsonProperty(propertyName: "event")]
            public string Event { get; private set; }

            [JsonProperty(propertyName: "details")]
            public EventDetailsData Details { get; private set; }
        }

        public class InFlightTxsData
        {
            [JsonProperty(propertyName: "txhash")]
            public string TxHash { get; private set; }

            [JsonProperty(propertyName: "txbytes")]
            public string TxBytes { get; private set; }

            [JsonProperty(propertyName: "input_addresses")]
            public string[] InputAddresses { get; private set; }

            [JsonProperty(propertyName: "ouput_addresses")]
            public string[] OutputAddresses { get; private set; }
        }

        public class InFlightExitsData
        {
            [JsonProperty(propertyName: "txhash")]
            public string TxHash { get; private set; }

            [JsonProperty(propertyName: "txbytes")]
            public string TxBytes { get; private set; }

            [JsonProperty(propertyName: "eth_height")]
            public ulong EthHeight { get; private set; }

            [JsonProperty(propertyName: "piggybacked_inputs")]
            public ulong[] PiggybackedInputs { get; private set; }

            [JsonProperty(propertyName: "piggybacked_outputs")]
            public ulong[] PiggybackedOutputs { get; private set; }
        }

        public class ServicesSyncedHeightsData
        {
            [JsonProperty(propertyName: "service")]
            public string Service { get; private set; }

            [JsonProperty(propertyName: "height")]
            public ulong Height { get; private set; }
        }

        [JsonProperty(propertyName: "last_validated_child_block_timestamp")]
        public ulong LastValidatedChildBlockTimestamp { get; private set; }

        [JsonProperty(propertyName: "last_validated_child_block_number")]
        public ulong LastValidatedChildBlockNumber { get; private set; }

        [JsonProperty(propertyName: "last_mined_child_block_timestamp")]
        public ulong LastMinedChildBlockTimestamp { get; private set; }

        [JsonProperty(propertyName: "last_mined_child_block_number")]
        public ulong LastMinedChildBlockNumber { get; private set; }

        [JsonProperty(propertyName: "last_seen_eth_block_timestamp")]
        public ulong LastSeenEthBlockTimestamp { get; private set; }

        [JsonProperty(propertyName: "last_seen_eth_block_number")]
        public ulong LastSeenEthBlockNumber { get; private set; }

        [JsonProperty(propertyName: "contract_addr")]
        public string ContractAddr { get; private set; }

        [JsonProperty(propertyName: "eth_syncing")]
        public bool EthSyncing { get; private set; }

        [JsonProperty(propertyName: "byzantine_events")]
        public ByzantineEventsData[] ByzantineEvents { get; private set; }

        [JsonProperty(propertyName: "in_flight_txs")]
        public InFlightTxsData[] InFlightTxs { get; private set; }

        [JsonProperty(propertyName: "in_flight_exits")]
        public InFlightExitsData[] InFlightExits { get; private set; }

        [JsonProperty(propertyName: "services_synced_heights")]
        public ServicesSyncedHeightsData[] ServicesSyncedHeights { get; private set; }
    }
}
