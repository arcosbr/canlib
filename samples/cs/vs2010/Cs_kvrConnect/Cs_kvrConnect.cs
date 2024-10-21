/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html

 * This examples shows how to find a device on your network and stop/start the helper service.
 * You need to run as administrator in order to start/stop service.
 * 
 * Dependences: 
 *      canlib32.dll 
 *      Kvrlib.dll
 *      irisdll.dll 
 *      irisflash.dll 
 *      libxml2.dll 
 *      iconv.dll 
 *      zlib1.dll 
 */

using System;
using System.Collections.Generic;
using System.Text;

using Kvaser.Kvrlib;
using Kvaser.CanLib;

namespace Cs_kvrConnect
{
    class kvrConnect
    {

        static string password = "";

        /*
         * Returns true if we can find the device on one of our channels,
         * goes through all the channels and compares their EAN and serial
         * to the one in the DeviceInfo
         */
        static bool IsUsedByMe(Kvrlib.DeviceInfo di)
        {
            int channel_count;
            UInt64 ean;
            Canlib.canStatus status;
            uint ean_hi = 0;
            uint ean_lo = 0;
            uint serial = 0;

            Canlib.canInitializeLibrary();

            status = Canlib.canGetNumberOfChannels(out channel_count);

            if (!status.Equals(Canlib.canStatus.canOK))
            {
                Console.WriteLine("ERROR: canGetNumberOfChannels failed " + status);
                return false;
            }

            for (int i = 0; (status.Equals(Canlib.canStatus.canOK)) && (i < channel_count); i++)
            {
                object ean0;
                status = Canlib.canGetChannelData(i, Canlib.canCHANNELDATA_CARD_UPC_NO, out ean0);
                if (!status.Equals(Canlib.canStatus.canOK))
                {
                    Console.WriteLine("ERROR: canCHANNELDATA_CARD_UPC_NO failed: " + status);
                    return false;
                }
                ean = UInt64.Parse(ean0.ToString());

                //Extract the high and low parts of the EAN
                ean_hi = (UInt32)((ean >> 32) & 0xffffffff);
                ean_lo = (UInt32)((ean) & 0xffffffff);

                object serial0;
                status = Canlib.canGetChannelData(i, Canlib.canCHANNELDATA_CARD_SERIAL_NO, out serial0);
                serial = Convert.ToUInt32(serial0);

                if (!status.Equals(Canlib.canStatus.canOK))
                {
                    Console.WriteLine("ERROR: canCHANNELDATA_CARD_SERIAL_NO failed: " + status);
                    return false;
                }
                                
                if ((di.ean_hi.Equals(ean_hi)) && (di.ean_lo.Equals(ean_lo)) && (di.ser_no.Equals(serial)))
                {
                    return true;
                }
            }
            return false;
        }


        /*
         * Prints information about a device
         */
        static void DumpDeviceInfo(Kvrlib.DeviceInfo di)
        {
            string service_text = "";
            string buf = "";
            Kvrlib.Status status;
            string addr_buf = "";
            Kvrlib.ServiceState service_state = 0;
            Kvrlib.StartInfo service_sub_state = 0;

            Console.WriteLine("--------------------------------------------------------------------------");
            Console.WriteLine("Device information");
            Console.WriteLine("EAN:         {0:X}{1:X}", di.ean_hi, di.ean_lo);
            Console.WriteLine("FW version:  " + di.fw_major_ver + "." + di.fw_minor_ver + "." + di.fw_build_ver);
            Console.WriteLine("Serial:      " + di.ser_no);
            Console.WriteLine("Name:        " + di.name);
            Console.WriteLine("Host name:   " + di.host_name);

            string passwdOutput = "";
            if (di.accessibility_pwd.Length > 0)
            {
                foreach (char c in di.accessibility_pwd)
                {
                    passwdOutput += "*";
                }
            }
            else
            {
                passwdOutput = "None!";
            }

            Console.WriteLine("Password:    " + passwdOutput);

            Kvrlib.StringFromAddress(out addr_buf, di.device_address);
            Console.WriteLine("IP:          " + addr_buf);
            Kvrlib.StringFromAddress(out addr_buf, di.client_address);
            Console.WriteLine("Client IP:   " + addr_buf);
            Console.WriteLine("Usage:       " + di.usage);
            if ((!di.usage.Equals(Kvrlib.DeviceUsage.FREE)) && IsUsedByMe(di))
            {
                Console.Write(" - Used by Me!\n");
            }
            else if ((!di.usage.Equals(Kvrlib.DeviceUsage.FREE)) && (!di.usage.Equals(Kvrlib.DeviceUsage.UNKNOWN)))
            {
                Console.Write(" - Used by other!\n");
            }
            else
            {
                Console.Write("\n");
            }
            Console.WriteLine("Alive:       " + ((di.availability & Kvrlib.Availability.FOUND_BY_SCAN) > 0 ? "Yes" : "No"));
            Console.WriteLine("Stored:      " + ((di.availability & Kvrlib.Availability.STORED) > 0 ? "Yes" : "No"));

            // Ask service for status service_text
            status = Kvrlib.DeviceGetServiceStatusText(di, out service_text);

            if (!status.Equals(Kvrlib.Status.OK))
            {
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("Service:     FAILED - " + buf);
            }
            else if (service_text.StartsWith("Service: "))
            {
                Console.WriteLine("Service:     " + service_text.Substring(9, service_text.Length - 9));
            }
            else
            {
                Console.WriteLine(service_text);
            }

            status = Kvrlib.DeviceGetServiceStatus(di, out service_state, out service_sub_state);
            if (status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("service_state: " + service_state + "." + service_sub_state);
            }
            else
            {
                Console.WriteLine("service_state: unknown");
            }
        }

        /*
         * Finds all devices and prints their info
         */
        static Kvrlib.Status DiscoverDevices(Kvrlib.DiscoveryHnd discoveryHandle)
        {
            Kvrlib.Status status;
            Kvrlib.DeviceInfo[] device_info = new Kvrlib.DeviceInfo[64];
            int delay_ms = 500;
            int timeout_ms = 300;

            status = Kvrlib.DiscoveryStart(discoveryHandle, delay_ms, timeout_ms);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                string buf;
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("kvrDiscoveryStart() FAILED - " + buf);
                return status;
            }

            int no_devices = 0;
            while (status.Equals(Kvrlib.Status.OK) && no_devices < device_info.Length)
            {
                status = Kvrlib.DiscoveryGetResults(discoveryHandle, out device_info[no_devices]);
                if (status.Equals(Kvrlib.Status.OK))
                {

                    DumpDeviceInfo(device_info[no_devices]);

                    //Tries to set password and encryption key (uncomment to test)


                    //if (!Kvrlib.DiscoverySetPassword(device_info[no_devices], password).Equals(Kvrlib.Status.OK))
                    //{
                    //    Console.WriteLine("Unable to set password: {0:s} ({1:d})", password, password.Length);
                    //}

                    //if (!Kvrlib.DiscoverySetEncryptionKey(device_info[no_devices], "testkey").Equals(Kvrlib.Status.OK))
                    //{
                    //    Console.WriteLine("Unable to set Ecryption key");
                    //}

                    //if (!Kvrlib.DiscoverySetEncryptionKey(device_info[no_devices], "").Equals(Kvrlib.Status.OK))
                    //{
                    //    Console.WriteLine("Unable to set Ecryption key");
                    //}

                    no_devices++;
                }
                else
                {
                    if (!status.Equals(Kvrlib.Status.BLANK))
                    {
                        Console.WriteLine("kvrDiscoveryGetResults() failed " + status);
                    }
                }
            }

            Console.WriteLine("{0} devices found", no_devices);

            status = Kvrlib.DiscoveryStoreDevices(device_info);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                string buf;
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("Device store failed: " + buf);
                return status;
            }
            return status;
        }

        /*
         * Sets up the addresses for discovery
         */
        static Kvrlib.Status SetupBroadcast(Kvrlib.DiscoveryHnd discoveryHandle)
        {
            string buf = "";
            Kvrlib.Status status;
            Kvrlib.Address[] addr_list = new Kvrlib.Address[20];
            int no_addrs = 0;

            status = Kvrlib.DiscoveryGetDefaultAddresses(out addr_list, Kvrlib.AddressTypeFlag.ALL);

            if (!status.Equals(Kvrlib.Status.OK))
            {
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("kvrDiscoveryGetDefaultAddresses() FAILED - " + buf);
                return status;
            }

            //Add an address to the array (strictly not necessary, just for demonstration purposes)
            Array.Resize<Kvrlib.Address>(ref addr_list, addr_list.Length + 1);
            String tmp_addr = "192.168.3.66";
            status = Kvrlib.AddressFromString(Kvrlib.AddressType.IPV4, out addr_list[addr_list.Length - 1], tmp_addr);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("ERROR: kvrAddressFromString(" + no_addrs + ", " + tmp_addr + ") failed");
                return status;
            }

            for (int i = 0; i < addr_list.Length; i++)
            {
                status = Kvrlib.StringFromAddress(out buf, addr_list[i]);
                Console.WriteLine("Looking for device using: " + buf);
            }

            status = Kvrlib.DiscoverySetAddresses(discoveryHandle, addr_list);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("DiscoverySetAddresses() FAILED - " + buf);
                return status;
            }
            return status;
        }

        /*
         * Gets and sets the helper service status
         */
        static Kvrlib.Status TestService()
        {
            Kvrlib.Status status = new Kvrlib.Status();
            string buf = "";
            int serviceStatus;

            status = Kvrlib.ServiceQuery(out serviceStatus);
            Console.WriteLine("ServiceQuery() service status - " + serviceStatus);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("ServiceQuery() FAILED - " + buf);
                return status;
            }
            System.Threading.Thread.Sleep(2000);
            status = Kvrlib.ServiceStop(out serviceStatus);
            Console.WriteLine("ServiceStop() service status - " + serviceStatus);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("ServiceStop() FAILED - " + buf);
                return status;
            }
            System.Threading.Thread.Sleep(2000);
            status = Kvrlib.ServiceStart(out serviceStatus);
            Console.WriteLine("ServiceStart() service status - " + serviceStatus);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("ServiceStart() FAILED - " + buf);
                return status;
            }
            System.Threading.Thread.Sleep(2000);
            status = Kvrlib.ServiceQuery(out serviceStatus);
            Console.WriteLine("ServiceQuery() service status - " + serviceStatus);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("ServiceQuery() FAILED - " + buf);
                return status;
            }
            return status;
        }


        static int Main(string[] args)
        {

            Kvrlib.Status status;
            Kvrlib.DiscoveryHnd discoveryHandle;
            String buf = "";

            if (args.Length > 0)
            {
                password = args[0];
            }

            Kvrlib.InitializeLibrary();

            status = Kvrlib.DiscoveryOpen(out discoveryHandle);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("DiscoveryOpen() FAILED - " + buf);
                return -1;
            }

            status = SetupBroadcast(discoveryHandle);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("SetupBroadcast() FAILED - " + buf);
                return -2;
            }

            status = DiscoverDevices(discoveryHandle);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("DiscoverDevices() FAILED - " + buf);
                return -3;
            }

            status = Kvrlib.DiscoveryClearDevicesAtExit(true);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("DiscoveryClearDevicesAtExit() FAILED - " + buf);
                return -4;
            }
            status = Kvrlib.DiscoveryClearDevicesAtExit(false);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("DiscoveryClearDevicesAtExit() FAILED - " + buf);
                return -5;
            }

            status = Kvrlib.DiscoveryClose(discoveryHandle);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("DiscoveryClose() FAILED - " + buf);
                return -6;
            }

            status = TestService();
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Kvrlib.GetErrorText(status, out buf);
                Console.WriteLine("TestService() FAILED - " + buf);
                return -7;
            }
            Kvrlib.UnloadLibrary();
            return 0;
        }
    }
}
