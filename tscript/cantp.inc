#include <cantp_defs.inc>
#include <cantp_const.inc>

variables {
    const int MAX_CANTP_SESSIONS = 10;
}

variables {
    myCanTpInfo canTpSessions[MAX_CANTP_SESSIONS];
}

on CanMessage<*> *
{
    int i;

    for (i = 0; i < MAX_CANTP_SESSIONS; i++)
    {
        if ((this.id & canTpSessions[i].mask) ==
            (canTpSessions[i].code & canTpSessions[i].mask))
        {
            int ft, dl, rl;

            if (canTpSessions[i].eid &&
                (this.data[0] != canTpSessions[i].rx_eid))
                continue;

            if (canTpSessions[i].channel != this.channel)
                continue;

            timerCancel(canTpSessions[i].receive_timeout_timer);
            ft = (this.data[canTpSessions[i].eid] & 0xf0) >> 4;

            if (canTpSessions[i].func && (ft != CANTP_SF))
                continue;

            if (canTpSessions[i].state == CANTP_STATE_TX)
            {
                if (canTpSessions[i].pos >= canTpSessions[i].length)
                {
                    canTpSessions[i].state = CANTP_STATE_IDLE;
                    canTpSessions[i].transmit_result = CANTP_N_OK;
                    txIndication(i);
                }
            }

            if (canTpSessions[i].fixed)
            {
                canTpSessions[i].rx_id_last = this.id & 0xff;
            }
            else
            {
                canTpSessions[i].rx_id_last = this.id;
            }

            switch (ft)
            {
            case CANTP_SF: {
                dl = this.data[canTpSessions[i].eid] & 0x0f;

                if ((dl == 0) || (dl >= (8 - canTpSessions[i].eid)) ||
                    (this.dlc < (dl + 1 + canTpSessions[i].eid)))
                {
                    break;
                }

                if (canTpSessions[i].state != CANTP_STATE_IDLE)
                {
                    printf("SF recv not idle (%d)\n", canTpSessions[i].state);
                }

                if (canTpSessions[i].state == CANTP_STATE_RX)
                {
                    canTpSessions[i].state = CANTP_STATE_IDLE;
                    canTpSessions[i].receive_result =
                        CANTP_N_UNEXP_PDU & CANTP_RX_ERR_MASK;
                    rxIndication(i);
                }

                if (canTpSessions[i].state != CANTP_STATE_TX)
                {
                    canTpSessions[i].buffer[0, dl] =
                        this.data[1 + canTpSessions[i].eid, dl];
                    canTpSessions[i].length = dl;
                    canTpSessions[i].pos = dl;
                    canTpSessions[i].state = CANTP_STATE_RX_DONE;
                    rxIndication(i);
                }
            }
            break;
            case CANTP_FF:
            case CANTP_FF_NOFC: {
                canTpSessions[i].length =
                    ((this.data[canTpSessions[i].eid] & 0x0f) << 8) +
                    this.data[canTpSessions[i].eid + 1];

                if ((canTpSessions[i].length <= (7 - canTpSessions[i].eid)) ||
                    (this.dlc != 8))
                {
                    break;
                }

                if (canTpSessions[i].state != CANTP_STATE_IDLE)
                {
                    printf("FF recv not idle (%d)\n", canTpSessions[i].state);
                }
                if (canTpSessions[i].state == CANTP_STATE_RX)
                {
                    canTpSessions[i].state = CANTP_STATE_IDLE;
                    canTpSessions[i].receive_result =
                        CANTP_N_UNEXP_PDU & CANTP_RX_ERR_MASK;
                    rxIndication(i);
                }

                if (canTpSessions[i].state != CANTP_STATE_TX)
                {
                    // ignore otherwise
                    canTpSessions[i]
                        .buffer[0, this.dlc - 2 - canTpSessions[i].eid] =
                        this.data[2 + canTpSessions[i].eid,
                                  this.dlc - 2 - canTpSessions[i].eid];
                    canTpSessions[i].pos = this.dlc - 2 - canTpSessions[i].eid;
                    canTpSessions[i].blocks_remaining = 0;
                    canTpSessions[i].sn = 1;

                    if (ft != CANTP_FF_NOFC)
                        cantp_transmit_fc(i);

                    canTpSessions[i].state = CANTP_STATE_RX;
                    canTpSessions[i].receive_result = CANTP_N_OK;

                    ffIndication(i);
                    timerStart(canTpSessions[i].receive_timeout_timer, 1);
                }
            }
            break;
            case CANTP_CF:
            case CANTP_CF_NOFC: {
                if (canTpSessions[i].state != CANTP_STATE_RX)
                {
                    printf("CF recv not in recv (%d)\n",
                           canTpSessions[i].state);
                    break;
                }

                rl = canTpSessions[i].length - canTpSessions[i].pos;

                if ((this.dlc != 8) && (rl > (7 - canTpSessions[i].eid)) ||
                    (rl > (this.dlc - 1 - canTpSessions[i].eid)))
                {
                    printf("Invalid DLC, CAN frame ignored\n");
                    break;
                }

                if (canTpSessions[i].sn !=
                    (this.data[canTpSessions[i].eid] & 0x0f))
                {
                    canTpSessions[i].state = CANTP_STATE_IDLE;
                    canTpSessions[i].receive_result =
                        CANTP_N_WRONG_SN & CANTP_RX_ERR_MASK;
                    rxIndication(i);
                    printf("Invalid Serial Number in CF\n");
                    break;
                }

                if (rl > (this.dlc - 1 - canTpSessions[i].eid))
                {
                    rl = this.dlc - 1 - canTpSessions[i].eid;
                }

                if (rl + canTpSessions[i].pos > CANTP_BUFFER_MAX)
                {
                    rl = CANTP_BUFFER_MAX - canTpSessions[i].pos;
                }

                canTpSessions[i].buffer[canTpSessions[i].pos, rl] =
                    this.data[1 + canTpSessions[i].eid, rl];
                canTpSessions[i].pos += rl;

                if (canTpSessions[i].pos >= canTpSessions[i].length)
                {
                    canTpSessions[i].state = CANTP_STATE_RX_DONE;
                }
                else
                {
                    canTpSessions[i].sn = (canTpSessions[i].sn + 1) & 0x0f;

                    if (ft == CANTP_CF)
                    {
                        if (canTpSessions[i].blocks_remaining)
                        {
                            canTpSessions[i].blocks_remaining--;
                            if (canTpSessions[i].blocks_remaining == 0)
                            {
                                cantp_transmit_fc(i);
                            }
                        }
                    }
                }
            }
            break;
            case CANTP_FC: {
                if (canTpSessions[i].state != CANTP_STATE_TX)
                {
                    printf("FC recv not trans (%d)\n", canTpSessions[i].state);
                    break;
                }

                if (this.dlc < 3)
                {
                    break;
                }

                if (canTpSessions[i].blocks_remaining != 0)
                {
                    printf("Unexpected FC\n");
                    break;
                }

                switch (this.data[canTpSessions[i].eid] & 0x0f)
                {
                case 0: {
                    canTpSessions[i].blocks_remaining =
                        this.data[canTpSessions[i].eid + 1];
                    if (canTpSessions[i].blocks_remaining != 0)
                    {
                        canTpSessions[i].blocks_remaining = 0xffff;
                    }
                    canTpSessions[i].wft = 0;
                }
                break;
                case 1: {
                    canTpSessions[i].wft++;
                    if (canTpSessions[i].wft > canTpSessions[i].wft_max)
                    {
                        canTpSessions[i].state = CANTP_STATE_IDLE;
                        canTpSessions[i].transmit_result =
                            CANTP_N_WFT_OVRN & CANTP_TX_ERR_MASK;
                        txIndication(i);
                        return;
                    }
                    canTpSessions[i].blocks_remaining = 0;
                }
                break;
                case 2: {
                    canTpSessions[i].state = CANTP_STATE_IDLE;
                    canTpSessions[i].transmit_result =
                        CANTP_N_BUFFER_OVFLW & CANTP_TX_ERR_MASK;
                    txIndication(i);
                    return;
                }
                break;
                default: {
                    canTpSessions[i].state = CANTP_STATE_IDLE;
                    canTpSessions[i].transmit_result =
                        CANTP_N_INVALID_FS & CANTP_TX_ERR_MASK;
                    txIndication(i);
                    return;
                }
                }

                if (this.data[canTpSessions[i].eid + 2] <= 0x7f)
                {
                    canTpSessions[i].cf_delay_timer.timeout =
                        this.data[canTpSessions[i].eid + 2];
                }
                else if ((this.data[canTpSessions[i].eid + 2] >= 0xf1) &&
                         (this.data[canTpSessions[i].eid + 2] <= 0xf9))
                {
                    canTpSessions[i].cf_delay_timer.timeout = 1;
                }
                else
                {
                    canTpSessions[i].cf_delay_timer.timeout = 0x7f;
                }

                if (canTpSessions[i].cf_delay_timer.timeout == 0)
                {
                    int num_msgs =
                        canTpSessions[i].length / (7 - canTpSessions[i].eid);
                    while (num_msgs-- > 0)
                    {
                        cantp_transmit_cf(i);
                    }
                }
                else
                {
                    timerStart(canTpSessions[i].cf_delay_timer, 1);
                }
            }
            break;
            }
        }
    }
} // on CanMessage

void canTpInit(void)
{
    int i;
    for (i = 0; i < MAX_CANTP_SESSIONS; i++)
    {
        canTpSessions[i].state = CANTP_STATE_UNINITIALIZED;
        canTpSessions[i].transmit_timeout_timer.id = i;
        canTpSessions[i].transmit_timeout_timer.timeout = 1000;
        canTpSessions[i].receive_timeout_timer.id = i;
        canTpSessions[i].receive_timeout_timer.timeout = 1000;
        canTpSessions[i].cf_delay_timer.id = i;
        canTpSessions[i].cf_delay_timer.timeout = 1;
        canTpSessions[i].receive_bs = 0;
        canTpSessions[i].fc_enabled = 1;
        canTpSessions[i].cf_delay_rx = 0;
        canTpSessions[i].flags = 0;
        canTpSessions[i].mask = 0xffffffff;
        canTpSessions[i].channel = 0;

        timerSetHandler(canTpSessions[i].transmit_timeout_timer,
                        "Transmit_Timeout");
        timerSetHandler(canTpSessions[i].receive_timeout_timer,
                        "Receive_Timeout");
        timerSetHandler(canTpSessions[i].cf_delay_timer, "CF_Delay");
    }
}

int canTpOpen(int &handle, int rxid, int txid, const char protocol[])
{
    return canTpOpen(&handle, rxid, txid, protocol, 0x000200, 0x000200);
}

int canTpOpen(int &handle, int rxid, int txid, const char protocol[],
              int addr_mode_rx)
{
    return canTpOpen(&handle, rxid, txid, protocol, addr_mode_rx, addr_mode_rx);
}

int canTpOpen(int &handle, int rxid, int txid, const char protocol[],
              int addr_mode_rx, int addr_mode_tx)
{
    int mask = 0xffffffff;
    int func = 0;
    byte fixed = 0;
    int i;

    for (i = 0; i < MAX_CANTP_SESSIONS; i++)
    {
        if (canTpSessions[i].state == CANTP_STATE_UNINITIALIZED)
        {
            break;
        }
    }

    if (i == MAX_CANTP_SESSIONS)
    {
        printf("Cannot open more than %d\n can tp sessions\n",
               MAX_CANTP_SESSIONS);
        return RET_CANTP_ERR_PARAM;
    }

    if (!strcmp(protocol, "ISO-15765"))
    {
        switch (addr_mode_rx >> 8)
        {
        case 0x0200: //              Fixed
        case 0x0210: //              Fixed Physical
        case 0x0212: //              Fixed Physical 29 bit
        case 0x0310: //       Normal Fixed Physical
        case 0x0312: //       Normal Fixed Physical 29 bit
        case 0x1210: // Local        Fixed Physical
        case 0x1212: // Local        Fixed Physical 29 bit
        case 0x1310: // Local Normal Fixed Physical
        case 0x1312: // Local Normal Fixed Physical 29 bit
            if ((addr_mode_rx != addr_mode_tx) || (addr_mode_rx & 0xff))
            {
                return RET_CANTP_ERR_PARAM;
            }

            if ((rxid > 0xff) || (txid > 0xff))
            {
                return RET_CANTP_ERR_PARAM;
            }

            canTpSessions[i].tx_id = 0x18da0000 | (rxid << 8) | txid;
            canTpSessions[i].rx_id = 0x18da0000 | (txid << 8) | rxid;
            canTpSessions[i].flags |= canMSG_EXT;
            fixed = 1;
            break;

        case 0x0220: //              Fixed Functional
        case 0x0222: //              Fixed Functional 29 bit
        case 0x0320: //       Normal Fixed Functional
        case 0x0322: //       Normal Fixed Functional 29 bit
        case 0x1220: // Local        Fixed Functional
        case 0x1222: // Local        Fixed Functional 29 bit
        case 0x1320: // Local Normal Fixed Functional
        case 0x1322: // Local Normal Fixed Functional 29 bit
            if ((addr_mode_rx != addr_mode_tx) || (addr_mode_rx & 0xff))
            {
                return RET_CANTP_ERR_PARAM;
            }

            if ((rxid > 0xff) || (txid > 0xff))
            {
                return RET_CANTP_ERR_PARAM;
            }

            canTpSessions[i].tx_id = 0x18db0000 | (rxid << 8) | txid;
            canTpSessions[i].rx_id = 0x18db0000 | (txid << 8) | rxid;
            canTpSessions[i].flags |= canMSG_EXT;

            mask = 0xfffeff00;
            func = 1;
            fixed = 1;
            // qqq Mask
            break;

        case 0x0400: //       Extended
        case 0x0402: //       Extended          29 bit
        case 0x0410: //       Extended Physical
        case 0x0412: //       Extended Physical 29 bit
        case 0x1400: // Local Extended
        case 0x1402: // Local Extended          29 bit
        case 0x1410: // Local Extended Physical
        case 0x1412: // Local Extended Physical 29 bit
            if ((addr_mode_rx & ~0xff) != (addr_mode_tx & ~0xff))
            {
                return RET_CANTP_ERR_PARAM;
            }

            if ((rxid > 0x1fffffff) || (txid > 0x1fffffff))
            {
                return RET_CANTP_ERR_PARAM;
            }

            canTpSessions[i].rx_id = rxid;
            canTpSessions[i].tx_id = txid;
            canTpSessions[i].flags |= canMSG_EXT;
            break;

        case 0x0401: //       Extended          11 bit
        case 0x0411: //       Extended Physical 11 bit
        case 0x1401: // Local Extended          11 bit
        case 0x1411: // Local Extended Physical 11 bit
            if ((addr_mode_rx & ~0xff) != (addr_mode_tx & ~0xff))
            {
                return RET_CANTP_ERR_PARAM;
            }

            if ((rxid > 0x7ff) || (txid > 0x7ff))
            {
                return RET_CANTP_ERR_PARAM;
            }

            canTpSessions[i].rx_id = rxid;
            canTpSessions[i].tx_id = txid;
            break;

        case 0x0810: //        Mixed Physical
        case 0x0812: //        Mixed Physical 29 bit
        case 0x1810: // Local  Mixed Physical
        case 0x1812: // Local  Mixed Physical 29 bit
        case 0x2810: // Remote Mixed Physical
        case 0x2812: // Remote Mixed Physical 29 bit
            if (addr_mode_rx != addr_mode_tx)
            {
                return RET_CANTP_ERR_PARAM;
            }

            if ((rxid > 0xff) || (txid > 0xff))
            {
                return RET_CANTP_ERR_PARAM;
            }

            canTpSessions[i].tx_id = 0x18ce0000 | (rxid << 8) | txid;
            canTpSessions[i].rx_id = 0x18ce0000 | (txid << 8) | rxid;
            canTpSessions[i].flags |= canMSG_EXT;
            fixed = 1;
            break;
        case 0x0820: //        Mixed Functional
        case 0x0822: //        Mixed Functional 29 bit
        case 0x1820: // Local  Mixed Functional
        case 0x1822: // Local  Mixed Functional 29 bit
        case 0x2820: // Remote Mixed Functional
        case 0x2822: // Remote Mixed Functional 29 bit
            if (addr_mode_rx != addr_mode_tx)
            {
                return RET_CANTP_ERR_PARAM;
            }

            if ((rxid > 0xff) || (txid > 0xff))
            {
                return RET_CANTP_ERR_PARAM;
                ;
            }

            canTpSessions[i].tx_id = 0x18cd0000 | (rxid << 8) | txid;
            canTpSessions[i].rx_id = 0x18cd0000 | (txid << 8) | rxid;
            canTpSessions[i].flags |= canMSG_EXT;
            mask = 0xfffcff00;
            func = 1;
            fixed = 1;
            break;

        case 0x0801: //        Mixed          11 bit
        case 0x0811: //        Mixed Physical 11 bit
        case 0x1801: // Local  Mixed          11 bit
        case 0x1811: // Local  Mixed Physical 11 bit
        case 0x2801: // Remote Mixed          11 bit
        case 0x2811: // Remote Mixed Physical 11 bit
            if (addr_mode_rx != addr_mode_tx)
            {
                return RET_CANTP_ERR_PARAM;
            }

            if ((rxid > 0x7ff) || (txid > 0x7ff))
            {
                return RET_CANTP_ERR_PARAM;
            }

            canTpSessions[i].rx_id = rxid;
            canTpSessions[i].tx_id = txid;
            break;

        case 0x0001: //                       11 bit
        case 0x0011: //              Physical 11 bit
        case 0x0101: //       Normal          11 bit
        case 0x0111: //       Normal Physical 11 bit
        case 0x1001: // Local                 11 bit
        case 0x1011: // Local        Physical 11 bit
        case 0x1101: // Local Normal          11 bit
        case 0x1111: // Local Normal Physical 11 bit
            if ((addr_mode_rx != addr_mode_tx) || (addr_mode_rx & 0xff))
            {
                return RET_CANTP_ERR_PARAM;
            }

            if ((rxid > 0x7ff) || (txid > 0x7ff))
            {
                return RET_CANTP_ERR_PARAM;
            }

            canTpSessions[i].rx_id = rxid;
            canTpSessions[i].tx_id = txid;
            break;

        case 0x0021: //              Functional 11 bit
        case 0x0121: //       Normal Functional 11 bit
        case 0x1021: // Local        Functional 11 bit
        case 0x1121: // Local Normal Functional 11 bit
            if ((addr_mode_rx != addr_mode_tx) || (addr_mode_rx & 0xff))
            {
                return RET_CANTP_ERR_PARAM;
            }

            if ((rxid > 0x7ff) || (txid > 0x7ff))
            {
                return RET_CANTP_ERR_PARAM;
            }

            canTpSessions[i].rx_id = rxid;
            canTpSessions[i].tx_id = txid;
            mask = 0xffffff00;
            func = 1;
            break;

        case 0x0002: //                       29 bit
        case 0x0012: //              Physical 29 bit
        case 0x0102: //       Normal          29 bit
        case 0x0112: //       Normal Physical 29 bit
        case 0x1002: // Local                 29 bit
        case 0x1012: // Local        Physical 29 bit
        case 0x1102: // Local Normal          29 bit
        case 0x1112: // Local Normal Physical 29 bit
            if ((addr_mode_rx != addr_mode_tx) || (addr_mode_rx & 0xff))
            {
                return RET_CANTP_ERR_PARAM;
            }
            if ((rxid > 0x1fffffff) || (txid > 0x1fffffff))
            {
                return RET_CANTP_ERR_PARAM;
            }
            canTpSessions[i].rx_id = rxid;
            canTpSessions[i].tx_id = txid;
            canTpSessions[i].flags |= canMSG_EXT;
            break;

        case 0x0022: //              Functional 29 bit
        case 0x0122: //       Normal Functional 29 bit
        case 0x1022: // Local        Functional 29 bit
        case 0x1122: // Local Normal Functional 29 bit
            if ((addr_mode_rx != addr_mode_tx) || (addr_mode_rx & 0xff))
            {
                return RET_CANTP_ERR_PARAM;
            }
            if ((rxid > 0x1fffffff) || (txid > 0x1fffffff))
            {
                return RET_CANTP_ERR_PARAM;
            }
            canTpSessions[i].rx_id = rxid;
            canTpSessions[i].tx_id = txid;
            canTpSessions[i].flags |= canMSG_EXT;
            mask = 0xffffff00;
            func = 1;
            break;

        default:
            return RET_CANTP_ERR_PARAM;
            break;
        }

        canTpSessions[i].code = canTpSessions[i].rx_id;
        canTpSessions[i].func = func;
        canTpSessions[i].mask = mask;
        canTpSessions[i].state = CANTP_STATE_IDLE;
        canTpSessions[i].rx_eid = addr_mode_rx & 0xff;
        canTpSessions[i].tx_eid = addr_mode_tx & 0xff;
        canTpSessions[i].fixed = fixed;

        if (addr_mode_rx & 0x0c0000)
        {
            canTpSessions[i].eid = 1;
        }
        else
        {
            canTpSessions[i].eid = 0;
        }

        handle = i;
        i++;

        return RET_CANTP_NO_ERR;
    }
    else
    {
        return RET_CANTP_ERR_PARAM;
    }
}

void canTpClose(int handle)
{
    if (handle >= MAX_CANTP_SESSIONS)
    {
        return;
    }

    timerCancel(canTpSessions[handle].transmit_timeout_timer);
    timerCancel(canTpSessions[handle].receive_timeout_timer);
    timerCancel(canTpSessions[handle].cf_delay_timer);

    canTpSessions[handle].state = CANTP_STATE_UNINITIALIZED;
}

int canTpAbort(int handle)
{
    if (handle >= MAX_CANTP_SESSIONS)
    {
        return RET_CANTP_ERR_PARAM;
    }

    if (canTpSessions[handle].state == CANTP_STATE_UNINITIALIZED)
    {
        return RET_CANTP_ERR_PARAM;
    }

    timerCancel(canTpSessions[handle].transmit_timeout_timer);
    timerCancel(canTpSessions[handle].receive_timeout_timer);
    timerCancel(canTpSessions[handle].cf_delay_timer);

    canTpSessions[handle].state = CANTP_STATE_IDLE;
    return RET_CANTP_NO_ERR;
}

int canTpTransmit(int handle, const byte data[])
{
    if (handle >= MAX_CANTP_SESSIONS)
    {
        return RET_CANTP_ERR_PARAM;
    }

    if ((!data.count) || (data.count > 0xFFF))
    {
        return RET_CANTP_ERR_PARAM;
    }

    if ((data.count >= (8 - canTpSessions[handle].eid)) &&
        canTpSessions[handle].func)
    {
        printf("ISO15765 error: Only SF messages are allowed in functional "
               "mode\n");
        return RET_CANTP_ERR_PARAM;
    }

    if (canTpSessions[handle].state == CANTP_STATE_RX_DONE)
    {
        canTpSessions[handle].state = CANTP_STATE_IDLE;
    }

    if (canTpSessions[handle].state != CANTP_STATE_IDLE)
    {
        return RET_CANTP_ERR_BUSY;
    }

    canTpSessions[handle].buffer = data;
    canTpSessions[handle].length = data.count;
    cantp_transmit_ff(handle);
}

int canTpSetAttr(int handle, int attr, int value)
{
    if (handle >= MAX_CANTP_SESSIONS)
    {
        return RET_CANTP_ERR_PARAM;
    }

    if (canTpSessions[handle].state == CANTP_STATE_UNINITIALIZED)
    {
        return RET_CANTP_ERR_PARAM;
    }

    if (value < 0)
    {
        return RET_CANTP_ERR_PARAM;
    }

    switch (attr)
    {
    case CANTP_ID:
        canTpSessions[handle].userid = value;
        break;
    case CANTP_ISO15765_BS:
        if (value > 0xff)
        {
            value = 0;
        }
        canTpSessions[handle].receive_bs = (byte)value;
        break;
    case CANTP_ISO15765_STMIN:
        if ((value > 0xff) && (value != 0xffff))
        {
            return RET_CANTP_ERR_PARAM;
        }
        canTpSessions[handle].cf_delay_rx = value;
        break;
    case CANTP_ISO15765_TX_TIMEOUT:
        canTpSessions[handle].transmit_timeout_timer.timeout = value;
        break;
    case CANTP_ISO15765_RX_TIMEOUT:
        canTpSessions[handle].receive_timeout_timer.timeout = value;
        break;
    case CANTP_ISO15765_WFTMAX:
        if (value > 0xff)
            value = 0xff;
        canTpSessions[handle].wft_max = (byte)value;
        break;
    case CANTP_ISO15765_MASK:
        canTpSessions[handle].mask = value;
        break;
    case CANTP_ISO15765_CODE:
        canTpSessions[handle].code = value;
        break;
    case CANTP_ISO15765_CHANNEL:
        // no effect so far
        canTpSessions[handle].channel = value;
        break;
    }

    return RET_CANTP_NO_ERR;
}

int canTpGetAttr(int handle, int attr)
{
    int value;
    if (handle >= MAX_CANTP_SESSIONS)
    {
        return RET_CANTP_ERR_PARAM;
    }

    if (canTpSessions[handle].state == CANTP_STATE_UNINITIALIZED)
    {
        return RET_CANTP_ERR_PARAM;
    }

    switch (attr)
    {
    case CANTP_ID:
        value = canTpSessions[handle].userid;
        break;
    case CANTP_ISO15765_BS:
        value = canTpSessions[handle].receive_bs;
        break;
    case CANTP_ISO15765_STMIN:
        value = canTpSessions[handle].cf_delay_rx;
        break;
    case CANTP_ISO15765_TX_TIMEOUT:
        value = canTpSessions[handle].transmit_timeout_timer.timeout;
        break;
    case CANTP_ISO15765_RX_TIMEOUT:
        value = canTpSessions[handle].receive_timeout_timer.timeout;
        break;
    case CANTP_ISO15765_WFTMAX:
        value = canTpSessions[handle].wft_max;
        break;
    case CANTP_ISO15765_MASK:
        value = canTpSessions[handle].mask;
        break;
    case CANTP_ISO15765_CODE:
        value = canTpSessions[handle].code;
        break;
    case CANTP_ISO15765_CHANNEL:
        value = canTpSessions[handle].channel;
        break;
    }
}

void cantp_transmit_ff(int handle)
{
    CanMessage msg;

    int is_ext_id;
    int var_length = canTpSessions[handle].length;
    int var_tx_id = canTpSessions[handle].tx_eid;
    if (canTpSessions[handle].eid > 0)
    {
        is_ext_id = 1;
    }
    else
    {
        is_ext_id = 0;
    }

    msg.data[0] = var_tx_id;

    if (canTpSessions[handle].length < 8 - is_ext_id)
    {
        /* All in one frame  */
        int i;
        msg.data[is_ext_id] = (CANTP_SF << 4) + var_length;
        msg.data[is_ext_id + 1, var_length] =
            canTpSessions[handle].buffer[0, var_length];
        msg.data[is_ext_id + var_length .. 7] = 0;
        canTpSessions[handle].pos = var_length;
    }
    else
    {
        if (canTpSessions[handle].fc_enabled)
        {
            msg.data[is_ext_id] =
                (CANTP_FF << 4) + ((canTpSessions[handle].length >> 8) & 0x0f);
            canTpSessions[handle].blocks_remaining = 0;
            timerStart(canTpSessions[handle].transmit_timeout_timer, 1);
        }
        else
        {
            msg.data[is_ext_id] = (CANTP_FF_NOFC << 4) +
                                  ((canTpSessions[handle].length >> 8) & 0x0f);
            canTpSessions[handle].blocks_remaining = 0xFFFF;
            timerStart(canTpSessions[handle].transmit_timeout_timer, 1);
        }
        msg.data[1 + is_ext_id] = canTpSessions[handle].length & 0xff;
        msg.data[2 + is_ext_id, 6 - is_ext_id] =
            canTpSessions[handle].buffer[0, 6 - is_ext_id];
        canTpSessions[handle].pos = 6 - is_ext_id;
        canTpSessions[handle].sn = 1;
        canTpSessions[handle].wft = 0;
    }

    canTpSessions[handle].state = CANTP_STATE_TX;
    msg.id = canTpSessions[handle].tx_id;
    msg.dlc = 8;
    msg.flags = canTpSessions[handle].flags;

    canWrite(canTpSessions[handle].channel, msg);

    if (canTpSessions[handle].fc_enabled == 0)
    {
        canTpSessions[handle].cf_delay_timer.timeout = 1;
    }

    timerStart(canTpSessions[handle].cf_delay_timer, 1);
}

void cantp_transmit_cf(int handle)
{
    CanMessage msg;
    int var_length = canTpSessions[handle].length - canTpSessions[handle].pos;
    int var_eid = canTpSessions[handle].eid;

    if (var_length <= 0)
        return;

    if (0 == canTpSessions[handle].blocks_remaining)
    {
        return;
    }

    if (var_length > 7 - var_eid)
    {
        var_length = 7 - var_eid;
    }

    msg.data[0] = canTpSessions[handle].tx_eid;

    if (canTpSessions[handle].fc_enabled)
    {
        msg.data[var_eid] = (CANTP_CF << 4) + canTpSessions[handle].sn;
        canTpSessions[handle].blocks_remaining--;
    }
    else
    {
        msg.data[var_eid] = (CANTP_CF_NOFC << 4) + canTpSessions[handle].sn;
    }

    canTpSessions[handle].sn = (canTpSessions[handle].sn + 1) & 0x0f;
    msg.data[1 + var_eid, var_length] =
        canTpSessions[handle].buffer[canTpSessions[handle].pos, var_length];
    msg.data[1 + var_eid + var_length - 1 .. 7] = 0;
    msg.id = canTpSessions[handle].tx_id;
    msg.dlc = 8;
    msg.flags = canTpSessions[handle].flags;
    canTpSessions[handle].pos += var_length;

    canWrite(canTpSessions[handle].channel, msg);
}

void cantp_transmit_fc(int handle)
{
    CanMessage msg;
    int var_eid = canTpSessions[handle].eid;
    msg.data[0] = canTpSessions[handle].tx_eid;
    msg.data[var_eid] = (CANTP_FC << 4);
    msg.data[var_eid + 1] = canTpSessions[handle].receive_bs;
    msg.data[var_eid + 2] = (byte)canTpSessions[handle].cf_delay_rx;
    msg.data[var_eid + 3 .. 7] = 0;

    canTpSessions[handle].blocks_remaining = canTpSessions[handle].receive_bs;
    msg.dlc = 8;
    msg.flags = canTpSessions[handle].flags;

    canWrite(canTpSessions[handle].channel, msg);
}

on Timer "Transmit_Timeout"
{
    if (canTpSessions[this.id].state != CANTP_STATE_TX)
    {
        return;
    }

    canTpSessions[this.id].state = CANTP_STATE_IDLE;
    canTpSessions[this.id].transmit_result =
        CANTP_N_TIMEOUT_Bs & CANTP_TX_ERR_MASK;

    txIndication(this.id);
}

on Timer "Receive_Timeout"
{
    if (canTpSessions[this.id].state != CANTP_STATE_RX)
    {
        return;
    }

    canTpSessions[this.id].state = CANTP_STATE_IDLE;
    canTpSessions[this.id].transmit_result =
        CANTP_N_TIMEOUT_Bs & CANTP_RX_ERR_MASK;

    rxIndication(this.id);
}

on Timer "CF_Delay"
{
    if (canTpSessions[this.id].state != CANTP_STATE_TX)
    {
        return;
    }

    timerCancel(canTpSessions[this.id].transmit_timeout_timer);
    cantp_transmit_cf(this.id);
    timerStart(canTpSessions[this.id].transmit_timeout_timer, 1);

    if (canTpSessions[this.id].pos < canTpSessions[this.id].length)
    {
        // Wait and do next transmit
        timerStart(this, 1);
    }

    if (canTpSessions[this.id].pos >= canTpSessions[this.id].length)
    {
        canTpSessions[this.id].state = CANTP_STATE_IDLE;
        canTpSessions[this.id].transmit_result = CANTP_N_OK;
        txIndication(this.id);
    }
}
