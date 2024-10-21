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

namespace _1_HelloWorld_T_net481
{
    internal class Program
    {
        //CAN channel off the device
        const int channel = 0;
        //The slot on the device on which we want to load the script
        const int slot = 0;
        //Name of the compiled t script file
        static string scriptfile = "1_HelloWorld.txe";

        static void Main(string[] args)
        {
            Canlib.canStatus status;

            //Channel setup
            Canlib.canInitializeLibrary();
            int chanhandle = Canlib.canOpenChannel(channel, Canlib.canOPEN_ACCEPT_VIRTUAL);
            CheckStatus((Canlib.canStatus)chanhandle, "Opening channel");

            status = Canlib.canSetBusParams(chanhandle, Canlib.canBITRATE_250K, 0, 0, 0, 0);
            CheckStatus((Canlib.canStatus)chanhandle, "Setting bitrate");

            status = Canlib.canBusOn(chanhandle);
            CheckStatus((Canlib.canStatus)chanhandle, "Bus on");


            //Stop and unload any running script on the slot
            Canlib.kvScriptStop(chanhandle, slot, Canlib.kvSCRIPT_STOP_NORMAL);
            Canlib.kvScriptUnload(chanhandle, slot);

            //Load our script to the device
            status = Canlib.kvScriptLoadFile(chanhandle, slot, scriptfile);
            CheckStatus((Canlib.canStatus)chanhandle, "Loading script");

            //Start the script
            status = Canlib.kvScriptStart(chanhandle, slot);
            CheckStatus((Canlib.canStatus)chanhandle, "Starting script");

            Thread.Sleep(500);

            //Stop the script
            status = Canlib.kvScriptStop(chanhandle, slot, Canlib.kvSCRIPT_STOP_NORMAL);
            CheckStatus((Canlib.canStatus)chanhandle, "Stopping script");

            ////Unload the script
            //status = Canlib.kvScriptUnload(chanhandle, slot);
            //CheckStatus(status, "Unloading script");

            //Closing the channel
            status = Canlib.canBusOff(chanhandle);
            CheckStatus(status, "Bus off");
            status = Canlib.canClose(chanhandle);
            CheckStatus(status, "Closing channel");
            Canlib.canUnloadLibrary();
            Console.WriteLine("Press any key to exit ..");
            Console.ReadKey();
        }

        static void CheckStatus(Canlib.canStatus status, string operation)
        {
            if (status < 0)
            {
                string errorMessage;
                Canlib.canGetErrorText(status, out errorMessage);
                Console.WriteLine("{0} failed: {1}", operation, errorMessage);
            }
        }
    }
}
