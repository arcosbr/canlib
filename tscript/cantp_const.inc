variables
{
    const int RET_CANTP_NO_ERR           = 0;
    const int RET_CANTP_ERR_PARAM        = -1;
    const int RET_CANTP_ERR_BUSY         = -2;

    const int CANTP_SF                   = 0;
    const int CANTP_FF                   = 1;
    const int CANTP_CF                   = 2;
    const int CANTP_FC                   = 3;
    const int CANTP_FF_NOFC              = 5;
    const int CANTP_CF_NOFC              = 6;

    const int CANTP_STATE_UNINITIALIZED  = -1;
    const int CANTP_STATE_IDLE           = 0;
    const int CANTP_STATE_TX             = 1;
    const int CANTP_STATE_RX             = 2;
    const int CANTP_STATE_RX_DONE        = 3;

    const int CANTP_TX_ERR_MASK          = 0x00000FFF;
    const int CANTP_RX_ERR_MASK          = 0x00FFF000;
    const int CANTP_N_OK                 = 0x00000000;
    const int CANTP_N_TIMEOUT_A          = 0x00001001;
    const int CANTP_N_TIMEOUT_Bs         = 0x00000002;
    const int CANTP_N_TIMEOUT_Cr         = 0x00002000;
    const int CANTP_N_WRONG_SN           = 0x00004000;
    const int CANTP_N_INVALID_FS         = 0x00000004;
    const int CANTP_N_UNEXP_PDU          = 0x00008000;
    const int CANTP_N_WFT_OVRN           = 0x00010010;
    const int CANTP_N_BUFFER_OVFLW       = 0x00000008;
    const int CANTP_N_ERROR              = 0x01000000;

    const int DRIVER_EXT_FLAG            = 0x80000000;

    const int CANTP_ISO15765_TX = 1;
    const int CANTP_ISO15765_RX = 2;
    const int CANTP_ISO15765_FF = 3;

    const int CANTP_ID                   = 0;
    const int CANTP_ISO15765_BS          = 1;
    const int CANTP_ISO15765_STMIN       = 2;
    const int CANTP_ISO15765_TX_TIMEOUT  = 3;
    const int CANTP_ISO15765_RX_TIMEOUT  = 4;
    const int CANTP_ISO15765_WFTMAX      = 5;
    const int CANTP_ISO15765_MASK        = 6;
    const int CANTP_ISO15765_CODE        = 7;
    const int CANTP_ISO15765_CHANNEL     = 8;
}
