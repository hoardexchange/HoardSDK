namespace Hoard.HW.Ledger.Ethereum
{
    internal class EthConstants
    {
        public const byte EMPTY = 0x00;
        public const byte CLA = 0xe0;

        public const byte INS_GET_PUBLIC_ADDRESS = 0x02;
        public const byte P1_NON_CONFIRM = 0x00;
        public const byte P1_CONFIRM = 0x01;
        public const byte P2_NO_CHAINCODE = 0x00;
        public const byte P2_CHAINCODE = 0x01;

        public const byte INS_SIGN_TRANSACTION = 0x04;
        public const byte INS_SIGN_PERSONAL_MESSAGE = 0x08;
        public const byte P1_FIRST_BLOCK = 0x00;
        public const byte P1_SUBSEQUENT_BLOCK = 0x80;

        public const int SW_INCORRECT_LENTGTH = 0x6700;
        public const int SW_SECURITY_STATUS_NOT_SATISFIED = 0x6982;
        public const int SW_INVALID_DATA = 0x6A80;
        public const int SW_INCORRECT_P1_OR_P2 = 0x6B00;
        public const int SW_INTERNAL_ERROR = 0x6F00;
        public const int SW_OK = 0x9000;
    }
}
