/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Kvaser.CanLib;

namespace _2_CanDump_T_net481
{
    internal class Program
    {
        const int channel = 0;
        const int slot = 0;
        static string scriptfile = @"2_CanDump.txe";

        static void Main(string[] args)
        {
            Canlib.canStatus status;

            //Channel setup
            Canlib.canInitializeLibrary();
            int chanhandle = Canlib.canOpenChannel(channel, Canlib.canOPEN_ACCEPT_VIRTUAL);
            if (chanhandle < 0)
            {
                CheckStatus((Canlib.canStatus)chanhandle, "Opening channel");
            }

            status = Canlib.canSetBusParams(chanhandle, Canlib.canBITRATE_250K, 0, 0, 0, 0);
            CheckStatus(status, "Setting bitrate");

            status = Canlib.canBusOn(chanhandle);
            CheckStatus(status, "Bus on");

            //Load our script to the device
            status = Canlib.kvScriptLoadFile(chanhandle, slot, scriptfile);
            CheckStatus(status, "Loading script");

            //Start the script
            status = Canlib.kvScriptStart(chanhandle, slot);
            CheckStatus(status, "Starting script");

            status = Canlib.kvScriptRequestText(chanhandle, slot, Canlib.kvSCRIPT_REQUEST_TEXT_SUBSCRIBE);
            Console.WriteLine("Listening for messages for 10 seconds");

            for (int i = 0; i < 100; i++)
            {
                string output;
                int s, f;
                long time;
                status = Canlib.kvScriptGetText(chanhandle, out s, out time, out f, out output);
                while (status == Canlib.canStatus.canOK)
                {
                    Console.WriteLine("Output: " + output);
                    status = Canlib.kvScriptGetText(chanhandle, out s, out time, out f, out output);
                }
                if (status != Canlib.canStatus.canERR_NOMSG)
                {
                    CheckStatus(status, "Reading text");
                }
                Thread.Sleep(100);
            }


            //Closing the channel
            status = Canlib.canBusOff(chanhandle);
            CheckStatus(status, "Bus off");
            status = Canlib.canClose(chanhandle);
            CheckStatus(status, "Closing channel");
            Canlib.canUnloadLibrary();
        }

        private static void CheckStatus(Canlib.canStatus status, string operation)
        {
            if (status != Canlib.canStatus.canOK)
            {
                string errorMessage;
                Canlib.canGetErrorText(status, out errorMessage);
                Console.WriteLine("{0} failed: {1}", operation, errorMessage);
            }
        }
    }
}
