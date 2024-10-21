/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kvaser.CanLib;
using Kvaser.Kvadblib;

namespace DBApp
{
    class Program
    {
        static string filename = "engine_example.dbc";
        static int channel = 0;

        static int chanhandle;
        static Kvadblib.Hnd dbhandle;



        static void Main(string[] args)
        {
            Canlib.canStatus status;
            Kvadblib.Status dbstatus;

            if (args.Length > 0)
            {
                filename = args[0];
            }
            if (args.Length > 1)
            {
                channel = Int32.Parse(args[1]);
            }

            //Initialize, open channel and go on bus
            Canlib.canInitializeLibrary();

            chanhandle = Canlib.canOpenChannel(channel, Canlib.canOPEN_ACCEPT_VIRTUAL);
            DisplayError((Canlib.canStatus)chanhandle, "canOpenChannel");

            status = Canlib.canSetBusParams(chanhandle, Canlib.canBITRATE_250K, 0, 0, 0, 0, 0);
            DisplayError(status, "canSetBusParams");

            status = Canlib.canBusOn(chanhandle);
            DisplayError(status, "canBusOn");


            //Load database
            dbhandle = new Kvadblib.Hnd();
            dbstatus = Kvadblib.Open(out dbhandle);
            DisplayDBError(dbstatus, "Opening database handle ");
            dbstatus = Kvadblib.ReadFile(dbhandle, filename);
            DisplayDBError(dbstatus, "Reading database from file ");

            //Start dumping messages
            if(dbstatus == Kvadblib.Status.OK && status == Canlib.canStatus.canOK)
            {
                DumpMessageLoop();
            }


            //Go off bus and close channel
            status = Canlib.canBusOff(chanhandle);
            DisplayError(status, "canBusOff");

            status = Canlib.canClose(chanhandle);
            DisplayError(status, "canClose");


            //Wait for the user to press a key before exiting, in case the console closes automatically on exit.
            Console.WriteLine("Channel closed. Press any key to exit");
            Console.ReadKey();
        }


        private static void DumpMessageLoop()
        {
            Canlib.canStatus status;
            bool finished = false;

            //These variables hold the incoming message
            byte[] data = new byte[8];
            int id;
            int dlc;
            int flags;
            long time;

            Console.WriteLine("Channel opened. Press Escape to close. ");

            while (!finished)
            {
                //Wait for 100 ms for a message on the channel
                status = Canlib.canReadWait(chanhandle, out id, data, out dlc, out flags, out time, 100);

                //Loop until all messages from the past 100 ms have been displayed, or an error occurs
                if (status == Canlib.canStatus.canOK)
                {
                    if ((flags & Canlib.canMSG_ERROR_FRAME) != 0)
                    {
                        Console.Write("***Error Frame received***");
                    }
                    else
                    {
                        DumpMessage(id, data, dlc, flags, time);
                    }
                }

                //Call DisplayError and exit in case an actual error occurs
                else if (status != Canlib.canStatus.canERR_NOMSG)
                {
                    DisplayError(status, "canRead/canReadWait");
                    finished = true;
                }

                //Breaks the loop if the user presses the Escape key
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    finished = true;
                }
            }
        }



        private static void DumpMessage(int id, byte[] data, int dlc, int flags, long time)
        {
            Kvadblib.Status status;
            Kvadblib.MessageHnd mh = new Kvadblib.MessageHnd();
            Kvadblib.SignalHnd sh = new Kvadblib.SignalHnd();
            
            
            //Flips the EXT bit if the EXT flag is set
            if ((flags & Canlib.canMSG_EXT) != 0)
            {
                id ^= -2147483648;
            }
            
            //Find the database message whose id matches the one
            //from the incoming message
            status = Kvadblib.GetMsgById(dbhandle, id, out mh);
            Console.WriteLine("Reading message with id " + id);
            DisplayDBError(status, "Reading message with id " + id);

            //Print the message info
            if (status == Kvadblib.Status.OK)
            {
                string msgName;
                status = Kvadblib.GetMsgName(mh, out msgName);

                Console.WriteLine("Message received: {0}", msgName);
                int msgId;
                Kvadblib.MESSAGE msgFlags;
                status = Kvadblib.GetMsgId(mh, out msgId, out msgFlags);

                Console.WriteLine("Id: {0}, flags: {1}", msgId, msgFlags);

                //Iterate through all the signals and print their name, value and unit
                status = Kvadblib.GetFirstSignal(mh, out sh);
                while (status == Kvadblib.Status.OK)
                {
                    string signalname;
                    status = Kvadblib.GetSignalName(sh, out signalname);

                    string unit;
                    status = Kvadblib.GetSignalUnit(sh, out unit);

                    double value;
                    status = Kvadblib.GetSignalValueFloat(sh, out value, data, dlc);

                    Console.WriteLine("Signal - {0}: {1} {2}", signalname, value, unit);

                    status = Kvadblib.GetNextSignal(mh, out sh);
                }
            }
        }


        //Displays error messages when a call to Canlib fails
        private static void DisplayError(Canlib.canStatus status, string method)
        {
            if (status < 0)
            {
                Console.WriteLine(method + " failed: " + status.ToString());
            }
        }

        //Displays messages for Kvadblib calls
        private static void DisplayDBError(Kvadblib.Status status, string method)
        {
            if (status < 0)
            {
                Console.WriteLine(method + " failed: " + status.ToString());
            }
        }



    }
}
