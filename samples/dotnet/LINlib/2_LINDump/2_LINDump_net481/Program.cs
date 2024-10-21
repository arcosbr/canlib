/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html
*/

using System;
using Kvaser.LinLib;

namespace _2_LINDump_net481
{
    internal class Program
    {
        private static readonly int CHANNEL = 0;

        static void Main(string[] args)
        {
            //Channel/handle data
            int handle = -1;

            //Status
            Linlib.LinStatus status;

            Linlib.linInitializeLibrary();

            //Channel setup, notice that we're going to use the device in slave mode
            handle = Linlib.linOpenChannel(CHANNEL, Linlib.LIN_SLAVE);
            CheckStatus((Linlib.LinStatus)handle, "Open channel");
            status = Linlib.linSetBitrate(handle, 20000);
            CheckStatus(status, "Set bitrate");
            status = Linlib.linBusOn(handle);
            CheckStatus(status, "Bus On");

            DumpMessageLoop(handle);

            //Closing the channel
            status = Linlib.linBusOff(handle);
            CheckStatus(status, "Off bus");
            status = Linlib.linClose(handle);
            CheckStatus(status, "Closing channel");

        }

        private static void DumpMessageLoop(int handle)
        {
            //Incoming message data
            int id = 0;
            byte[] msg = new byte[8];
            int dlc = 0;
            int flags = 0;
            Linlib.LinMessageInfo msgInfo = new Linlib.LinMessageInfo();

            bool finished = false;
            Linlib.LinStatus status;

            Console.WriteLine("Channel opened. Press Escape to close. ");
            Console.WriteLine("TX/RX ID  DLC DATA                      Timestamp");

            while (!finished)
            {
                //Read a message
                status = Linlib.linReadMessageWait(handle, out id, msg, out dlc, out flags, msgInfo, 100);
                if (status == Linlib.LinStatus.linOK)
                {
                    //Print the message
                    PrintMessage(id, msg, dlc, flags, msgInfo);
                }
                //If status is not OK or NOMSG, something has gone wrong
                else if (status != Linlib.LinStatus.linERR_NOMSG)
                {
                    CheckStatus(status, "Reading message");
                    finished = true;
                }
                //Breaks the loop if the user presses the Escape key
                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.Escape)
                    {
                        finished = true;
                    }
                }
            }
        }

        //Prints an incoming message
        private static void PrintMessage(int id, byte[] data, int dlc, int flags, Linlib.LinMessageInfo msgInfo)
        {
            //Check if the message was transmitted by us or received
            if ((flags & Linlib.LIN_TX) != 0)
            {
                Console.Write("TX: ");
            }
            else if ((flags & Linlib.LIN_RX) != 0)
            {
                Console.Write("RX: ");
            }
            //Check for error or wakeup frames
            if ((flags & (Linlib.LIN_CSUM_ERROR | Linlib.LIN_PARITY_ERROR | Linlib.LIN_BIT_ERROR | Linlib.LIN_SYNCH_ERROR)) != 0)
            {
                Console.WriteLine("***ERROR FRAME RECEIVED***");
            }
            else if ((flags & Linlib.LIN_WAKEUP_FRAME) != 0)
            {
                Console.WriteLine("***WAKEUP FRAME RECEIVED***");
            }
            //Print any regular messages
            else
            {
                Console.WriteLine("  {0}  {1}  {2:x2} {3:x2} {4:x2} {5:x2} {6:x2} {7:x2} {8:x2} {9:x2}    {10}",
                                                     id, dlc, data[0], data[1], data[2], data[3], data[4],
                                                     data[5], data[6], data[7], msgInfo.timestamp);
            }
        }

        //Displays an error message if the status is not OK
        private static void CheckStatus(Linlib.LinStatus status, string action)
        {
            if (status < 0)
            {
                Console.WriteLine("{0} failed, status {1}", action, status);
            }
        }
    }
}
