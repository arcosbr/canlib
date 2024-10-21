/**
 * This software is furnished as Redistributable under the Kvaser Software
 * Licence
 * https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html
 *
 * Description:
 * This is a 32-bit Windows console mode program which can transmit messages on
 * the CAN bus. It might be used as a traffic generator.
 */

#include <stdio.h>
#include <stdlib.h>
#include <windows.h>
#include <memory.h>
#include <conio.h>

// To use CANlib, we need to include canlib.h and also link to canlib32.lib
// when compiling.
#include "canlib.h"

void Usage(int argc, char *argv[]);
void Check(const char *id, canStatus stat);
int InitCtrl(int ctrl);
void Send(void);
int getBitrateValue(int bitrate_constant);

//
// Global variables for the command-line options.
//
int Tseg1 = 0, Tseg2 = 0;
int Bitrate = canBITRATE_250K;
int Channel = 0;
int WaitTime = 100;     // Default wait time set to 100 ms
int Fail = FALSE;
int BurstSize = 1;      // Default burst size set to 1
int Quiet = 0;
int MessageLength = 8;
int Loopback = FALSE;
int CurrentCount = 0;

//
// Function to convert the bitrate constant to the actual value in kbps.
//
int getBitrateValue(int bitrate_constant)
{
    switch (bitrate_constant)
    {
    case canBITRATE_1M:
        return 1000000;
    case canBITRATE_500K:
        return 500000;
    case canBITRATE_250K:
        return 250000;
    case canBITRATE_125K:
        return 125000;
    default:
        return 0; // Unknown bitrate
    }
}

//
// Check a status code and display an error message if the code isn't canOK.
//
void Check(const char *id, canStatus stat)
{
    if (stat != canOK)
    {
        char buf[50];
        buf[0] = '\0';
        canGetErrorText(stat, buf, sizeof(buf));
        printf("%s: failed, stat=%d (%s)\n", id, (int)stat, buf);
        exit(1); // Exit on error
    }
}

//
// Setup a CAN controller.
//
int InitCtrl(int ctrl)
{
    canStatus stat;
    canHandle hnd;
    int actualBitrate;

    // Open a handle to the CAN channel (Allow virtual channels)
    printf("Opening CAN channel %d... ", ctrl);
    hnd = canOpenChannel(ctrl, canOPEN_ACCEPT_VIRTUAL);

    if (hnd < 0)
    {
        Check("canOpenChannel", (canStatus)hnd);
    }

    printf("OK.\n");

    // Set the bitrate
    printf("Setting the bus speed to ");
    actualBitrate = getBitrateValue(Bitrate);
    if (actualBitrate > 0)
    {
        printf("%d", actualBitrate);
    }
    else
    {
        printf("Unknown bitrate");
    }
    printf("... ");

    stat = canSetBusParams(hnd, Bitrate, Tseg1, Tseg2, 1, 1, 0);

    if (stat != canOK)
    {
        Check("canSetBusParams", stat);
    }

    printf("OK.\n");

    // Set the channel to bus-on
    printf("Going bus-on... ");
    stat = canBusOn(hnd);

    if (stat != canOK)
    {
        Check("canBusOn", stat);
    }

    printf("OK.\n");

    return hnd;
}

//
// Transmit messages on the CAN bus.
//
void Send(void)
{
    canHandle hnd;
    canStatus stat;
    int i;
    int ready = FALSE;
    const long srcID = 123L;  // Arbitrary source ID for the CAN message
    char msg[8] = {1, 2, 3, 4, 5, 6, 7, 8};  // Example CAN message

    // Open a channel
    hnd = InitCtrl(Channel);

    if (Loopback)
    {
        (void)InitCtrl(Channel + 1);
    }

    // Display the wait time between messages
    printf("Wait time between messages: %d ms\n", WaitTime);

    while (!ready)
    {
        // Send a burst of messages
        for (i = 0; i < BurstSize; i++)
        {
            stat = canWrite(hnd, srcID, msg, MessageLength, 0);

            if (Quiet <= 1)
            {
                Check("canWrite", stat);
                if ((stat != canOK) && Fail)
                {
                    ready = TRUE;
                    break;
                }
            }

            CurrentCount++;

            // Wait for the message to be sent (timeout of 100 ms)
            stat = canWriteSync(hnd, 100);
            Check("canWriteSync", stat);

            // Wait for WaitTime ms between messages
            Sleep(WaitTime);
        }

        // Break if key is hit
        if (_kbhit())
        {
            break;
        }
    }

    printf("\nTransmitted %d messages.\n", CurrentCount);

    canBusOff(hnd);
    canClose(hnd);
}

//
// Parse command-line arguments and run the program.
//
int main(int argc, char *argv[])
{
    int i;

    // Parse command-line arguments
    for (i = 1; i < argc; i++)
    {
        int tmp;
        char c;
        if (sscanf(argv[i], "-%d%c", &tmp, &c) == 1)
        {
            Channel = tmp;
        }
        else if (sscanf(argv[i], "-B%d%c", &tmp, &c) == 1)
        {
            switch (tmp)
            {
            case 1000:
                Bitrate = canBITRATE_1M;
                break;
            case 500:
                Bitrate = canBITRATE_500K;
                break;
            case 250:
                Bitrate = canBITRATE_250K;
                break;
            case 125:
                Bitrate = canBITRATE_125K;
                break;
            default:
                Usage(argc, argv);
            }
        }
        else if (sscanf(argv[i], "-wait%d%c", &tmp, &c) == 1)
        {
            WaitTime = tmp;
        }
        else if (sscanf(argv[i], "-burst%d%c", &tmp, &c) == 1)
        {
            BurstSize = tmp;
        }
        else if (strcmp(argv[i], "-fail") == 0)
        {
            Fail = TRUE;
        }
        else if (strcmp(argv[i], "-loopback") == 0)
        {
            Loopback = TRUE;
        }
        else if (strcmp(argv[i], "-q") == 0)
        {
            Quiet++;
        }
        else
        {
            Usage(argc, argv);
        }
    }

    // Initialize the CAN library
    canInitializeLibrary();

    // Start the transmission
    printf("Starting...\n");
    Send();
    printf("\nThat's all for today!\n");

    return 0;
}

//
// Display usage information.
//
void Usage(int argc, char *argv[])
{
    printf("\nCANLIB Traffic Generator\n\n");
    printf("(Part of the CANLIB SDK from KVASER AB - http://www.kvaser.se)\n");
    printf("Usage: CanSend [flags]\n");
    printf("   -X          Use channel number X as source. (Default 0.)\n");
    printf("   -waitXXX    Sleep for XXX milliseconds between messages.\n");
    printf("   -burstXXX   Send XXX messages in each burst.\n");
    printf("   -B<value>   Set the bitrate. Value is any of 1000,500,250,125.\n");
    printf("   -fail       Exit the program immediately if transmission fails.\n");
    printf("   -loopback   Run a loopback between channel X and X+1.\n");
    printf("   -q          Be quiet.\n");

    exit(1);
}
