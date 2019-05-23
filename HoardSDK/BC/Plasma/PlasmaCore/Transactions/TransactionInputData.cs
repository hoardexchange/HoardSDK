using Nethereum.RLP;
using Newtonsoft.Json;
using System;
using System.Numerics;

namespace PlasmaCore.Transactions
{
    /// <summary>
    /// Description of plasma transaction input data
    /// </summary>
    public class TransactionInputData
    {
        /// <summary>
        /// Block number
        /// </summary>
        [JsonProperty(propertyName: "blknum")]
        public UInt64 BlkNum { get; set; }

        /// <summary>
        /// Transaction index within the block
        /// </summary>
        [JsonProperty(propertyName: "txindex")]
        public UInt16 TxIndex { get; set; }

        /// <summary>
        /// Transaction output index
        /// </summary>
        [JsonProperty(propertyName: "oindex")]
        public UInt16 OIndex { get; set; }

        /// <summary>
        /// Creates empty transaction input data
        /// </summary>
        public TransactionInputData()
        {
            BlkNum = 0;
            TxIndex = 0;
            OIndex = 0;
        }

        /// <summary>
        /// Creates base transaction input data
        /// </summary>
        /// <param name="blkNum"></param>
        /// <param name="txIndex"></param>
        /// <param name="oIndex"></param>
        public TransactionInputData(UInt64 blkNum, UInt16 txIndex, UInt16 oIndex)
        {
            BlkNum = blkNum;
            TxIndex = txIndex;
            OIndex = oIndex;
        }

        /// <summary>
        /// Returns rlp encoded transaction input data
        /// </summary>
        /// <returns></returns>
        public byte[] GetRLPEncoded()
        {
            var data = new byte[3][];
            data[0] = RLP.EncodeElement(new BigInteger(BlkNum).ToBytesForRLPEncoding());
            data[1] = RLP.EncodeElement(new BigInteger(TxIndex).ToBytesForRLPEncoding());
            data[2] = RLP.EncodeElement(new BigInteger(OIndex).ToBytesForRLPEncoding());
            return RLP.EncodeList(data);
        }

        /// <summary>
        /// Returns if transaction input data is empty
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return (BlkNum == 0 && TxIndex == 0 && OIndex == 0);
        }
    }
}
