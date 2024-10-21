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
using Kvaser.Kvrlib;

namespace RemoteConnect
{
    class Program
    {
        static string ipAddress = "0.0.0.0";

        static void Main(string[] args)
        {
            if(args.Length > 0)
            {
                ipAddress = args[0];
            }

            Kvrlib.Status status;
            Kvrlib.DiscoveryHnd discoveryHandle;
            int deviceIndex = 0;
            Canlib.canStatus canStat;
            
            //Initialization
            Kvrlib.InitializeLibrary();
            status = Kvrlib.DiscoveryOpen(out discoveryHandle);
            KvrCheck(status, "open discovery");

            //Setting a discovery list with the device's address
            Kvrlib.Address[] addr_list = new Kvrlib.Address[1];
            status = Kvrlib.AddressFromString(Kvrlib.AddressType.IPV4, out addr_list[0], ipAddress);
            KvrCheck(status, "address");
            status = Kvrlib.DiscoverySetAddresses(discoveryHandle, addr_list);
            KvrCheck(status, "setaddress");

            //Discovering the device
            Kvrlib.DeviceInfo[] device_info = new Kvrlib.DeviceInfo[64];
            int delay_ms = 500;
            int timeout_ms = 300;
            status = Kvrlib.DiscoveryStart(discoveryHandle, delay_ms, timeout_ms);
            KvrCheck(status, "start discovery");

            int i = 0;
            while (status == Kvrlib.Status.OK && i < device_info.Length)
            {
                status = Kvrlib.DiscoveryGetResults(discoveryHandle, out device_info[i]);
                if (status == Kvrlib.Status.OK)
                {
                    Kvrlib.Address addr = device_info[i].device_address;
                    string foundAddr = "";
                    Kvrlib.StringFromAddress(out foundAddr, addr);
                    if (foundAddr.Equals(ipAddress))
                    {
                        Console.WriteLine("Found device");
                        deviceIndex = i;
                    }
                    i++;
                }
            }

            //This connects us to the device
            device_info[deviceIndex].request_connection = 1;
            status = Kvrlib.DiscoveryStoreDevices(device_info);

            Console.WriteLine("Connecting to {0}", device_info[deviceIndex].name);

            //Poll until the device has been found on a CAN channel
            int channel = -1;
            while (channel < 0)
            {
                channel = FindChannel(device_info[deviceIndex]);
                if (channel < 0)
                    Thread.Sleep(500);
            }

            //Open and go on bus
            Console.WriteLine("Opening channel {0}", channel);
            int handle = Canlib.canOpenChannel(channel, 0);

            canStat = Canlib.canSetBusParams(handle, Canlib.canBITRATE_250K, 0, 0, 0, 0, 0);
            canStat = Canlib.canBusOn(handle);

            //Print incoming messages
            DumpMessageLoop(handle);

            //Close and go off bus
            Console.WriteLine("Closing...");
            canStat = Canlib.canBusOff(handle);
            canStat = Canlib.canClose(handle);

            //This will disconnect the device
            device_info[deviceIndex].request_connection = 0;
            status = Kvrlib.DiscoveryStoreDevices(device_info);
            
            Kvrlib.DiscoveryClose(discoveryHandle);
            Kvrlib.UnloadLibrary();
            Canlib.canUnloadLibrary();
        }

        //Goes through all the CAN channels, returns the channel number of the device
        static int FindChannel(Kvrlib.DeviceInfo device)
        {
            Canlib.canInitializeLibrary();
            Canlib.canStatus status;
            int channelCount;
            Canlib.canGetNumberOfChannels(out channelCount);
            for (int i = 0; i < channelCount; i++)
            {
                object ean, serial;
                UInt64 ean64;
                uint ean_hi = 0;
                uint ean_lo = 0;
                uint serialno = 0;
                status = Canlib.canGetChannelData(i, Canlib.canCHANNELDATA_CARD_UPC_NO, out ean);
                status = Canlib.canGetChannelData(i, Canlib.canCHANNELDATA_CARD_SERIAL_NO, out serial);
                ean64 = UInt64.Parse(ean.ToString());
                ean_hi = (UInt32)((ean64 >> 32) & 0xffffffff);
                ean_lo = (UInt32)((ean64) & 0xffffffff);
                serialno = Convert.ToUInt32(serial);

                if (device.ean_hi == ean_hi && device.ean_lo == ean_lo && device.ser_no == serialno)
                {
                   return i;
                }
            }
            Canlib.canUnloadLibrary();
            return -1;
        }

        private static void DumpMessageLoop(int handle)
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
            Console.WriteLine("ID  DLC  DATA                       Timestamp");

            while (!finished)
            {
                //Wait for 100 ms for a message on the channel
                status = Canlib.canReadWait(handle, out id, data, out dlc, out flags, out time, 100);

                //Display message
                if (status == Canlib.canStatus.canOK)
                {
                        DumpMessage(id, data, dlc, flags, time);
                    
                }

                //Call DisplayError and exit in case an actual error occurs
                if (status != Canlib.canStatus.canERR_NOMSG)
                {
                    CanCheck(status, "canRead/canReadWait");
                }

                //Breaks the loop if the user presses the Escape key
                if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
                {
                    finished = true;
                }
            }
        }

        //Prints an incoming message to the screen
        private static void DumpMessage(int id, byte[] data, int dlc, int flags, long time)
        {
            if ((flags & Canlib.canMSG_ERROR_FRAME) != 0)
            {
                Console.WriteLine("Error Frame received ****");
            }
            else
            {
                Console.WriteLine("{0:03}  {1}    {2:x2} {3:x2} {4:x2} {5:x2} {6:x2} {7:x2} {8:x2} {9:x2}    {10}",
                                                 id, dlc, data[0], data[1], data[2], data[3], data[4],
                                                 data[5], data[6], data[7], time);
            }
        }

        static void CanCheck(Canlib.canStatus status, string action){
            if (status < 0)
            {
                Console.WriteLine(action + " failed: " + status.ToString());
            }
        }

        static void KvrCheck(Kvrlib.Status status, string action)
        {
            if (status != Kvrlib.Status.OK)
            {
                string err;
                Kvrlib.GetErrorText(status, out err);
                Console.WriteLine(action + " failed: " + err);
            }
        }
    }
}
