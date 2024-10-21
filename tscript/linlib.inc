variables
{
    const int LIN_STATE_UNINITIALIZED = 0;
    const int LIN_STATE_OPERATION_IN_PROGRESS = 1;
    const int LIN_STATE_BUS_OFF = 2;
    const int LIN_STATE_BUS_ON = 3;
    const int LIN_STATE_INTERNAL_FAILURE = 4;

    const int LIN_RETURN_CODE_SUCCESS = 0;
    const int LIN_RETURN_CODE_RESULT_PENDING = 1;
    const int LIN_RETURN_CODE_INVALID_PARAMETERS = 2;
    const int LIN_RETURN_CODE_INVALID_CALL_SEQUENCE = 3;
    const int LIN_RETURN_CODE_NOT_BUS_ON = 4;
    const int LIN_RETURN_CODE_ONLY_MASTER = 5;
    const int LIN_RETURN_CODE_ONLY_SLAVE = 6;
    const int LIN_RETURN_CODE_INTERNAL_FAILURE = 7;

    // const int LIN_MODE_PASSIVE = 0; Not implemented yet
    const int LIN_MODE_MASTER = 1;
    const int LIN_MODE_SLAVE = 2;

    const int LIN_FLAG_ENHANCED_CHECKSUM = 0x01;
    const int LIN_FLAG_VARIABLE_DLC = 0x02;

    const int LIN_MIN_BITRATE = 1000;
    const int LIN_DEFAULT_BITRATE = 10000;
    const int LIN_MAX_BITRATE = 20000;

    const int LIN_MSG_FLAG_TX = 0x01;
    const int LIN_MSG_FLAG_RX = 0x02;
    const int LIN_MSG_FLAG_WAKEUP_FRAME = 0x04;
    const int LIN_MSG_FLAG_NODATA = 0x08;
    const int LIN_MSG_FLAG_CSUM_ERROR = 0x10;
    const int LIN_MSG_FLAG_PARITY_ERROR = 0x20;
    const int LIN_MSG_FLAG_SYNCH_ERROR = 0x40;
    const int LIN_MSG_FLAG_BIT_ERROR = 0x80;

    const int LIN_MSG_DISTURB_CSUM = 1;
    const int LIN_MSG_DISTURB_PARITY = 2;

    typedef struct
    {
        // NOTE! All these variable should be treated as if they were unsigned
        int timestamp;
        int synchBreakLength;
        int frameLength;
        int bitrate;
        byte checkSum;
        byte idPar;
        int synchEdgeTime[4];
        int byteTime[8];
    } LinMsgInfo;

    typedef struct
    {
        byte channel;
        byte flags;
        byte dlc;
        int id;
        byte data[8];
    } LinMsg;

    typedef struct
    {
        LinMsg msg;
        LinMsgInfo info; // NOTE! Not supported yet. The content is undefined
    } LinMessage;
}

// *****************************************************************************
// Opens a channel for lin communication. See also linLibraryState &
// linSetBitrate If channel bitrate hasn't been set before the call
// LIN_DEFAULT_BITRATE is used
//
// channel - The channel to open
// mode    - Mode of operation [LIN_MODE_MASTER | LIN_MODE_SLAVE]
//
// Returns:
// LIN_RETURN_CODE_RESULT_PENDING on successful operation.
// Any error code on failure
//
// Before using the opened channel linLibraryState has to return
// LIN_STATE_BUS_OFF
//
int linOpenChannel(int channel, int mode);

// *****************************************************************************
// Closes an open channel
//
// channel - The channel to close
//
int linCloseChannel(int channel);

// *****************************************************************************
// Sets the channel bitrate. Call will be rejected unless linLibraryState
// signals LIN_STATE_BUS_OFF
//
// channel - The channel to open [0 | 1]
// bitrate - Lin bitrate [LIN_MIN_BITRATE <= bitrate <= LIN_MAX_BITRATE]
//
int linSetBitrate(int channel, int bitrate);

// *****************************************************************************
// Sets the channel bitrate. Call will be rejected unless linLibraryState
// signals LIN_STATE_BUS_OFF
//
// channel - The channel to open [0 | 1]
// flags   - One or more of the flags LIN_FLAG_VARIABLE_DLC &
//           LIN_FLAG_ENHANCED_CHECKSUM or 0
// bitrate - Lin bitrate [LIN_MIN_BITRATE <= bitrate <= LIN_MAX_BITRATE]
//
int linSetupLIN(int channel, int flags, int bitrate);

// *****************************************************************************
// Checks the library state.
//
// Generally speaking, no operations except linOpenChannel will be granted
// unless the library signals LIN_STATE_BUS_ON. One exception exist though and
// that is linSetBitrate (see description of linSetBitrate)
//
int linLibraryState(int channel);

// *****************************************************************************
// Switches the bus to off state.
//
// No operations except linBusOn & linLibraryState will be granted after
// linBusOff has been called.
//
// NOTE! Going bus off voids all slave messages. I.e. when going bus on again
//       they have to be initialized again using linUpdateMessage
//
int linBusOff(int channel);

// *****************************************************************************
// Switches the bus to on state.
//
// No operations will be granted before linLibraryState signals
// LIN_STATE_BUS_ON.
//
int linBusOn(int channel);

// *****************************************************************************
// Create corrupted LIN messages.
//
// No operations will be granted before linLibraryState signals
// LIN_STATE_BUS_ON.
//
int linSetupIllegalMessage(int channel, int id, int cFlags);

// *****************************************************************************
// Sends data to a slave. A complete operation will be signalled via the
// linMessageTransmitted callback
//
int linWriteMessage(const LinMessage msg);

// *****************************************************************************
// Reads data from a slave. A complete operation will be signalled via the
// linRequestCompleted callback
//
int linRequestMessage(const LinMessage msg);

// *****************************************************************************
// Updates the data that a slave will respond with.
//
int linUpdateMessage(const LinMessage msg);

// *****************************************************************************
// Clears a slave response message.
//
int linClearMessage(int id);

// *****************************************************************************
// Sends one or more wakeup frame(s)
//
// Constraints:
// Channel has to be open in slave mode
// count <= 255
// interval <= 65535
//
int linWriteWakeup(int channel, int count, int interval);

// *****************************************************************************
// Utility methods that may be handy when debugging scripts
// *****************************************************************************
void PrintWithTranslatedReturnCode(int channel, const char text[],
                                   int resultCode);

// *****************************************************************************
// Methods that has to be implemented by the user of this library
// *****************************************************************************

// *****************************************************************************
// This method will be called by the library after a LIN message has been
// transmitted. The method will only be called when the channel is opened in
// master mode.
//
void linMessageTransmitted(const LinMessage msg);

// *****************************************************************************
// This method will be called by the library after a LIN request has been
// completed. The method will only be called when the channel is opened in
// master mode.
//
void linRequestCompleted(const LinMessage msg);

// *****************************************************************************
// This method will be called by the library after a LIN message has been
// received. The method will only be called when the channel is opened in
// slave mode.
//
void linMessageReceived(const LinMessage msg);

// *****************************************************************************
// This method will be called by the library after a LIN wakeup has been
// received.
//
void linWakeupReceived(int channel);

#include <linimp.inc>
