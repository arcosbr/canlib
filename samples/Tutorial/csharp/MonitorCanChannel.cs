// This tutorial walks you through how to open a channel and read all CAN messages being sent.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Kvaser.CanLib;

namespace MonitorCanChannel
{
    class Program
    {
        static void Main(string[] args)
        {
            // Holds a handle to the CAN channel
            int handle;

            // Status returned by the Canlib calls
            Canlib.canStatus status;

            // The CANlib channel number we would like to use
            int channelNumber = 0;

            // Initialize the Canlib library with a call to
            // Canlib.initializeLibrary(). This always needs to be done before
            // doing anything with the library.
            Canlib.canInitializeLibrary();

            // Next, we open up the channel and receive a handle to
            // it. Depending on what devices you have connected to your
            // computer, you might want to change the channel number. The
            // canOPEN_ACCEPT_VIRTUAL flag means that it is ok to open the
            // selected channel, even if it is on a virtual device.
            handle = Canlib.canOpenChannel(channelNumber, Canlib.canOPEN_ACCEPT_VIRTUAL);
            CheckStatus((Canlib.canStatus)handle, "canOpenChannel");

            // Once we have successfully opened a channel, we need to set its bitrate. We
            // do this using canSetBusParams. CANlib provides a set of predefined bus parameter
            // settings in the form of canBITRATE_XXX constants. For other desired bus speeds
            // bus paramters have to be set manually.
            // See CANlib documentation for more information on parameter settings.
            status = Canlib.canSetBusParams(handle, Canlib.canBITRATE_250K, 0, 0, 0, 0);
            CheckStatus(status, "canSetBusParams");

            // Next, take the channel on bus using the canBusOn method. This
            // needs to be done before we can send a message.
            status = Canlib.canBusOn(handle);
            CheckStatus(status, "canBusOn");

            //Start dumping messages on the console
            DumpMessageLoop(handle);

            // Once we are done using the channel, we go off bus using the
            // canBusOff method. It take the handle as the only argument.
            status = Canlib.canBusOff(handle);
            CheckStatus(status, "canBusOff");

            // We also close the channel using the canCloseChannel method,
            // which take the handle as the only argument.
            status = Canlib.canClose(handle);
            CheckStatus(status, "canClose");


            // Wait for the user to press a key before exiting, in case the
            // console closes automatically on exit.
            Console.WriteLine("Channel closed. Press any key to exit");
            Console.ReadKey();
        }


        private static void DumpMessageLoop(int handle)
        {
            Canlib.canStatus status;
            bool finished = false;

            // Declare some variables for holding the incoming messages. The
            // incoming messages consist of the same parameters as an outgoing
            // message, i.e. identifier (id), body (data), length (dlc), and
            // flags), as well as a timestamp.
            byte[] data = new byte[8];
            int id;
            int dlc;
            int flags;
            long timestamp;

            Console.WriteLine("Channel opened. Press Escape to close. ");
            Console.WriteLine("ID  DLC DATA                      Timestamp");

            // Start a loop that loops until a key is pressed or an unexpected
            // error occur.
            while (!finished)
            {
                // Call the canReadWait method to wait for a message on the
                // channel. This method has a timeout parameter which in this
                // case is set to 100 ms. If a message is received during this
                // time, it will return the status code canOK and the message
                // will be written to the output parameters. If no message is
                // received, it will return canERR_NOMSG.
                status = Canlib.canReadWait(handle, out id, data, out dlc, out flags, out timestamp, 100);

                // If a message was received, display it
                if (status == Canlib.canStatus.canOK)
                {
                    DumpMessage(id, data, dlc, flags, timestamp);
                }

                // Call DisplayError and exit in case an actual error occurs
                else if (status != Canlib.canStatus.canERR_NOMSG)
                {
                    CheckStatus(status, "canReadWait");
                    finished = true;
                }

                // End the loop if the user presses the Escape key
                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                    {
                        finished = true;
                    }
                }
            }
        }


        //Prints an incoming message to the screen
        private static void DumpMessage(int id, byte[] data, int dlc, int flags, long timestamp)
        {
            if ((flags & Canlib.canMSG_ERROR_FRAME) != 0)
            {
                Console.WriteLine("Error Frame received ****");
            }
            else
            {
                Console.WriteLine("{0}  {1}  {2:x2} {3:x2} {4:x2} {5:x2} {6:x2} {7:x2} {8:x2} {9:x2}    {10}",
                                                 id, dlc, data[0], data[1], data[2], data[3], data[4],
                                                 data[5], data[6], data[7], timestamp);
            }
        }

        //This method prints an error if something goes wrong
        private static void CheckStatus(Canlib.canStatus status, string method)
        {
            if (status < 0)
            {
                Console.WriteLine(method + " failed: " + status.ToString());
            }
        }
    }
}

/*
Exercises:
 - Start this program, then run the SendMessage.cs program from another
   tutorial. Make sure to modify one of the programs so they use different
   channel numbers which both should be connected.

 - Send an error message to your program using the canMSG_ERROR_FRAME flag.

 - The canReadSync method waits until there is a message on the channel, but
   doesn't read the message. The canRead method reads messages, but returns
   canERR_NOMSG immediately if there is no message on the channel. Rewrite the
   program to use these methods instead of canReadWait.
*/
