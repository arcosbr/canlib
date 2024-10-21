/**
 * Here we open a channel and print all the messages we receive.
 * SendMessage.c
 *
 * Exercises:
 * - Start this program, then run the SendMessage.c program from another
 *   tutorial. Make sure to modify one of the programs so they use different
 *   channel numbers which both should be connected.
 *
 * - Send an error message to your program using the canMSG_ERROR_FRAME flag.
 *
 * - The canReadSync method waits until there is a message on the channel, but
 *   doesn't read the message. The canRead method reads messages, but returns
 *   canERR_NOMSG immediately if there is no message on the channel.
 *   Rewrite the program to use these methods instead of canReadWait.
 */

#include <stdio.h>
#include <conio.h>

// To use CANlib, we need to include canlib.h and also link to canlib32.lib
// when compiling.
#include "canlib.h"

// The check method takes a canStatus (which is an enumerable) and the method
// name as a string argument. If the status is an error code, it will print it.
// Most Canlib method return a status, and checking it with a method like this
// is a useful practice to avoid code duplication.
void Check(const char *id, canStatus stat)
{
    if (stat != canOK)
    {
        char buf[50];
        buf[0] = '\0';
        canGetErrorText(stat, buf, sizeof(buf));
        printf("%s: failed, stat=%d (%s)\n", id, (int)stat, buf);
        exit(1);
    }
}

void dumpMessageLoop(canHandle hnd, int channel_number)
{
    // First declare some variables for holding the incoming messages. The
    // incoming messages consist of the same parameters as an outgoing message,
    // i.e. identifier (id), body (msg), length (dlc), and flags), as well as a
    // timestamp.
    canStatus stat = canOK;
    long id;
    unsigned int dlc, flags;
    unsigned char msg[8];
    DWORD timestamp;

    printf("Listening for messages on channel %d, press any key to close\n",
           channel_number);

    // Start a loop that loops until a key is pressed.
    while (!_kbhit())
    {

        // Call the canReadWait method to wait for a message on the channel.
        // This method has a timeout parameter which in this case is set to 100
        // ms. If a message is received during this time, it will return the
        // status code canOK and the message will be written to the output
        // parameters. If no message is received, it will return canERR_NOMSG.
        stat = canReadWait(hnd, &id, msg, &dlc, &flags, &timestamp, 100);
        // Check that the returned status is OK (which means that a message has
        // been received)
        if (stat == canOK)
        {
            // If the message contains an error flag (which implies a different
            // kind of error than if an error signal had been returned), an
            // error message will be shown.
            if (flags & canMSG_ERROR_FRAME)
            {
                printf("***ERROR FRAME RECEIVED***");
            }
            // If no error flag was found, the program prints the message.
            else
            {
                printf("Id: %ld, Msg: %u %u %u %u %u %u %u %u length: %u "
                       "Flags: %lu\n",
                       id, dlc, msg[0], msg[1], msg[2], msg[3], msg[4], msg[5],
                       msg[6], msg[7], timestamp);
            }
        }
        // Break the loop if something goes wrong, i.e. if we get a status code
        // that is not canOK (taken care of above) and not canERR_NOMSG
        else if (stat != canERR_NOMSG)
        {
            Check("canRead", stat);
            break;
        }
    }
}

void main(int argc, int *argv[])
{
    // Holds a handle to the CAN channel
    canHandle hnd;

    // Status returned by the Canlib calls
    canStatus stat;

    // The CANlib channel number we would like to use
    int channel_number = 0;

    // The first thing we need to do is to initialize the Canlib library. This
    // always needs to be done before doing anything with the library.
    canInitializeLibrary();

    printf("Opening channel %d\n", channel_number);

    // Next, we open up the channel and receive a handle to it. Depending on
    // what devices you have connected to your computer, you might want to
    // change the channel number. The canOPEN_ACCEPT_VIRTUAL flag means that it
    // is ok to open the selected channel, even if it is on a virtual device.
    hnd = canOpenChannel(channel_number, canOPEN_ACCEPT_VIRTUAL);

    // If the call to canOpenChannel is successful, it will return an integer
    // which is greater than or equal to zero. However, is something goes wrong,
    // it will return an error status which is a negative number.
    if (hnd < 0)
    {

        // To check for errors and print any possible error message, we can use
        // the Check method.
        Check("canOpenChannel", (canStatus)hnd);

        // and then exit the program.
        exit(1);
    }

    printf("Setting bitrate and going bus on\n");

    // Once we have successfully opened a channel, we need to set its bitrate.
    // We do this using canSetBusParams. CANlib provides a set of predefined bus
    // parameter settings in the form of canBITRATE_XXX constants. For other
    // desired bus speeds bus paramters have to be set manually. See CANlib
    // documentation for more information on parameter settings.
    stat = canSetBusParams(hnd, canBITRATE_250K, 0, 0, 0, 0, 0);
    Check("canSetBusParams", stat);

    // Next, take the channel on bus using the canBusOn method. This needs to be
    // done before we can read any messages from the bus.
    stat = canBusOn(hnd);
    Check("canBusOn", stat);

    // Listening for messages
    dumpMessageLoop(hnd, channel_number);

    printf("Going of bus and closing channel");

    // Once we are done using the channel, we go off bus using the
    // canBusOff method. It take the handle as the only argument.
    stat = canBusOff(hnd);
    Check("canBusOff", stat);

    // We also close the channel using the canCloseChannel method, which take
    // the handle as the only argument.
    stat = canClose(hnd);
    Check("canClose", stat);
}
