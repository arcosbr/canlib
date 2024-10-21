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

namespace _3_FileIO_T_net481
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //A channel to the device
            int channel = 0;
            //The slot on the device to which we want to load the script
            int slot = 0;
            //The name of the file we write incoming messages to
            string outputfile = @"output.txt";
            //The name of the file where we have stored the messages to send        
            string inputfile = @"input.txt";

            string sourcefile = @"..\..\..\source.txt";
            sourcefile = System.IO.Path.GetFullPath(sourcefile);

            //The compiled t script file
            string scriptfile = @"..\..\..\read_write.txe";
            Console.WriteLine($"scriptfile exists: {System.IO.File.Exists(scriptfile)}");


            //How long we should wait for program output in the loop
            int timeout = 100;
            int wait = 10000;

            Canlib.canInitializeLibrary();

            int handle = Canlib.canOpenChannel(channel, 0);

            //If there is a file with the same name as our output file on the device, delete it
            CheckStatus(Canlib.kvFileDelete(handle, outputfile), "removing output file");

            CheckStatus(Canlib.kvFileCopyToDevice(handle, sourcefile, inputfile), "uploading input file");

            CheckStatus(Canlib.kvScriptLoadFile(handle, slot, scriptfile), "loading script file");

            CheckStatus(Canlib.kvScriptRequestText(handle, slot, Canlib.kvSCRIPT_REQUEST_TEXT_SUBSCRIBE), "Requesting text");

            CheckStatus(Canlib.kvScriptStart(handle, slot), "starting script");

            Console.WriteLine("Script started");
            for (int i = 0; i < wait; i += timeout)
            {
                int s, f;
                long time;
                string output;
                Canlib.canStatus status = Canlib.kvScriptGetText(handle, out s, out time, out f, out output);
                while (status == Canlib.canStatus.canOK)
                {
                    Console.WriteLine("Output: " + output);
                    status = Canlib.kvScriptGetText(handle, out s, out time, out f, out output);
                }
                if (status != Canlib.canStatus.canERR_NOMSG)
                {
                    CheckStatus(status, "Reading text");
                }
                Thread.Sleep(timeout);
            }

            CheckStatus(Canlib.kvScriptStop(handle, slot, Canlib.kvSCRIPT_STOP_NORMAL), "Stopping script");

            CheckStatus(Canlib.kvFileCopyFromDevice(handle, outputfile, outputfile), "Fetching output file");

            CheckStatus(Canlib.kvScriptUnload(handle, slot), "unload script");

            CheckStatus(Canlib.canClose(handle), "closing channel");

            Canlib.canUnloadLibrary();

            Console.WriteLine("Script stopped");

            string outputText = System.IO.File.ReadAllText(outputfile);

            Console.WriteLine("Fetched output file from device:\n {0}", outputText);
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
        }

        private static void CheckStatus(Canlib.canStatus status, string method)
        {
            if (status < 0)
            {
                Console.WriteLine(method + " failed: " + status.ToString());
            }
        }
    }
}
