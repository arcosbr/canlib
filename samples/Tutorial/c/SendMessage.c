/*
 * This tutorial demonstrates how to open a CAN channel and send a message on it.
 *
 * Exercises:
 * - The canWriteWait method combines canWrite with canWriteSync. Try it out.
 *
 * - Use some other program (such as Kvaser CanKing) to listen for messages on
 *   a different channel on the same device as the one used in your
 *   program. Make sure to use the same bitrate.
 *
 * - Change the fourth parameter in the call to canWrite to 4. What happens to
 *   the message on the receiving side?
 *
 * - Change the message identifier to something large, like 10000. What happens
 *   on the receiving side? Then, change the fifth parameter to
 *   canMSG_EXT. What happens now?
 */

#include <stdio.h>

// To use CANlib, we need to include canlib.h and also link to canlib32.lib
// when compiling.
#include "canlib.h"

// The check method takes a canStatus (which is an enumerable) and the method
// name as a string argument. If the status is an error code, it will print it.
// Most Canlib method return a status, and checking it with a method like this
// is a useful practice to avoid code duplication.
void Check(const char* id, canStatus stat)
{
  if (stat != canOK) {
    char buf[50];
    buf[0] = '\0';
    canGetErrorText(stat, buf, sizeof(buf));
    printf("%s: failed, stat=%d (%s)\n", id, (int)stat, buf);
  }
}

void main(int argc, char* argv[]){
  // Holds a handle to the CAN channel
  canHandle hnd;

  // Status returned by the Canlib calls
  canStatus stat;

  // The CANlib channel number we would like to use
  int channel_number = 0;

  // The msg will be the body of the message we send on the CAN bus.
  char msg[8] = {1, 2, 3, 4, 5, 6, 7, 8};

  // The first thing we need to do is to initialize the Canlib library. This
  // always needs to be done before doing anything with the library.
  canInitializeLibrary();

  printf("Opening channel %d\n", channel_number);

  // Next, we open up the channel and receive a handle to it. Depending on what
  // devices you have connected to your computer, you might want to change the
  // channel number. The canOPEN_ACCEPT_VIRTUAL flag means that it is ok to
  // open the selected channel, even if it is on a virtual device.
  hnd = canOpenChannel(channel_number, canOPEN_ACCEPT_VIRTUAL);

  // If the call to canOpenChannel is successful, it will return an integer
  // which is greater than or equal to zero. However, is something goes wrong,
  // it will return an error status which is a negative number.
  if(hnd < 0){

    // To check for errors and print any possible error message, we can use the
    // Check method.
    Check("canOpenChannel", (canStatus)hnd);

    // and then exit the program.
    exit(1);
  }

  printf("Setting bitrate and going bus on\n");

  // Once we have successfully opened a channel, we need to set its bitrate. We
  // do this using canSetBusParams. CANlib provides a set of predefined bus parameter
  // settings in the form of canBITRATE_XXX constants. For other desired bus speeds
  // bus paramters have to be set manually.
  // See CANlib documentation for more information on parameter settings.
  stat = canSetBusParams(hnd, canBITRATE_250K, 0, 0, 0, 0, 0);
  Check("canSetBusParams", stat);

  // Next, take the channel on bus using the canBusOn method. This needs to be
  // done before we can send a message.
  stat = canBusOn(hnd);
  Check("canBusOn", stat);

  printf("Writing a message to the channel and waiting for it to be sent \n");

  // We send the message using canWrite. This method takes five parameters:
  // the channel handle, the message identifier, the message body, the message
  // length (in bytes) and optional flags.
  stat = canWrite(hnd, 123, msg, 8, 0);
  Check("canWrite", stat);

  // After sending, we wait for at most 100 ms for the message to be sent, using
  // canWriteSync.
  stat = canWriteSync(hnd, 100);
  Check("canWriteSync", stat);

  printf("Going off bus and closing channel");

  // Once we are done using the channel, we go off bus using the
  // canBusOff method. It take the handle as the only argument.
  stat = canBusOff(hnd);
  Check("canBusOff", stat);

  // We also close the channel using the canCloseChannel method, which take the
  // handle as the only argument.
  stat = canClose(hnd);
  Check("canClose", stat);
}

