﻿//-----------------------------------------------------------
// This is a sample program for Visual Studio 2017 CANlib tutorial. 
// It prints a list of connected CAN interfaces.
//
// For further information please refer to the tutorial section of the CANlib documentation.
//-----------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Kvaser.CanLib;

namespace CanlibTutorial
{
    class Program
    {
        // When called CheckForError will check for and print any error.
        // Return true if an error has occured.
        static public bool CheckForError(string cmd, Canlib.canStatus stat)
        {
            if (stat != Canlib.canStatus.canOK)
            {
                Canlib.canGetErrorText(stat, out string buf);

                Console.WriteLine("[{0}] {1}: failed, stat={2}", cmd, buf, (int)stat);
                return true;
            }
            return false;
        }

        // ListChannels prints a list of all connected CAN interfaces.
        static public void ListChannels()
        {
            Canlib.canStatus stat;

            // Get number channels
            stat = Canlib.canGetNumberOfChannels(out int number_of_channels);
            if (CheckForError("canGetNumberOfChannels", stat))
                return;

            Console.WriteLine("Found {0} channels", number_of_channels);


            // Loop and print all channels
            for (int i = 0; i < number_of_channels; i++)
            {
                stat = Canlib.canGetChannelData(i, Canlib.canCHANNELDATA_DEVDESCR_ASCII, out object device_name);
                if (CheckForError("canGetChannelData", stat))
                    return;

                stat = Canlib.canGetChannelData(i, Canlib.canCHANNELDATA_CHAN_NO_ON_CARD, out object device_channel);
                if (CheckForError("canGetChannelData", stat))
                    return;

                Console.WriteLine("Found channel: {0} {1} {2}", i, device_name, ((UInt32)device_channel + 1));
            }
        }

        static void Main(string[] args)
        {
            Canlib.canInitializeLibrary();

            ListChannels();

            // Press any key to continue
            Console.ReadKey(true);
        }
    }
}
