/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kvaser.LinLib;


namespace _1_HelloLIN_net481
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Channel/handle data
            int channel = 0;
            int handle = -1;

            //Message data
            int id = 0;
            byte[] msg = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
            int dlc = 8;

            //Incoming message data
            int inId = 0;
            byte[] inMsg = new byte[8];
            int inDlc = 0;
            int flags = 0;
            Linlib.LinMessageInfo msgInfo = new Linlib.LinMessageInfo();

            //Status
            Linlib.LinStatus status;


            //Initializing library
            Linlib.linInitializeLibrary();

            //Opening the channel, using our device as master
            Console.WriteLine("Opening channel");
            handle = Linlib.linOpenChannel(channel, Linlib.LIN_MASTER);
            CheckStatus((Linlib.LinStatus)handle, "Open channel");

            //Set bitrate
            Console.WriteLine("Setting bitrate");
            status = Linlib.linSetBitrate(handle, 20000);
            CheckStatus(status, "Set bitrate");

            //Go on bus
            Console.WriteLine("Going on bus");
            status = Linlib.linBusOn(handle);
            CheckStatus(status, "Bus On");

            //Send a message
            Console.WriteLine("Writing message");
            status = Linlib.linWriteMessage(handle, id, msg, dlc);
            CheckStatus(status, "Write Message");

            //Read the message
            Console.WriteLine("Reading message from bus");
            status = Linlib.linReadMessageWait(handle, out inId, inMsg, out inDlc, out flags, msgInfo, 100);
            CheckStatus(status, "Read Message");

            //Print the message
            Console.WriteLine("\nChecking if message matches the previously written one: ");
            Console.WriteLine("ID: {0}, expected: {1}", inId, id);
            Console.WriteLine("Message: {0}, expected: {1}", String.Join(",", inMsg), String.Join(",", msg));
            Console.WriteLine("DLC: {0}, expected: {1}", inDlc, dlc);
            Console.WriteLine("Message flags: {0}, expected 1", flags);
            Console.WriteLine(
                "Timestamp : {0}\n" +
                "Checksum: {1}\n" +
                "Bitrate: {2}",
                msgInfo.timestamp, msgInfo.checkSum, msgInfo.bitrate);


            //Going bus off and closing
            Console.WriteLine("Going off bus");
            status = Linlib.linBusOff(handle);
            CheckStatus(status, "Off bus");
            Console.WriteLine("Closing channel");
            status = Linlib.linClose(handle);
            CheckStatus(status, "Closing channel");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

        }

        private static void CheckStatus(Linlib.LinStatus status, string action)
        {
            if (status < 0)
            {
                Console.WriteLine("{0} failed, status {1}", action, status);
            }
        }
    }
}
