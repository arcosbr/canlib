# This software is furnished as Redistributable under the Kvaser Software Licence
# https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html

import sys

from canlib import canlib


def print_frame(frame):
    """Prints a message to screen"""
    if (frame.flags & canlib.canMSG_ERROR_FRAME != 0):
        print("***ERROR FRAME RECEIVED***")
    else:
        print("{id:0>8b}  {dlc}  {data}  {timestamp}".format(
            id=frame.id,
            dlc=frame.dlc,
            data=' '.join('%02x' % i for i in frame.data),
            timestamp=frame.timestamp
        ))


if __name__ == '__main__':
    # Initialization
    channel_number = 0

    # Specific CANlib channel number may be specified as first argument
    if len(sys.argv) == 2:
        channel_number = int(sys.argv[1])

    print("Opening channel %d" % (channel_number))
    # Open CAN channel, virtual channels are considered ok to use
    ch = canlib.openChannel(channel_number, canlib.canOPEN_ACCEPT_VIRTUAL)

    print("Setting bitrate to 250 kb/s")
    ch.setBusParams(canlib.canBITRATE_250K)
    ch.busOn()

    # Start listening for messages
    finished = False
    print("   ID    DLC DATA                     Timestamp")
    while not finished:
        try:
            frame = ch.read(timeout=50)
            print_frame(frame)
        except(canlib.canNoMsg) as ex:
            None
        except (canlib.canError) as ex:
            print(ex)
            finished = True

    # Channel teardown
    ch.busOff()
    ch.close()
