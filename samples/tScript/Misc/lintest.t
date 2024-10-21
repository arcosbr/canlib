#include <linlib.inc>

variables
{
  const int RUNMODE_UNDEFINED = 0;
  const int RUNMODE_MASTER    = 1;
  const int RUNMODE_SLAVE     = 2;

  int runMode = RUNMODE_UNDEFINED;
  int sequenceState = 0;
  int channelToUse = 0;
  int aSingleMsg = 0;
  int curBitrate = LIN_DEFAULT_BITRATE;
  int curFlags = 0;

  Timer StateTimer;
  int lastLinState = -1;//LIN_STATE_UNINITIALIZED;
  int lastLinState1 = -1;//LIN_STATE_UNINITIALIZED;

  LinMessage msg[6];
}

// *****************************************************************************
void FixupMessages()
{
  // Without variable dlc the messages are constrained to:
  // id  0 - 31 -> dlc = 2
  // id 32 - 47 -> dlc = 4
  // id 48 - 63 -> dlc = 8

  if (curFlags & LIN_FLAG_VARIABLE_DLC)
  {
    msg[0].msg.dlc = 8;
    msg[1].msg.dlc = 8;
    msg[2].msg.dlc = 8;
    msg[3].msg.dlc = 8;
    msg[4].msg.dlc = 8;
    msg[5].msg.dlc = 8;
  }
  else
  {
    msg[0].msg.dlc = 2;
    msg[1].msg.dlc = 4;
    msg[2].msg.dlc = 8;
    msg[3].msg.dlc = 2;
    msg[4].msg.dlc = 4;
    msg[5].msg.dlc = 8;
  }
}

// *****************************************************************************
on start
{
  msg[0].msg.channel = channelToUse;
  msg[0].msg.id = 30; msg[0].msg.flags = 0; msg[0].msg.dlc = 8;
  msg[0].msg.data = "\x11\x22\x33\x44\x55\x66\x77\x88";

  msg[1].msg.channel = channelToUse;
  msg[1].msg.id = 40; msg[1].msg.flags = 0; msg[1].msg.dlc = 8;
  msg[1].msg.data = "\x88\x77\x66\x55\x44\x33\x22\x11";

  msg[2].msg.channel = channelToUse;
  msg[2].msg.id = 50; msg[2].msg.flags = 0; msg[2].msg.dlc = 8;
  msg[2].msg.data = "\x22\x11\x44\x22\x66\x55\x88\x77";

  msg[3].msg.channel = channelToUse;
  msg[3].msg.id = 31; msg[3].msg.flags = 0; msg[3].msg.dlc = 8;
  msg[3].msg.data = "\x88\x99\xAA\xBB\xCC\xDD\xEE\xFF";

  msg[4].msg.channel = channelToUse;
  msg[4].msg.id = 41; msg[4].msg.flags = 0; msg[4].msg.dlc = 8;
  msg[4].msg.data = "\xFF\xEE\xDD\xCC\xBB\xAA\x99\x88";

  msg[5].msg.channel = channelToUse;
  msg[5].msg.id = 51; msg[5].msg.flags = 0; msg[5].msg.dlc = 8;
  msg[5].msg.data = "\x99\x88\xBB\xAA\xDD\xCC\xFF\xEE";

  FixupMessages();

  StateTimer.timeout = 1;
  timerStart(StateTimer);
}

// *****************************************************************************
on stop
{
  int linState = linLibraryState(channelToUse);
  if ((linState == LIN_STATE_BUS_OFF) || (linState == LIN_STATE_BUS_ON))
  {
    linCloseChannel(channelToUse);
    printf("Channel %d - Lin library closed", channelToUse);
  }
}

// *****************************************************************************
on Timer StateTimer
{
  int newLinState = linLibraryState(channelToUse);
  if (lastLinState != newLinState)
  {
    switch (newLinState)
    {
    case LIN_STATE_UNINITIALIZED:
      printf("Channel %d - Lin library uninitialized", channelToUse);
      break;
    case LIN_STATE_OPERATION_IN_PROGRESS:
      printf("Channel %d - Lin library initializing", channelToUse);
      break;
    case LIN_STATE_BUS_OFF:
      printf("Channel %d - Lin library in bus off", channelToUse);
      break;
    case LIN_STATE_BUS_ON:
      printf("Channel %d - Lin library operational and bus on", channelToUse);
      if (runMode == RUNMODE_MASTER)
      {
        linUpdateMessage(msg[3]);
        linUpdateMessage(msg[4]);
        linUpdateMessage(msg[5]);
      }
      break;
    case LIN_STATE_INTERNAL_FAILURE:
      printf("Channel %d - Lin library has an internal failure", channelToUse);
      break;
    }
    lastLinState = newLinState;
  }

  newLinState = linLibraryState(1);
  if (lastLinState1 != newLinState)
  {
    switch (newLinState)
    {
    case LIN_STATE_UNINITIALIZED:
      printf("Channel %d - Lin library uninitialized", 1);
      break;
    case LIN_STATE_OPERATION_IN_PROGRESS:
      printf("Channel %d - Lin library initializing", 12000);
      break;
    case LIN_STATE_BUS_OFF:
      printf("Channel %d - Lin library in bus off", 1);
      break;
    case LIN_STATE_BUS_ON:
      printf("Channel %d - Lin library operational and bus on", 1);
      break;
    case LIN_STATE_INTERNAL_FAILURE:
      printf("Channel %d - Lin library has an internal failure", 1);
      break;
    }
    lastLinState1 = newLinState;
  }

  timerStart(StateTimer);
}

// *****************************************************************************
void MasterSequence()
{
  if (aSingleMsg)
  {
    aSingleMsg = 0;
    return;
  }

  int result = LIN_RETURN_CODE_SUCCESS;

  if (sequenceState < 3)
  {
    result = linWriteMessage(msg[sequenceState]);
  }
  else if (sequenceState < 6)
  {
    result = linRequestMessage(msg[sequenceState]);
  }
  else
  {
    sequenceState = -1;  // Make the sequence runnable again
  }
  if (result != LIN_RETURN_CODE_SUCCESS)
  {
    printf("Channel %d - Seq %d operation failed with result %d", channelToUse, sequenceState, result);
  }

  sequenceState++;
}

// *****************************************************************************
void RotateMessageData(LinMessage msg)
{
  int buf;

  buf = msg.msg.data[0];
  msg.msg.data[0] = msg.msg.data[1];
  msg.msg.data[1] = msg.msg.data[2];
  msg.msg.data[2] = msg.msg.data[3];
  msg.msg.data[3] = msg.msg.data[4];
  msg.msg.data[4] = msg.msg.data[5];
  msg.msg.data[5] = msg.msg.data[6];
  msg.msg.data[6] = msg.msg.data[7];
  msg.msg.data[7] = buf;
}

// *****************************************************************************
void SlaveSequence()
{
  RotateMessageData(msg[3]);

  int result = linUpdateMessage(msg[3]);
  if (result != LIN_RETURN_CODE_SUCCESS)
  {
    printf("Channel %d - Msg 3 update failed with result %d", channelToUse, result);
  }

  RotateMessageData(msg[4]);
  result = linUpdateMessage(msg[4]);
  if (result != LIN_RETURN_CODE_SUCCESS)
  {
    printf("Channel %d - Msg 4 update failed with result %d", channelToUse, result);
  }

  RotateMessageData(msg[5]);
  result = linUpdateMessage(msg[5]);
  if (result != LIN_RETURN_CODE_SUCCESS)
  {
    printf("Channel %d - Msg 5 update failed with result %d", channelToUse, result);
  }
}

// *****************************************************************************
void PrintLinMessage(const char text[], const LinMessage msg, int maskOffId)
{
  int locId = msg.msg.id;

  if (maskOffId) locId = locId & 0x3F;

  printf("Channel %d - %s flags=0x%04x id=0x%03x dlc=%x data=0x%02x 0x%02x 0x%02x 0x%02x 0x%02x 0x%02x 0x%02x 0x%02x",
         msg.msg.channel, text, msg.msg.flags, locId, msg.msg.dlc,
         msg.msg.data[0], msg.msg.data[1], msg.msg.data[2], msg.msg.data[3],
         msg.msg.data[4], msg.msg.data[5], msg.msg.data[6], msg.msg.data[7]);
}

// *****************************************************************************
void RunSequence()
{
  if (runMode == RUNMODE_MASTER)
  {
    MasterSequence();
  }
  else
  {
    SlaveSequence();
  }
}

// *****************************************************************************
void linRequestCompleted(const LinMessage msg)
{
  if (msg.msg.channel != channelToUse) return;
  if (runMode == RUNMODE_UNDEFINED) return;
  PrintLinMessage("linRequestCompleted: ", msg, 1);
  RunSequence();
}

// *****************************************************************************
void linMessageReceived(const LinMessage msg)
{
  if (msg.msg.channel != channelToUse) return;
  if (runMode == RUNMODE_UNDEFINED) return;
  PrintLinMessage("linMessageReceived: ", msg, 1);
  RunSequence();
}

// *****************************************************************************
void linMessageTransmitted(const LinMessage msg)
{
  if (msg.msg.channel != channelToUse) return;
  if (runMode == RUNMODE_UNDEFINED) return;
  PrintLinMessage("linMessageTransmitted: ", msg, 1);
  RunSequence();
}

// *****************************************************************************
void linWakeupReceived(int channel)
{
  printf("linWakeupReceived on channel %d", channel);
}

// *****************************************************************************
on key '?'
{
  printf("a: Set master mode");
  printf("b: Set slave mode");
  printf("c: Set default bitrate");
  printf("d: Set bitrate to 12000");
  printf("e: Reset flags to 0");
  printf("f: Add enhanced checksum to flags");
  printf("g: Add variable dlc to flags");
  printf("h: Open LIN channel");
  printf("i: Setup LIN (flags & bitrate)");
  printf("j: Close LIN channel");
  printf("k: Go bus off");
  printf("l: Go bus on");
  printf("m: Send wakeup");
  printf("n: Send message sequence");
  printf("o: Send a single message");
  printf("p: Read a single message");
}

// *****************************************************************************
on key 'a'
{
  int linState = linLibraryState(channelToUse);
  if (linState != LIN_STATE_UNINITIALIZED)
  {
    printf("Channel %d - Can't change run mode when running", channelToUse);
  }
  else
  {
    printf("Channel %d - Master run mode set", channelToUse);
  }
  runMode = RUNMODE_MASTER;
}

// *****************************************************************************
on key 'b'
{
  int linState = linLibraryState(channelToUse);
  if (linState != LIN_STATE_UNINITIALIZED)
  {
    printf("Channel %d - Can't change run mode when running", channelToUse);
  }
  else
  {
    printf("Channel %d - Slave run mode set", channelToUse);
  }
  runMode = RUNMODE_SLAVE;
}

// *****************************************************************************
on key 'c'
{
  curBitrate = LIN_DEFAULT_BITRATE;
  int result = linSetBitrate(channelToUse, curBitrate);
  if (result == LIN_RETURN_CODE_SUCCESS || result == LIN_RETURN_CODE_RESULT_PENDING)
  {
    printf("Channel %d - Bitrate set to %d", curBitrate);
  }
  else
  {
    PrintWithTranslatedReturnCode(channelToUse, "Action failed", result);
  }
}

// *****************************************************************************
on key 'd'
{
  curBitrate = 12000;
  int result = linSetBitrate(channelToUse, curBitrate);
  if (result == LIN_RETURN_CODE_SUCCESS || result == LIN_RETURN_CODE_RESULT_PENDING)
  {
    printf("Channel %d - Bitrate set to %d", curBitrate);
  }
  else
  {
    PrintWithTranslatedReturnCode(channelToUse, "Action failed", result);
  }
}

// *****************************************************************************
on key 'e'
{
  curFlags = 0;
  printf("Channel %d - Standard checksum & no variable dlc set", channelToUse);
}
// *****************************************************************************
on key 'f'
{
  curFlags |= LIN_FLAG_ENHANCED_CHECKSUM;
  printf("Channel %d - Enhanced checksum added to the setup", channelToUse);
}
// *****************************************************************************
on key 'g'
{
  curFlags |= LIN_FLAG_VARIABLE_DLC;
  printf("Channel %d - Variable dlc added to the setup", channelToUse);
}

// *****************************************************************************
on key 'h'
{
  if (runMode == RUNMODE_UNDEFINED)
  {
    printf("Channel %d - No run mode defined", channelToUse);
    return;
  }

  int result;

  if (runMode == RUNMODE_MASTER)
  {
    result = linOpenChannel(channelToUse, LIN_MODE_MASTER);
  }
  else
  {
    result = linOpenChannel(channelToUse, LIN_MODE_SLAVE);
  }

  if ( !(result == LIN_RETURN_CODE_SUCCESS || result == LIN_RETURN_CODE_RESULT_PENDING))
  {
    PrintWithTranslatedReturnCode(channelToUse, "Action failed", result);
  }
}

// *****************************************************************************
on key 'i'
{
  int result = linSetupLIN(channelToUse, curFlags, curBitrate);
  if (result == LIN_RETURN_CODE_SUCCESS || result == LIN_RETURN_CODE_RESULT_PENDING)
  {
    FixupMessages();
  }
  else
  {
    PrintWithTranslatedReturnCode(channelToUse, "Action failed", result);
  }
}

// *****************************************************************************
on key 'j'
{
  int result = linCloseChannel(channelToUse);
  if ( !(result == LIN_RETURN_CODE_SUCCESS || result == LIN_RETURN_CODE_RESULT_PENDING))
  {
    PrintWithTranslatedReturnCode(channelToUse, "Action failed", result);
  }
}

// *****************************************************************************
on key 'k'
{
  int result = linBusOff(channelToUse);
  if ( !(result == LIN_RETURN_CODE_SUCCESS || result == LIN_STATE_OPERATION_IN_PROGRESS))
  {
    PrintWithTranslatedReturnCode(channelToUse, "Action failed", result);
  }
}

// *****************************************************************************
on key 'l'
{
  int result = linBusOn(channelToUse);
  if ( !(result == LIN_RETURN_CODE_SUCCESS || result == LIN_STATE_OPERATION_IN_PROGRESS))
  {
    PrintWithTranslatedReturnCode(channelToUse, "Action failed", result);
  }
}

// *****************************************************************************
on key 'm'
{
  int result = linWriteWakeup(channelToUse, 5, 10);
  if ( result != LIN_RETURN_CODE_SUCCESS)
  {
    PrintWithTranslatedReturnCode(channelToUse, "Action failed", result);
  }
}

// *****************************************************************************
on key 'n'
{
  if (runMode == RUNMODE_UNDEFINED)
  {
    printf("Channel %d - No run mode defined", channelToUse);
    return;
  }

  RunSequence();
}

// *****************************************************************************
on key 'o'
{
  if (runMode == RUNMODE_UNDEFINED)
  {
    printf("Channel %d - No run mode defined", channelToUse);
    return;
  }

  aSingleMsg = 1;
  int result = linWriteMessage(msg[0]);
  if ( !(result == LIN_RETURN_CODE_SUCCESS || result == LIN_STATE_OPERATION_IN_PROGRESS))
  {
    PrintWithTranslatedReturnCode(channelToUse, "Action failed", result);
  }
}

// *****************************************************************************
on key 'p'
{
  if (runMode == RUNMODE_UNDEFINED)
  {
    printf("Channel %d - No run mode defined", channelToUse);
    return;
  }

  aSingleMsg = 1;
  int result = linRequestMessage(msg[3]);
  if ( !(result == LIN_RETURN_CODE_SUCCESS || result == LIN_STATE_OPERATION_IN_PROGRESS))
  {
    PrintWithTranslatedReturnCode(channelToUse, "Action failed", result);
  }
}
