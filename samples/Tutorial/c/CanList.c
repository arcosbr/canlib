/*
 * This tutorial demonstrates how to list available CAN channels and display
 * detailed information about each channel, including its description, UPC
 * number, serial number, firmware version, and channel-specific details.
 *
 * Exercises:
 * - The canGetChannelData method retrieves various pieces of information about
 *   each channel. Try modifying the parameters to request different types of
 *   data (e.g., canCHANNELDATA_CHANNEL_CAP for channel capabilities).
 *
 * - Use an external tool (such as Kvaser CanKing) to observe the available
 *   channels and compare the information retrieved by this program with the
 *   information displayed in the external tool.
 *
 * - Try adding error handling for cases where no channels are found.
 *   What message does the program output, and how could you make the program
 *   handle this more gracefully?
 *
 * - Experiment with connecting multiple CAN devices. How does the number of
 *   channels change when you add or remove a device?
 */

#include <stdio.h>
#include <stdint.h>

// To use CANlib, we need to include canlib.h and also link to canlib32.lib
// when compiling.
#include "canlib.h"

void list_channels(void);
static void check(char *id, canStatus stat);
void show_channel_info(int channel_no);

// Find out how many channels exist and loop over them
void list_channels(void)
{
    char driverName[256];
    int chanCount = 0;
    canStatus stat;
    int i;

    // Initialize the CAN library (note: no assignment needed)
    canInitializeLibrary();

    // Get the number of available channels
    stat = canGetNumberOfChannels(&chanCount);
    check("canGetNumberOfChannels", stat);

    printf("Found %d channel(s).\n", chanCount);

    // Loop through each available channel and display its information
    for (i = 0; i < chanCount; i++)
    {
        show_channel_info(i);
    }
}

// Print status message if stat contains an error code
static void check(char *id, canStatus stat)
{
    if (stat != canOK)
    {
        char buf[50];
        buf[0] = '\0';

        // Retrieve informational text about the status code
        canGetErrorText(stat, buf, sizeof(buf));
        printf("%s: failed, stat=%d (%s)\n", id, (int)stat, buf);
    }
}

void show_channel_info(int channel_no)
{
    canStatus stat;
    char name[256];
    char driverName[256];
    char custChanName[40];
    unsigned int ean[2], fw[2], serial[2];
    unsigned int chan_no_on_card;

    memset(name, 0, sizeof(name));
    memset(custChanName, 0, sizeof(custChanName));
    memset(ean, 0, sizeof(ean));
    memset(fw, 0, sizeof(fw));
    memset(serial, 0, sizeof(serial));

    stat = canGetChannelData(channel_no, canCHANNELDATA_DEVDESCR_ASCII,
                             &name, sizeof(name));
    check("canGetChannelData: DEVDESCR_ASCII", stat);

    stat = canGetChannelData(channel_no, canCHANNELDATA_CARD_UPC_NO,
                             &ean, sizeof(ean));
    check("canGetChannelData: CARD_UPC_NO", stat);

    stat = canGetChannelData(channel_no, canCHANNELDATA_CARD_SERIAL_NO,
                             &serial, sizeof(serial));
    check("canGetChannelData: CARD_SERIAL_NO", stat);

    stat = canGetChannelData(channel_no, canCHANNELDATA_CARD_FIRMWARE_REV,
                             &fw, sizeof(fw));
    check("canGetChannelData: CARD_FIRMWARE_REV", stat);

    stat = canGetChannelData(channel_no, canCHANNELDATA_CHAN_NO_ON_CARD,
                             &chan_no_on_card, sizeof(chan_no_on_card));
    check("canGetChannelData: canCHANNELDATA_CHAN_NO_ON_CARD", stat);

    canGetChannelData(channel_no, canCHANNELDATA_CUST_CHANNEL_NAME,
                      custChanName, sizeof(custChanName));

    printf("ch %2.1d: %-22s\t%x-%05x-%05x-%x, s/n %u, v%u.%u.%u :%d \"%s\"\n",
           channel_no, name, (ean[1] >> 12),
           ((ean[1] & 0xfff) << 8) | ((ean[0] >> 24) & 0xff),
           (ean[0] >> 4) & 0xfffff, (ean[0] & 0x0f), serial[0], fw[1] >> 16,
           fw[1] & 0xffff, fw[0] & 0xffff, chan_no_on_card, custChanName);
}

int main(void)
{
    // List available CAN channels
    list_channels();

    return 0;
}
