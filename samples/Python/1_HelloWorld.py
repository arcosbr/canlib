# This software is furnished as Redistributable under the Kvaser Software Licence
# https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html

import sys

from canlib import canlib, Frame

channel_number = 0
# Specific CANlib channel number may be specified as first argument
if len(sys.argv) == 2:
    channel_number = int(sys.argv[1])

print("Opening channel %d" % (channel_number))
# Use ChannelData to get some information about the selected channel
chd = canlib.ChannelData(channel_number)
print("%d. %s (%s / %s) " % (channel_number,
                             chd.channel_name,
                             chd.card_upc_no,
                             chd.card_serial_no))

# If the channel have a custom name, print it
if chd.custom_name != '':
    print("Customized Channel Name: %s " % (chd.custom_name))

# Open CAN channel, virtual channels are considered ok to use
ch = canlib.openChannel(channel_number, canlib.canOPEN_ACCEPT_VIRTUAL)

print("Setting bitrate to 250 kb/s")
ch.setBusParams(canlib.canBITRATE_250K)

print("Going on bus")
ch.busOn()

print("Sending a message")
frame = Frame(id_=123,
              data=[1, 2, 3, 4, 5, 6, 7, 8],
              dlc=8,
              flags=0)
ch.write(frame)
print("Going off bus")
ch.busOff()

print("Closing channel")
ch.close()
