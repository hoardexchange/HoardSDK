using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hoard.BC.Contracts
{
    public class ContractInterfaceID
    {
        /// <summary>
        /// InterfaceID stored in 4 bytes
        /// </summary>
        public byte[] InterfaceID { get; }

        /// <summary>
        /// Contract type connected with interface ID
        /// </summary>
        public Type ContractType { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="interfaceID">interfaceID 4 bytes represented in hex</param>
        public ContractInterfaceID(string interfaceID, Type contractType)
        {
            InterfaceID = BitConverter.GetBytes(Convert.ToUInt32(interfaceID, 16));

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(InterfaceID);
            }

            ContractType = contractType;
        }
    }

}
