// NOTE! Do not touch any of these variables from external code
variables
{
    // Comment out this line to disable debug traces
    // const int LINPRIVATE_TRACE_ALL_CHANNELS = 0;
    // const int LINPRIVATE_TRACE_OPERATIONS = 0;
    // Comment out this line to only output debug traces
    // for channels opened by T scripts

    const int LINPRIVATE_CAN_ID_COMMAND = 0x7FF;
    const int LINPRIVATE_CAN_ID_RESPONSE = 0x7FE;
    const int LINPRIVATE_CAN_ID_STATUS = 0x7FD;
    const int LINPRIVATE_CAN_ID_TIMING = 0x7FC;
    const int LINPRIVATE_CAN_ID_TIMING0 = 0x7F8;
    const int LINPRIVATE_CAN_ID_TX_WAKEUP = 0x7FB;
    const int LINPRIVATE_CAN_ID_RX_WAKEUP = 0x7F9;
    const int LINPRIVATE_CAN_ID_TXE_WAKEUP = 0x7F4;
    const int LINPRIVATE_CAN_ID_SYNCHERROR = 0x7FA;
    const int LINPRIVATE_CAN_ID_TIMING_SYNCH = 0x7F7;
    const int LINPRIVATE_CAN_ID_TIMING_DATA0 = 0x7F6;
    const int LINPRIVATE_CAN_ID_TIMING_DATA1 = 0x7F5;
    const int LINPRIVATE_CAN_ID_TX_WAKEUP_HYBRID = 0x7F3;
    const int LINPRIVATE_CAN_ID_TERM = 0x7F2;
    const int LINPRIVATE_CAN_ID_PASSTHRU_TX = 0x7F1;
    const int LINPRIVATE_CAN_ID_PASSTHRU_RX = 0x7F0;

    const int LINPRIVATE_ID_DONGLE_RCV = 0x0040;
    const int LINPRIVATE_ID_DONGLE_TX = 0x0080;
    const int LINPRIVATE_ID_CSUM_ERROR = 0x0100;
    const int LINPRIVATE_ID_NO_DATA = 0x0200;
    const int LINPRIVATE_ID_STATUS_BITS =
        (LINPRIVATE_ID_DONGLE_RCV | LINPRIVATE_ID_DONGLE_TX |
         LINPRIVATE_ID_CSUM_ERROR | LINPRIVATE_ID_NO_DATA);

    const int LINPRIVATE_COMMAND_RESET = 0x00;
    const int LINPRIVATE_ENHANCED_CHECKSUM = LIN_FLAG_ENHANCED_CHECKSUM;
    const int LINPRIVATE_VARIABLE_DLC = LIN_FLAG_VARIABLE_DLC;
    const int LINPRIVATE_COMMAND_SETUP = 0x08;
    const int LINPRIVATE_COMMAND_START_BOOTLOADER = 0x11;
    const int LINPRIVATE_COMMAND_EEPROM_WRITE_OLD = 0x51;
    const int LINPRIVATE_COMMAND_GET_INFO = 0x60;
    const int LINPRIVATE_COMMAND_SETUP_ILLEGAL_MESSAGE = 0x07;
    const int LINPRIVATE_GET_INFO_TYPE_REFPOWER = 1;

    const int LINPRIVATE_EEPROM_PROG_PASSWD0 = 0x01;
    const int LINPRIVATE_EEPROM_PROG_PASSWD1 = 0x03;
    const int LINPRIVATE_EEPROM_LOC_OPERATION_MODE = 0x10;
    const int LINPRIVATE_EEPROM_LOC_BITTIME_LOW = 0x11;
    const int LINPRIVATE_EEPROM_LOC_BITTIME_HIGH = 0x12;

    const int LINPRIVATE_LIN_COMMAND_TIMEOUT = 1000;

    const int LINPRIVATE_CALL_SEQUENCE_DONE = 0;
    const int LINPRIVATE_CALL_SEQUENCE_CHECK_CABLE = 1;
    const int LINPRIVATE_CALL_SEQUENCE_SET_MODE = 2;
    const int LINPRIVATE_CALL_SEQUENCE_SET_BITRATE0 = 3;
    const int LINPRIVATE_CALL_SEQUENCE_SET_BITRATE1 = 4;
    const int LINPRIVATE_CALL_SEQUENCE_GO_LIN_ON = 5;
    const int LINPRIVATE_CALL_SEQUENCE_BOOT = 6;
    const int LINPRIVATE_CALL_SEQUENCE_GO_LIN_OFF = 7;
    const int LINPRIVATE_CALL_SEQUENCE_SETUP_ILLEGAL_MESSAGE = 8;
    const int LINPRIVATE_CALL_SEQUENCE_SETUP_LIN = 9;

    // NOTE! Increasing LINPRIVATE_MAX_CHANNEL_NUMBER also requires that:
    // Timer linPrivateTimerNoResponseX variable is/are added
    // timer function(s) is/are added
    // LinPrivateCommand is updated
    // LinPrivateCancelResponseTimer is updated
    const int LINPRIVATE_MAX_CHANNEL_NUMBER = 1;

    // Channel state variables
    int linPrivateCurrentFlags[LINPRIVATE_MAX_CHANNEL_NUMBER + 1] = {0, 0};
    int linPrivateCurrentMessageFlags[LINPRIVATE_MAX_CHANNEL_NUMBER + 1] = {0,
                                                                            0};
    int linPrivateCurrentBitrate[LINPRIVATE_MAX_CHANNEL_NUMBER + 1] = {
        LIN_DEFAULT_BITRATE, LIN_DEFAULT_BITRATE};
    int linPrivateCurrentMode[LINPRIVATE_MAX_CHANNEL_NUMBER + 1] = {
        LIN_MODE_MASTER, LIN_MODE_MASTER};
    int linPrivateCurrentUserState[LINPRIVATE_MAX_CHANNEL_NUMBER + 1] = {
        LIN_STATE_UNINITIALIZED, LIN_STATE_UNINITIALIZED};

    // Channel sequence variables
    int linPrivateLastCommand[LINPRIVATE_MAX_CHANNEL_NUMBER + 1];
    int linPrivateLastCommandCounter[LINPRIVATE_MAX_CHANNEL_NUMBER + 1];
    int linPrivateCurrentCallState[LINPRIVATE_MAX_CHANNEL_NUMBER + 1] = {
        LINPRIVATE_CALL_SEQUENCE_DONE, LINPRIVATE_CALL_SEQUENCE_DONE};
    int linPrivateCommandCounter[LINPRIVATE_MAX_CHANNEL_NUMBER + 1] = {0, 0};

    Timer linPrivateTimerNoResponse0;
    Timer linPrivateTimerNoResponse1;
}

#ifdef LINPRIVATE_TRACE_OPERATIONS
// *****************************************************************************
void PrivateTraceCanMessage(const char text[], const CanMessage msg,
                            int maskOffId)
{
    int locId = msg.id;

    if (maskOffId)
        locId = locId & 0x3F;

    printf("Channel %d - %s flags=0x%04x id=0x%03x dlc=%x data=0x%02x 0x%02x "
           "0x%02x 0x%02x 0x%02x 0x%02x 0x%02x 0x%02x",
           msg.channel, text, msg.flags, locId, msg.dlc, msg.data[0],
           msg.data[1], msg.data[2], msg.data[3], msg.data[4], msg.data[5],
           msg.data[6], msg.data[7]);
}

// *****************************************************************************
void PrivateTraceLinMessage(const char text[], const LinMessage msg,
                            int maskOffId)
{
    int locId = msg.msg.id;

    if (maskOffId)
        locId = locId & 0x3F;

    printf("Channel %d - %s flags=0x%04x id=0x%03x dlc=%x data=0x%02x 0x%02x "
           "0x%02x 0x%02x 0x%02x 0x%02x 0x%02x 0x%02x",
           msg.msg.channel, text, msg.msg.flags, locId, msg.msg.dlc,
           msg.msg.data[0], msg.msg.data[1], msg.msg.data[2], msg.msg.data[3],
           msg.msg.data[4], msg.msg.data[5], msg.msg.data[6], msg.msg.data[7]);
}

// *****************************************************************************
void PrivateTraceText(const char text[]) { printf(text); }

// *****************************************************************************
void PrivateTraceOneInt(const char format[], int i1) { printf(format, i1); }

// *****************************************************************************
void PrivateTraceTwoInts(const char format[], int i1, int i2)
{
    printf(format, i1, i2);
}
#else
void PrivateTraceCanMessage(const char text[], const CanMessage msg,
                            int maskOffId)
{
}
void PrivateTraceLinMessage(const char text[], const LinMessage msg,
                            int maskOffId)
{
}
void PrivateTraceText(const char text[]) {}
void PrivateTraceOneInt(const char format[], int i1) {}
void PrivateTraceTwoInts(const char format[], int i1, int i2) {}
#endif

// *****************************************************************************
int LinPrivateCommand(int channel, int command, int p1, int p2, int p3, int p4,
                      int p5, int p6, int responseExpected)
{
    linPrivateLastCommand[channel] = command;
    linPrivateLastCommandCounter[channel] = linPrivateCommandCounter[channel]++;

    CanMessage msg;

    msg.channel = channel;
    msg.id = LINPRIVATE_CAN_ID_COMMAND;
    msg.dlc = 8;
    msg.flags = 0;
    msg.data[0] = command;
    msg.data[1] = linPrivateLastCommandCounter[channel];
    msg.data[2] = p1;
    msg.data[3] = p2;
    msg.data[4] = p3;
    msg.data[5] = p4;
    msg.data[6] = p5;
    msg.data[7] = p6;

    PrivateTraceCanMessage("LinCommand: ", msg, 0);

    if (canWrite(channel, msg) < 0)
    {
        printf("Failed to send lin command %d", command);
        return LIN_RETURN_CODE_INTERNAL_FAILURE;
    }

    if (responseExpected)
    {
        linPrivateCurrentUserState[channel] = LIN_STATE_OPERATION_IN_PROGRESS;
        if (channel == 0)
        {
            linPrivateTimerNoResponse0.timeout = LINPRIVATE_LIN_COMMAND_TIMEOUT;
            timerStart(linPrivateTimerNoResponse0);
        }
        else
        {
            linPrivateTimerNoResponse1.timeout = LINPRIVATE_LIN_COMMAND_TIMEOUT;
            timerStart(linPrivateTimerNoResponse1);
        }

        return LIN_RETURN_CODE_RESULT_PENDING;
    }

    return LIN_RETURN_CODE_SUCCESS;
}

// *****************************************************************************
on Timer linPrivateTimerNoResponse0
{
    PrivateTraceText("Lin 0 command timeout");
    linPrivateCurrentUserState[0] =
        LIN_STATE_INTERNAL_FAILURE; // TODO How to really resolve?
}

// *****************************************************************************
on Timer linPrivateTimerNoResponse1
{
    PrivateTraceText("Lin 1 command timeout");
    linPrivateCurrentUserState[1] =
        LIN_STATE_INTERNAL_FAILURE; // TODO How to really resolve?
}

// *****************************************************************************
int LinPrivateCheckCable(int channel)
{
    return LinPrivateCommand(channel, LINPRIVATE_COMMAND_GET_INFO, 0,
                             LINPRIVATE_GET_INFO_TYPE_REFPOWER, 0, 0, 0, 0, 1);
}

// *****************************************************************************
int LinPrivateCanSetMode(int channel, int opMode)
{
    return LinPrivateCommand(channel, LINPRIVATE_COMMAND_EEPROM_WRITE_OLD,
                             LINPRIVATE_EEPROM_LOC_OPERATION_MODE, opMode,
                             LINPRIVATE_EEPROM_PROG_PASSWD0,
                             LINPRIVATE_EEPROM_PROG_PASSWD1, 0, 0, 1);
}

// *****************************************************************************
int LinPrivateSetBitrate(int channel, int bitrate, int callNumber)
{
    int bittime = (int)(4000000.0 / bitrate + 0.5);

    if (callNumber == 0)
    {
        return LinPrivateCommand(channel, LINPRIVATE_COMMAND_EEPROM_WRITE_OLD,
                                 LINPRIVATE_EEPROM_LOC_BITTIME_LOW,
                                 bittime & 0xff, LINPRIVATE_EEPROM_PROG_PASSWD0,
                                 LINPRIVATE_EEPROM_PROG_PASSWD1, 0, 0, 1);
    }
    else if (callNumber == 1)
    {
        return LinPrivateCommand(channel, LINPRIVATE_COMMAND_EEPROM_WRITE_OLD,
                                 LINPRIVATE_EEPROM_LOC_BITTIME_HIGH,
                                 (bittime >> 8) & 0xff,
                                 LINPRIVATE_EEPROM_PROG_PASSWD0,
                                 LINPRIVATE_EEPROM_PROG_PASSWD1, 0, 0, 1);
    }
    else
    {
        return LinPrivateCommand(channel, LINPRIVATE_COMMAND_SETUP, 0,
                                 linPrivateCurrentFlags[channel],
                                 bittime & 0xff, (bittime >> 8) & 0xff, 0, 0,
                                 1);
    }
}

// *****************************************************************************
void LinPrivateCancelResponseTimer(int channel)
{
    if (channel == 0)
    {
        timerCancel(linPrivateTimerNoResponse0);
    }
    else
    {
        timerCancel(linPrivateTimerNoResponse1);
    }
}

// *****************************************************************************
void LinPrivateOnResponse(const CanMessage msg)
{
    if (msg.dlc == 8 && msg.data[0] == linPrivateLastCommand[msg.channel] &&
        msg.data[1] == linPrivateLastCommandCounter[msg.channel])
    {
        if (msg.data[2] == 0)
        {
            int result;

            printf("Channel %d - LinCommand successful", msg.channel);

            switch (linPrivateCurrentCallState[msg.channel])
            {
            case LINPRIVATE_CALL_SEQUENCE_CHECK_CABLE:
                LinPrivateCancelResponseTimer(msg.channel);
                if ((msg.data[3] == 0) || (msg.data[4] == 0))
                {
                    printf("Channel %d - Warning! No power connected!",
                           msg.channel);
                }
                PrivateTraceTwoInts(
                    "Channel %d - Assign to LIN running in mode %d",
                    msg.channel, linPrivateCurrentMode[msg.channel]);
                result = LinPrivateCanSetMode(
                    msg.channel, linPrivateCurrentMode[msg.channel]);
                linPrivateCurrentCallState[msg.channel] =
                    LINPRIVATE_CALL_SEQUENCE_SET_MODE;
                PrintWithTranslatedReturnCode(msg.channel, "LinCommand ",
                                              result);
                break;

            case LINPRIVATE_CALL_SEQUENCE_SET_MODE:
                LinPrivateCancelResponseTimer(msg.channel);
                linPrivateCurrentCallState[msg.channel] =
                    LINPRIVATE_CALL_SEQUENCE_DONE;
                linPrivateCurrentUserState[msg.channel] = LIN_STATE_BUS_OFF;
                break;

            case LINPRIVATE_CALL_SEQUENCE_SET_BITRATE0:
                LinPrivateCancelResponseTimer(msg.channel);
                PrivateTraceTwoInts(
                    "Channel %d - Second call to setup bitrate %d", msg.channel,
                    linPrivateCurrentBitrate[msg.channel]);
                result = LinPrivateSetBitrate(
                    msg.channel, linPrivateCurrentBitrate[msg.channel], 1);
                linPrivateCurrentCallState[msg.channel] =
                    LINPRIVATE_CALL_SEQUENCE_SET_BITRATE1;
                PrintWithTranslatedReturnCode(msg.channel, "LinCommand ",
                                              result);
                break;

            case LINPRIVATE_CALL_SEQUENCE_SET_BITRATE1:
                LinPrivateCancelResponseTimer(msg.channel);
                PrivateTraceTwoInts(
                    "Channel %d - Third call to setup bitrate %d", msg.channel,
                    linPrivateCurrentBitrate[msg.channel]);
                result = LinPrivateSetBitrate(
                    msg.channel, linPrivateCurrentBitrate[msg.channel], 2);
                linPrivateCurrentCallState = LINPRIVATE_CALL_SEQUENCE_GO_LIN_ON;
                PrintWithTranslatedReturnCode(msg.channel, "LinCommand ",
                                              result);
                break;

            case LINPRIVATE_CALL_SEQUENCE_GO_LIN_ON:
                LinPrivateCancelResponseTimer(msg.channel);
                PrivateTraceOneInt("Channel %d - Resetting LIN", msg.channel);
                result = LinPrivateCommand(
                    msg.channel, LINPRIVATE_COMMAND_RESET, 0, 0, 0, 0, 0, 0, 0);
                PrintWithTranslatedReturnCode(msg.channel, "LinCommand ",
                                              result);

                PrivateTraceOneInt("Channel %d - LIN start bootloader",
                                   msg.channel);
                result = LinPrivateCommand(msg.channel,
                                           LINPRIVATE_COMMAND_START_BOOTLOADER,
                                           0, 0, 0, 0, 0, 0, 1);
                PrintWithTranslatedReturnCode(msg.channel, "LinCommand ",
                                              result);
                linPrivateCurrentCallState = LINPRIVATE_CALL_SEQUENCE_BOOT;
                break;

            case LINPRIVATE_CALL_SEQUENCE_BOOT:
                PrivateTraceOneInt("Channel %d - Unexpected frame received!",
                                   msg.channel);
                break;

            case LINPRIVATE_CALL_SEQUENCE_SETUP_ILLEGAL_MESSAGE:
                PrivateTraceOneInt(
                    "Channel %d - Corrupted message set up successfully",
                    msg.channel);
                break;

            case LINPRIVATE_CALL_SEQUENCE_SETUP_LIN:
                LinPrivateCancelResponseTimer(msg.channel);
                PrivateTraceOneInt("Channel %d - LIN set up successfully",
                                   msg.channel);
                break;

            default:
                PrivateTraceOneInt("Channel %d - Unexpected frame received!",
                                   msg.channel);
                break;
            }
        }
        else
        {
            printf(
                "Channel %d - Command 0x%02x, counter=%d returned status=0x%d",
                msg.channel, linPrivateLastCommand[msg.channel],
                linPrivateLastCommandCounter[msg.channel], msg.data[2]);
            // TODO How to resolve?
        }
    }
}

// *****************************************************************************
void LinPrivateOnStatus(const CanMessage msg)
{
    int result;

    switch (linPrivateCurrentCallState[msg.channel])
    {
    case LINPRIVATE_CALL_SEQUENCE_SET_MODE:
    case LINPRIVATE_CALL_SEQUENCE_SET_BITRATE0:
    case LINPRIVATE_CALL_SEQUENCE_SET_BITRATE1:
    case LINPRIVATE_CALL_SEQUENCE_GO_LIN_ON:
        PrivateTraceOneInt("Channel %d - Unexpected frame received!",
                           msg.channel);
        break;

    case LINPRIVATE_CALL_SEQUENCE_BOOT:
        LinPrivateCancelResponseTimer(msg.channel);
        if ((msg.dlc == 8) && (msg.data[0] == 'L') && (msg.data[1] == 'I') &&
            (msg.data[2] == 'N') && (msg.data[3] == ' ') &&
            (msg.data[4] == 'B') && (msg.data[5] == 'O') &&
            (msg.data[6] == 'O') && (msg.data[7] == 'T'))
        {
            printf("Channel %d - LinCommand successful", msg.channel);
            linPrivateCurrentUserState[msg.channel] = LIN_STATE_BUS_ON;
        }
        else
        {
            linPrivateCurrentUserState[msg.channel] =
                LIN_STATE_INTERNAL_FAILURE;
        }
        break;

    case LINPRIVATE_CALL_SEQUENCE_GO_LIN_OFF:
        linPrivateCurrentUserState[msg.channel] = LIN_STATE_BUS_OFF;
        break;

    default:
        PrivateTraceOneInt("Channel %d - Unexpected frame received!",
                           msg.channel);
        break;
    }
}

// *****************************************************************************
void LinPrivateCopyCanToLinMessage(LinMessage linMsg, const CanMessage canMsg)
{
    linMsg.msg.channel = canMsg.channel;
    linMsg.msg.id = canMsg.id & ~LINPRIVATE_ID_STATUS_BITS;
    linMsg.msg.flags = linPrivateCurrentMessageFlags[canMsg.channel];
    linMsg.msg.dlc = canMsg.dlc;
    linMsg.msg.data[0] = canMsg.data[0];
    linMsg.msg.data[1] = canMsg.data[1];
    linMsg.msg.data[2] = canMsg.data[2];
    linMsg.msg.data[3] = canMsg.data[3];
    linMsg.msg.data[4] = canMsg.data[4];
    linMsg.msg.data[5] = canMsg.data[5];
    linMsg.msg.data[6] = canMsg.data[6];
    linMsg.msg.data[7] = canMsg.data[7];
}

// *****************************************************************************
void LinPrivateExtractCanFromLinMessage(CanMessage canMsg,
                                        const LinMessage linMsg)
{
    canMsg.channel = linMsg.msg.channel;
    canMsg.id = linMsg.msg.id & 0x3f; // Remove the parity bits.
    canMsg.flags = 0; // E.g. canMsgStd but symbol doesn't exist in T
    canMsg.dlc = linMsg.msg.dlc;
    canMsg.data[0] = linMsg.msg.data[0];
    canMsg.data[1] = linMsg.msg.data[1];
    canMsg.data[2] = linMsg.msg.data[2];
    canMsg.data[3] = linMsg.msg.data[3];
    canMsg.data[4] = linMsg.msg.data[4];
    canMsg.data[5] = linMsg.msg.data[5];
    canMsg.data[6] = linMsg.msg.data[6];
    canMsg.data[7] = linMsg.msg.data[7];
}

// *****************************************************************************
void LinPrivateOnCanMessageDefaultProcessing(const CanMessage msg)
{
    LinMessage locMsg;
    LinPrivateCopyCanToLinMessage(locMsg, msg);

    int canIdM = msg.id & ~LINPRIVATE_ID_STATUS_BITS;
    if (canIdM <= 63)
    {
        if (msg.id & LINPRIVATE_ID_NO_DATA)
        {
            linPrivateCurrentMessageFlags[msg.channel] |= LIN_MSG_FLAG_NODATA;
            PrivateTraceOneInt("Channel %d - No data", msg.channel);
        }

        if (msg.id & LINPRIVATE_ID_CSUM_ERROR)
        {
            linPrivateCurrentMessageFlags[msg.channel] |=
                (LIN_MSG_FLAG_CSUM_ERROR | LIN_MSG_FLAG_PARITY_ERROR);
            PrivateTraceOneInt("Channel %d - Checksum error", msg.channel);
        }

        if (msg.id & LINPRIVATE_ID_DONGLE_RCV)
        {
            linPrivateCurrentMessageFlags[msg.channel] |= LIN_MSG_FLAG_RX;
            if (linPrivateCurrentMode[msg.channel] == LIN_MODE_MASTER)
            {
                linRequestCompleted(locMsg);
            }
            else
            {
                linMessageReceived(locMsg);
            }
        }

        if (msg.id & LINPRIVATE_ID_DONGLE_TX)
        {
            linPrivateCurrentMessageFlags[msg.channel] |= LIN_MSG_FLAG_TX;
            linMessageTransmitted(locMsg);
        }
    }
}

// *****************************************************************************
void LinPrivateOnCanMessage(const CanMessage msg)
{
#ifndef LINPRIVATE_TRACE_ALL_CHANNELS
    if ((linPrivateCurrentUserState[msg.channel] != LIN_STATE_UNINITIALIZED) &&
        (linPrivateCurrentUserState[msg.channel] != LIN_STATE_INTERNAL_FAILURE))
    // NOTE! The absence of curly braces is intended
#endif
        PrivateTraceCanMessage("Got CAN message: ", msg, 0);

    if ((linPrivateCurrentUserState[msg.channel] == LIN_STATE_UNINITIALIZED) ||
        (linPrivateCurrentUserState[msg.channel] == LIN_STATE_INTERNAL_FAILURE))
    {
        return; // Ignore the frame
    }

    if ((msg.flags &
         (canMSG_TXACK | canMSG_ERROR_FRAME | canMSG_EXT | canMSG_RTR)) == 0)
    {
        linPrivateCurrentMessageFlags[msg.channel] = 0;
        switch (msg.id)
        {
        case LINPRIVATE_CAN_ID_TIMING0:
        case LINPRIVATE_CAN_ID_TIMING:
        case LINPRIVATE_CAN_ID_TIMING_SYNCH:
        case LINPRIVATE_CAN_ID_TIMING_DATA0:
        case LINPRIVATE_CAN_ID_TIMING_DATA1:
        case LINPRIVATE_CAN_ID_TX_WAKEUP:
            break;

        case LINPRIVATE_CAN_ID_SYNCHERROR:
            linPrivateCurrentMessageFlags[msg.channel] |=
                LIN_MSG_FLAG_SYNCH_ERROR;
            break;

        case LINPRIVATE_CAN_ID_RX_WAKEUP:
            linPrivateCurrentMessageFlags[msg.channel] |=
                (LIN_MSG_FLAG_WAKEUP_FRAME | LIN_MSG_FLAG_RX);
            linWakeupReceived(msg.channel);
            break;

        case LINPRIVATE_CAN_ID_TXE_WAKEUP:
            linPrivateCurrentMessageFlags[msg.channel] |=
                (LIN_MSG_FLAG_WAKEUP_FRAME | LIN_MSG_FLAG_TX);
            break;

        case LINPRIVATE_CAN_ID_COMMAND:
            break;

        case LINPRIVATE_CAN_ID_RESPONSE:
            LinPrivateOnResponse(msg);
            break;

        case LINPRIVATE_CAN_ID_STATUS:
            LinPrivateOnStatus(msg);
            break;

        case LINPRIVATE_CAN_ID_TERM:
        case LINPRIVATE_CAN_ID_PASSTHRU_RX:
        case LINPRIVATE_CAN_ID_PASSTHRU_TX:
            break;

        default:
            if (msg.id & 0xC00)
            {
                PrivateTraceTwoInts(
                    "Channel %d - Got a non LIN message (id=0x%x)", msg.channel,
                    msg.id);
            }
            else
            {
                LinPrivateOnCanMessageDefaultProcessing(msg);
            }
        }
    }
    else
    {
        PrivateTraceOneInt("Channel %d - Message ignored", msg.channel);
    }
}

// *****************************************************************************
on CanMessage<*> * { LinPrivateOnCanMessage(this); }

// *****************************************************************************
int linOpenChannel(int channel, int mode)
{
    if ((channel < 0) || (channel > LINPRIVATE_MAX_CHANNEL_NUMBER))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    if (linPrivateCurrentUserState[channel] != LIN_STATE_UNINITIALIZED)
        return LIN_RETURN_CODE_INVALID_CALL_SEQUENCE;
    if ((mode < LIN_MODE_MASTER) || (mode > LIN_MODE_SLAVE))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    // TODO Support passive mode?

    if (linPrivateCurrentFlags[channel] &
        (LINPRIVATE_ENHANCED_CHECKSUM | LINPRIVATE_VARIABLE_DLC))
    {
        printf("Opening channel %d with bitrate = %d, enhanced checksum & "
               "variable dlc");
    }
    else if (linPrivateCurrentFlags[channel] & LINPRIVATE_ENHANCED_CHECKSUM)
    {
        printf("Opening channel %d with bitrate = %d & enhanced checksum");
    }
    else if (linPrivateCurrentFlags[channel] & LINPRIVATE_VARIABLE_DLC)
    {
        printf("Opening channel %d with bitrate = %d & variable dlc");
    }

    int result;

    PrivateTraceOneInt("Channel %d - Switched to bus off", channel);
    result = canBusOff(channel);
    PrintWithTranslatedReturnCode(channel, "canBusOff call ", result);

    if (result != LIN_RETURN_CODE_SUCCESS)
    {
        linPrivateCurrentUserState[channel] = LIN_STATE_INTERNAL_FAILURE;
        return LIN_RETURN_CODE_INTERNAL_FAILURE;
    }

    PrivateTraceOneInt("Channel %d - Switched on output", channel);
    result = canSetBusOutputControl(channel, canDRIVER_NORMAL);
    PrintWithTranslatedReturnCode(channel, "canSetBusOutputControl call ",
                                  result);

    if (result != LIN_RETURN_CODE_SUCCESS)
    {
        linPrivateCurrentUserState[channel] = LIN_STATE_INTERNAL_FAILURE;
        return LIN_RETURN_CODE_INTERNAL_FAILURE;
    }

    PrivateTraceOneInt("Channel %d - Assign bus to be LIN", channel);
    result = canSetCommunicationMode(channel, canMODE_LIN);
    PrintWithTranslatedReturnCode(channel, "canSetCommunicationMode call ",
                                  result);

    if (result != LIN_RETURN_CODE_SUCCESS)
    {
        linPrivateCurrentUserState[channel] = LIN_STATE_INTERNAL_FAILURE;
        return LIN_RETURN_CODE_INTERNAL_FAILURE;
    }

    PrivateTraceOneInt("Channel %d - Switched to bus on", channel);
    result = canBusOn(channel);
    PrintWithTranslatedReturnCode(channel, "canBusOn call ", result);

    if (result != LIN_RETURN_CODE_SUCCESS)
    {
        linPrivateCurrentUserState[channel] = LIN_STATE_INTERNAL_FAILURE;
        return LIN_RETURN_CODE_INTERNAL_FAILURE;
    }

    // Now in LIN mode

    PrivateTraceOneInt("Channel %d - Checking LIN cable", channel);
    result = LinPrivateCheckCable(channel);
    PrintWithTranslatedReturnCode(channel, "LinCommand ", result);

    if (result != LIN_RETURN_CODE_RESULT_PENDING)
    {
        linPrivateCurrentUserState[channel] = LIN_STATE_INTERNAL_FAILURE;
        return LIN_RETURN_CODE_INTERNAL_FAILURE;
    }

    linPrivateCurrentMode[channel] = mode;
    linPrivateCurrentUserState[channel] = LIN_STATE_OPERATION_IN_PROGRESS;
    linPrivateCurrentCallState[channel] = LINPRIVATE_CALL_SEQUENCE_CHECK_CABLE;

    return LIN_RETURN_CODE_RESULT_PENDING;
}

// *****************************************************************************
int linCloseChannel(int channel)
{
    if ((channel < 0) || (channel > LINPRIVATE_MAX_CHANNEL_NUMBER))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    if (!((linPrivateCurrentUserState[channel] == LIN_STATE_BUS_ON) ||
          (linPrivateCurrentUserState[channel] == LIN_STATE_BUS_OFF)))
        return LIN_RETURN_CODE_INVALID_CALL_SEQUENCE;

    int result;

    PrivateTraceOneInt("Channel %d - Switched to bus off", channel);
    result = canBusOff(channel);
    PrintWithTranslatedReturnCode(channel, "canBusOff call ", result);

    if (result != LIN_RETURN_CODE_SUCCESS)
    {
        linPrivateCurrentUserState[channel] = LIN_STATE_INTERNAL_FAILURE;
        return LIN_RETURN_CODE_INTERNAL_FAILURE;
    }

    PrivateTraceOneInt("Channel %d - Assign bus to be CAN", channel);
    result = canSetCommunicationMode(channel, canMODE_CAN);
    PrintWithTranslatedReturnCode(channel, "canSetCommunicationMode call ",
                                  result);

    if (result != LIN_RETURN_CODE_SUCCESS)
    {
        linPrivateCurrentUserState[channel] = LIN_STATE_INTERNAL_FAILURE;
        return LIN_RETURN_CODE_INTERNAL_FAILURE;
    }

    linPrivateCurrentUserState[channel] = LIN_STATE_UNINITIALIZED;

    return LIN_RETURN_CODE_SUCCESS;
}

// *****************************************************************************
int linSetBitrate(int channel, int bitrate)
{
    if ((channel < 0) || (channel > LINPRIVATE_MAX_CHANNEL_NUMBER))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    if ((bitrate < LIN_MIN_BITRATE) || (bitrate > LIN_MAX_BITRATE))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    if (linPrivateCurrentUserState[channel] != LIN_STATE_BUS_OFF)
        return LIN_RETURN_CODE_INVALID_CALL_SEQUENCE;

    linPrivateCurrentBitrate[channel] = bitrate;

    return LIN_RETURN_CODE_SUCCESS;
}

// *****************************************************************************
int linSetupLIN(int channel, int flags, int bitrate)
{
    if ((channel < 0) || (channel > LINPRIVATE_MAX_CHANNEL_NUMBER))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    if ((bitrate < LIN_MIN_BITRATE) || (bitrate > LIN_MAX_BITRATE))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    if (linPrivateCurrentUserState[channel] != LIN_STATE_BUS_OFF)
        return LIN_RETURN_CODE_INVALID_CALL_SEQUENCE;

    linPrivateCurrentFlags[channel] =
        flags & (LINPRIVATE_ENHANCED_CHECKSUM | LINPRIVATE_VARIABLE_DLC);
    linPrivateCurrentBitrate[channel] = bitrate;
    linPrivateCurrentCallState[channel] = LINPRIVATE_CALL_SEQUENCE_SETUP_LIN;
    return LinPrivateSetBitrate(channel, bitrate, 2);
}

// *****************************************************************************
int linLibraryState(int channel)
{
    if ((channel < 0) || (channel > LINPRIVATE_MAX_CHANNEL_NUMBER))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;

    return linPrivateCurrentUserState[channel];
}

// *****************************************************************************
int linBusOff(int channel)
{
    if ((channel < 0) || (channel > LINPRIVATE_MAX_CHANNEL_NUMBER))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    if (linPrivateCurrentUserState[channel] == LIN_STATE_BUS_OFF)
        return LIN_RETURN_CODE_SUCCESS;
    if (linPrivateCurrentUserState[channel] != LIN_STATE_BUS_ON)
        return LIN_RETURN_CODE_NOT_BUS_ON;

    PrivateTraceOneInt("Channel %d - Resetting LIN", channel);
    int result = LinPrivateCommand(channel, LINPRIVATE_COMMAND_RESET, 0, 0, 0,
                                   0, 0, 0, 0);
    PrivateTraceTwoInts("Channel %d - Result code=%d", channel, result);

    // We don't wait for anything but set call state just in case. If we get a
    // response the state will be set to LIN_STATE_BUS_OFF again.
    linPrivateCurrentCallState[channel] = LINPRIVATE_CALL_SEQUENCE_GO_LIN_OFF;
    linPrivateCurrentUserState[channel] = LIN_STATE_BUS_OFF;

    return result;
}

// *****************************************************************************
int linBusOn(int channel)
{
    if ((channel < 0) || (channel > LINPRIVATE_MAX_CHANNEL_NUMBER))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    if (linPrivateCurrentUserState[channel] == LIN_STATE_BUS_ON)
        return LIN_RETURN_CODE_SUCCESS;

    PrivateTraceTwoInts("Channel %d - First call to setup bitrate %d", channel,
                        linPrivateCurrentBitrate[channel]);
    int result =
        LinPrivateSetBitrate(channel, linPrivateCurrentBitrate[channel], 0);
    linPrivateCurrentCallState[channel] = LINPRIVATE_CALL_SEQUENCE_SET_BITRATE0;
    PrivateTraceTwoInts("Channel %d - Result code=%d", channel, result);

    return result;
}

// *****************************************************************************
int linSetupIllegalMessage(int channel, int id, int cFlags, int delay)
{
    if ((channel < 0) || (channel > LINPRIVATE_MAX_CHANNEL_NUMBER))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    if (linPrivateCurrentUserState[channel] != LIN_STATE_BUS_ON)
        return LIN_RETURN_CODE_NOT_BUS_ON;
    if (linPrivateCurrentMode[channel] != LIN_MODE_SLAVE)
        return LIN_RETURN_CODE_ONLY_SLAVE;

    PrivateTraceOneInt("Channel %d - Call to SetupIllegalMessage", channel);
    linPrivateCurrentCallState[channel] =
        LINPRIVATE_CALL_SEQUENCE_SETUP_ILLEGAL_MESSAGE;
    int result =
        LinPrivateCommand(channel, LINPRIVATE_COMMAND_SETUP_ILLEGAL_MESSAGE, 0,
                          id, cFlags, delay, 0, 0, 0);
    PrivateTraceTwoInts("Channel %d - Result code=%d", channel, result);

    return result;
}

// *****************************************************************************
int linWriteMessage(const LinMessage msg)
{
    if ((msg.msg.channel < 0) ||
        (msg.msg.channel > LINPRIVATE_MAX_CHANNEL_NUMBER))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    if (linPrivateCurrentUserState[msg.msg.channel] != LIN_STATE_BUS_ON)
        return LIN_RETURN_CODE_NOT_BUS_ON;
    if (linPrivateCurrentMode[msg.msg.channel] != LIN_MODE_MASTER)
        return LIN_RETURN_CODE_ONLY_MASTER;
    if (msg.msg.dlc > 8)
        return LIN_RETURN_CODE_INVALID_PARAMETERS;

    CanMessage locMsg;
    LinPrivateExtractCanFromLinMessage(locMsg, msg);

    if (canWrite(locMsg.channel, locMsg) < 0)
    {
        PrivateTraceOneInt("Channel %d - Failed to send lin frame",
                           locMsg.channel);
        return LIN_RETURN_CODE_INTERNAL_FAILURE;
    }

    PrivateTraceLinMessage("LIN Tx message: ", msg, 0);

    return LIN_RETURN_CODE_SUCCESS;
}

// *****************************************************************************
int linRequestMessage(const LinMessage msg)
{
    if ((msg.msg.channel < 0) ||
        (msg.msg.channel > LINPRIVATE_MAX_CHANNEL_NUMBER))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    if (linPrivateCurrentUserState[msg.msg.channel] != LIN_STATE_BUS_ON)
        return LIN_RETURN_CODE_NOT_BUS_ON;
    if (linPrivateCurrentMode[msg.msg.channel] != LIN_MODE_MASTER)
        return LIN_RETURN_CODE_ONLY_MASTER;
    if (msg.msg.dlc > 8)
        return LIN_RETURN_CODE_INVALID_PARAMETERS;

    CanMessage locMsg;
    LinPrivateExtractCanFromLinMessage(locMsg, msg);

    locMsg.flags = canMSG_RTR;

    if (canWrite(locMsg.channel, locMsg) < 0)
    {
        PrivateTraceOneInt("Channel %d - Failed to send lin request frame",
                           locMsg.channel);
        return LIN_RETURN_CODE_INTERNAL_FAILURE;
    }

    PrivateTraceCanMessage("LIN Rx message: ", locMsg, 0);

    return LIN_RETURN_CODE_SUCCESS;
}

// *****************************************************************************
int linUpdateMessage(const LinMessage msg)
{
    if ((msg.msg.channel < 0) ||
        (msg.msg.channel > LINPRIVATE_MAX_CHANNEL_NUMBER))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    if (linPrivateCurrentUserState[msg.msg.channel] != LIN_STATE_BUS_ON)
        return LIN_RETURN_CODE_NOT_BUS_ON;
    if (linPrivateCurrentMode[msg.msg.channel] != LIN_MODE_SLAVE)
        return LIN_RETURN_CODE_ONLY_SLAVE;
    if (msg.msg.dlc > 8)
        return LIN_RETURN_CODE_INVALID_PARAMETERS;

    CanMessage locMsg;
    LinPrivateExtractCanFromLinMessage(locMsg, msg);

    if (canWrite(locMsg.channel, locMsg) < 0)
    {
        PrivateTraceOneInt("Channel %d - Failed to send lin frame",
                           locMsg.channel);
        return LIN_RETURN_CODE_INTERNAL_FAILURE;
    }

    PrivateTraceLinMessage("LIN message updated: ", msg, 0);

    return LIN_RETURN_CODE_SUCCESS;
}

// *****************************************************************************
int linClearMessage(int channel, int id)
{
    if ((channel < 0) || (channel > LINPRIVATE_MAX_CHANNEL_NUMBER))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    if (linPrivateCurrentUserState[channel] != LIN_STATE_BUS_ON)
        return LIN_RETURN_CODE_NOT_BUS_ON;
    if (linPrivateCurrentMode[channel] != LIN_MODE_SLAVE)
        return LIN_RETURN_CODE_ONLY_SLAVE;

    CanMessage locMsg;

    locMsg.id = id & 0x3f; // Remove the parity bits
    locMsg.flags = 0;      // E.g. canMsgStd but symbol doesn't exist in T
    locMsg.dlc = 0;
    locMsg.data[0] = 0;
    locMsg.data[1] = 0;
    locMsg.data[2] = 0;
    locMsg.data[3] = 0;
    locMsg.data[4] = 0;
    locMsg.data[5] = 0;
    locMsg.data[6] = 0;
    locMsg.data[7] = 0;

    if (canWrite(channel, locMsg) < 0)
    {
        PrivateTraceOneInt("Channel %d - Failed to clear lin frame", channel);
        return LIN_RETURN_CODE_INTERNAL_FAILURE;
    }

    PrivateTraceCanMessage("LIN message cleared: ", locMsg, 0);

    return LIN_RETURN_CODE_SUCCESS;
}

// *****************************************************************************
int linWriteWakeup(int channel, int count, int interval)
{
    if ((channel < 0) || (channel > LINPRIVATE_MAX_CHANNEL_NUMBER))
        return LIN_RETURN_CODE_INVALID_PARAMETERS;
    if (linPrivateCurrentUserState[channel] != LIN_STATE_BUS_ON)
        return LIN_RETURN_CODE_NOT_BUS_ON;
    if (linPrivateCurrentMode[channel] != LIN_MODE_SLAVE)
        return LIN_RETURN_CODE_ONLY_SLAVE;
    // TODO The dongle supports to send wakeup even in master mode.
    //      Wakeup is only received when in master mode though.
    //      OK?

    CanMessage locMsg;

    locMsg.id = LINPRIVATE_CAN_ID_TX_WAKEUP_HYBRID;
    locMsg.flags = 0; // E.g. canMsgStd but symbol doesn't exist in T

    if (count)
    {
        if (count > 255 || interval > 65535)
            return LIN_RETURN_CODE_INVALID_PARAMETERS;

        locMsg.dlc = 4;
        locMsg.data[0] = count;
        locMsg.data[1] = 5; // Default break length in bittime
        locMsg.data[2] = 0xFF & interval;
        locMsg.data[3] = 0xFF & (interval >> 8);
    }
    else
    {
        locMsg.dlc = 0;
    }

    if (canWrite(channel, locMsg) < 0)
    {
        PrivateTraceOneInt("Channel &d - Failed to send lin frame", channel);
        return LIN_RETURN_CODE_INTERNAL_FAILURE;
    }

    PrivateTraceCanMessage("Sending LIN wakeup : ", locMsg, 0);

    return LIN_RETURN_CODE_SUCCESS;
}

// *****************************************************************************
void PrintWithTranslatedReturnCode(int channel, const char text[],
                                   int resultCode)
{
    switch (resultCode)
    {
    case LIN_RETURN_CODE_SUCCESS:
        printf("Channel %d - %s(Result code %d = success)", channel, text,
               resultCode);
        break;
    case LIN_RETURN_CODE_RESULT_PENDING:
        printf("Channel %d - %s(Result code %d = result pending)", channel,
               text, resultCode);
        break;
    case LIN_RETURN_CODE_INVALID_PARAMETERS:
        printf("Channel %d - %s(Result code %d = invalid parameters)", channel,
               text, resultCode);
        break;
    case LIN_RETURN_CODE_INVALID_CALL_SEQUENCE:
        printf("Channel %d - %s(Result code %d = invalid call sequence)",
               channel, text, resultCode);
        break;
    case LIN_RETURN_CODE_NOT_BUS_ON:
        printf("Channel %d - %s(Result code %d = is bus off)", channel, text,
               resultCode);
        break;
    case LIN_RETURN_CODE_ONLY_MASTER:
        printf("Channel %d - %s(Result code %d = only master)", channel, text,
               resultCode);
        break;
    case LIN_RETURN_CODE_ONLY_SLAVE:
        printf("Channel %d - %s(Result code %d = only slave)", channel, text,
               resultCode);
        break;
    case LIN_RETURN_CODE_INTERNAL_FAILURE:
        printf("Channel %d - %s(Result code %d = internal failure)", channel,
               text, resultCode);
        break;
    default:
        printf("Channel %d - %s(Result code %d = unknown return code)", channel,
               text, resultCode);
        break;
    }
}
