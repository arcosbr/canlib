/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html

 * This examples shows how to read and write configurations to a device.
 * It also uses some other functions found in the Kvrlib.
 * 
 * Dependences: 
 *      canlib32.dll 
 *      Kvrlib.dll
 *      irisdll.dll 
 *      irisflash.dll 
 *      libxml2.dll 
 *      zlib1.dll 
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;

using Kvaser.Kvrlib;
using Kvaser.CanLib;

namespace Cs_kvrConfig
{
    class kvrConfig
    {

        /*
         * Lists the devices on every channel
         */
        static void ListDevices()
        {
            Canlib.canStatus status = new Canlib.canStatus();
            int channel_count = 0;

            Canlib.canGetNumberOfChannels(out channel_count);

            Console.WriteLine("First argument must be a channel!\n");
            Console.WriteLine("Channel\t Name");
            Console.WriteLine("--------------------------------------------------------");
            for (int i = 0; (status == Canlib.canStatus.canOK) && (i < channel_count); i++)
            {
                object chData;
                status = Canlib.canGetChannelData(i, Canlib.canCHANNELDATA_CHANNEL_NAME, out chData);
                Console.WriteLine(chData.ToString());
            }
        }

        /*
         * Waits until a device with the specified ean and serial numbers appears on some channel
         * or a timeout occurs
         */
        static int WaitForDevice(object ean, object serial, long timeout_in_ms)
        {
            long time_start;
            Canlib.canStatus stat;

            Console.WriteLine("\nWaiting for device with EAN {0:X}, and serial " + Convert.ToUInt64(serial), Convert.ToUInt64(ean));

            time_start = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            System.Threading.Thread.Sleep(3000);

            do
            {
                int channel_count;
                object tmp_serial;
                object tmp_ean;

                int channel;

                stat = Canlib.canGetNumberOfChannels(out channel_count);
                if (!stat.Equals(Canlib.canStatus.canOK))
                {
                    Console.WriteLine("canGetNumberOfChannels() failed.");
                    return -1;
                }

                for (channel = 0; channel < channel_count; channel++)
                {
                    stat = Canlib.canGetChannelData(channel, Canlib.canCHANNELDATA_CARD_UPC_NO, out tmp_ean);
                    if (!stat.Equals(Canlib.canStatus.canOK))
                    {
                        Console.WriteLine("canGetChannelData(canCHANNELDATA_CARD_UPC_NO) failed.");
                        return -2;
                    }
                    stat = Canlib.canGetChannelData(channel, Canlib.canCHANNELDATA_CARD_SERIAL_NO, out tmp_serial);
                    if (!stat.Equals(Canlib.canStatus.canOK))
                    {
                        Console.WriteLine("canGetChannelData(canCHANNELDATA_CARD_SERIAL_NO) failed.");
                        return -3;
                    }

                    if ((ean.Equals(tmp_ean)) && (serial.Equals(tmp_serial)))
                    {
                        Console.WriteLine("Found!\n");
                        return 0;
                    }
                }
                Console.WriteLine("Try again...");
                System.Threading.Thread.Sleep(500);
            }
            while ((DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond) < (time_start + timeout_in_ms));

            Console.WriteLine("Device did not appear within given timeout\n");
            return -4;
        }


        /*
         * Returns true if the supplied channel number can be opened for reading using an empty password
         */
        static Boolean IsPasswordFree(int channelNumber)
        {
            Kvrlib.Status status;
            Kvrlib.ConfigHnd handle;

            status = Kvrlib.ConfigOpen(channelNumber, Kvrlib.ConfigMode.R, "", out handle);

            if (status != Kvrlib.Status.OK)
            {
                Console.WriteLine("Failed to open configuration with empty password on channel " + channelNumber);
                return false;
            }
            else
            {
                Kvrlib.ConfigClose(handle);
                return true;
            }
        }

        /*
         * Returns true if we can open the channel and all other channels on the same device exclusively,
         * the device is connected locally and it can either be opened with the empty or the supplied password
         *
         */
        static Boolean IsAvailableForConfig(int canlib_channel, String password)
        {
            int can_hnd;
            Canlib.canStatus stat;
            Kvrlib.Status kvrstat;
            int bus_type;
            object serial;
            object tmp_serial;
            object ean;
            object tmp_ean;
            object chan_no;
            int tmp_chan;
            Kvrlib.ConfigHnd cfg_hnd;

            can_hnd = Canlib.canOpenChannel(canlib_channel, Canlib.canOPEN_EXCLUSIVE);

            if (can_hnd < 0)
            {
                Console.WriteLine("Channel " + canlib_channel + " could not be opened exclusively");
                return false;
            }

            stat = Canlib.canIoCtl(can_hnd, Canlib.canIOCTL_GET_BUS_TYPE, out bus_type);
            if (!stat.Equals(Canlib.canStatus.canOK))
            {
                Console.WriteLine("ERROR: failed to get bustype " + stat);
                return false;
            }

            if (bus_type != Canlib.kvBUSTYPE_GROUP_LOCAL)
            {
                Console.WriteLine("Channel in not local (bus type:" + stat + ").");
                return false;
            }

            Canlib.canClose(can_hnd);

            // 
            // check all channels on a given device
            // 

            stat = Canlib.canGetChannelData(canlib_channel, Canlib.canCHANNELDATA_CARD_UPC_NO, out ean);
            stat = Canlib.canGetChannelData(canlib_channel, Canlib.canCHANNELDATA_CARD_SERIAL_NO, out serial);
            stat = Canlib.canGetChannelData(canlib_channel, Canlib.canCHANNELDATA_CHAN_NO_ON_CARD, out chan_no);

            tmp_chan = canlib_channel - Convert.ToInt32(chan_no);

            while (true)
            {
                can_hnd = Canlib.canOpenChannel(tmp_chan, Canlib.canOPEN_EXCLUSIVE);
                if (can_hnd < 0)
                {
                    Console.WriteLine("Channel " + tmp_chan + "(same device as channel " + canlib_channel + "can not be opened exclusively.");
                    return false;
                }
                Canlib.canClose(can_hnd);

                tmp_chan++;

                stat = Canlib.canGetChannelData(tmp_chan, Canlib.canCHANNELDATA_CARD_UPC_NO, out tmp_ean);
                stat = Canlib.canGetChannelData(tmp_chan, Canlib.canCHANNELDATA_CARD_SERIAL_NO, out tmp_serial);

                if ((Convert.ToUInt64(tmp_ean) != Convert.ToUInt64(ean)) || (Convert.ToUInt64(tmp_serial) != Convert.ToUInt64(serial)))
                {
                    break;
                }
            }

            Canlib.canClose(can_hnd);
            if (IsPasswordFree(canlib_channel))
            {
                Console.WriteLine("No password is needed for configuring channel ", canlib_channel);
                kvrstat = Kvrlib.ConfigOpen(canlib_channel, Kvrlib.ConfigMode.RW, "", out cfg_hnd);
                if (!kvrstat.Equals(Kvrlib.Status.OK))
                {
                    Console.WriteLine("Failed to open configuration with empty password on channel " + canlib_channel);
                    return false;
                }
                else
                {
                    Kvrlib.ConfigClose(cfg_hnd);
                    return (0 == WaitForDevice(ean, serial, 10000));
                }
            }
            else
            {
                Console.WriteLine("Password is needed for configuring channel " + canlib_channel);
                // This test will remove the device from Kvaser Hardware
                kvrstat = Kvrlib.ConfigOpen(canlib_channel, Kvrlib.ConfigMode.RW, password, out cfg_hnd);
                if (!kvrstat.Equals(Kvrlib.Status.OK))
                {
                    Console.WriteLine("Failed to open configuration with supplied password " + password + " on channel " + canlib_channel);
                    return false;
                }
                else
                {
                    Kvrlib.ConfigClose(cfg_hnd);
                    return (0 == WaitForDevice(ean, serial, 10000));
                }
            }
        }

        /*
         * Scans for available networks
         */
        static Kvrlib.Status ScanNetworks(Kvrlib.ConfigHnd handle)
        {
            Kvrlib.Status status;
            Kvrlib.Status stat;

            int active = 0;   //is a passive scan
            Kvrlib.Bss bss_type = Kvrlib.Bss.ANY;  //infrastruction and adhoc
            Kvrlib.RegulatoryDomain domain = Kvrlib.RegulatoryDomain.WORLD;

            string ssid = "";
            string securityString = "";

            int rssi = 0;
            int channel = 0;
            Kvrlib.Address mac;
            int capability;
            int type_wpa;
            Kvrlib.CipherInfoElement wpa_info = new Kvrlib.CipherInfoElement();
            Kvrlib.CipherInfoElement rsn_info = new Kvrlib.CipherInfoElement();

            status = Kvrlib.WlanStartScan(handle, active, bss_type, domain);

            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could not start scan " + Convert.ToInt32(status));
                return status;
            }

            do
            {
                status = Kvrlib.WlanGetScanResults(handle, ref rssi, ref channel, out mac, out bss_type, out ssid,
                                                   out capability, out type_wpa, out wpa_info, out rsn_info);

                if (status.Equals(Kvrlib.Status.OK))
                {
                    String tmp_address;
                    Console.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
                    Console.WriteLine("SSID: " + ssid);
                    Console.WriteLine("RSSI: " + rssi + " dBm");
                    Console.WriteLine("WLAN Channel: " + channel);
                    status = Kvrlib.StringFromAddress(out tmp_address, mac);
                    if (!status.Equals(Kvrlib.Status.OK))
                    {
                        Console.WriteLine("Could convert string from address (" + status + ")");
                        return status;
                    }
                    Console.WriteLine("MAC address: " + tmp_address);
                    Console.WriteLine("BSS type: " + bss_type);
                    Console.WriteLine("Capabilities: {0:X}", capability);

                    stat = Kvrlib.WlanGetSecurityText(out securityString, capability, type_wpa, wpa_info, rsn_info);
                    Console.Write("Security");
                    if (stat.Equals(Kvrlib.Status.PARAMETER))
                    {
                        Console.Write("(Truncated)");
                    }
                    Console.WriteLine(": " + securityString);
                }
            }
            while (status.Equals(Kvrlib.Status.OK) || status.Equals(Kvrlib.Status.NO_ANSWER));

            // Kvrlib.Status.BLANK => no more networks => OK
            return (status.Equals(Kvrlib.Status.BLANK) ? Kvrlib.Status.OK : status);
        }

        /*
         * Deletes the currently active configuration
         */
        static Kvrlib.Status EraseConfiguration(int canlib_channel, string password)
        {
            Kvrlib.Status status;
            Kvrlib.ConfigHnd handle = new Kvrlib.ConfigHnd();

            status = Kvrlib.ConfigOpen(canlib_channel, Kvrlib.ConfigMode.ERASE, password, out handle);

            status = Kvrlib.ConfigClear(handle);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could not clear configuration (" + status + ")");
                Kvrlib.ConfigClose(handle);
                return status;
            }

            Kvrlib.ConfigClose(handle);
            return 0;
        }

        /*
         * Reads the currently active configuration, then writes it to the device
         */
        static Kvrlib.Status DoConfigure(Kvrlib.ConfigHnd handle)
        {
            Kvrlib.Status status;
            string new_xml_config = "";
            string old_xml_config = "";
            string xml_error = "";

            status = Kvrlib.ConfigGet(handle, out old_xml_config);

            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could not read configuration from device (" + status + ")");
                Kvrlib.ConfigClose(handle);
                return status;
            }

            Console.WriteLine("Old configuration: " + old_xml_config);

            new_xml_config = old_xml_config;
            // Check that the new configuration is valid
            status = Kvrlib.ConfigVerifyXml(new_xml_config, out xml_error);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("The XML configuration is not valid (" + status + "):\n" + xml_error);
                Kvrlib.ConfigClose(handle);
                return status;
            }

            // Download new configuration
            status = Kvrlib.ConfigSet(handle, old_xml_config);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could not write configuration to device (" + status + ")");
                Kvrlib.ConfigClose(handle);
                return status;
            }
            return status;
        }

        /*
         * Runs network tests with the configuration
         */
        static Kvrlib.Status TestConfiguration(Kvrlib.ConfigHnd handle,
                                            int seconds,
                                            object ean,
                                            object serial)
        {
            Kvrlib.Status status;
            Kvrlib.Address address;
            Kvrlib.Address mac;
            Kvrlib.Address netmask;
            Kvrlib.Address gateway;
            string tmp_address = "";
            Boolean dhcp;

            Kvrlib.NetworkState state;
            int tx_rate;
            int rx_rate;
            int channel;
            int rssi_mean;
            int tx_power;
            uint ean_hi;
            uint ean_lo;

            int[] rssi;
            int[] rtt;

            status = Kvrlib.NetworkConnectionTest(handle, 1);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could not start ping(" + status + ")");
                return status;
            }

            while (seconds > 0)
            {
                System.Threading.Thread.Sleep(1000);
                // Ask for RSSI and RTT so that we get updated 
                // values when calling kvrNetworkGetConnectionStatus()

                status = Kvrlib.NetworkGetRssiRtt(handle, out rssi, out rtt);

                if (!status.Equals(Kvrlib.Status.OK))
                {
                    Console.WriteLine("Could not get RSSI / RTT (" + status + ")");
                    break;
                }

                status = Kvrlib.NetworkGetConnectionStatus(handle, out state, out tx_rate, out rx_rate, out channel, out rssi_mean, out tx_power);
                if (!status.Equals(Kvrlib.Status.OK))
                {
                    Console.WriteLine("Could not get status (" + status + ")");
                    break;
                }


                Console.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
                Console.WriteLine("Connection state: " + state);
                Console.WriteLine("Transmit rate: " + tx_rate + " kbit/s");
                Console.WriteLine("Receive rate: " + rx_rate + " kbit/s");
                Console.WriteLine("Channel: " + channel);
                Console.WriteLine("Receive Signal Strength Indicator: " + rssi_mean + " dBm");
                Console.WriteLine("Transmit power level: " + tx_power + " dB");

                seconds--;

            }


            status = Kvrlib.NetworkConnectionTest(handle, 0);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could not stop ping(" + status + ")");
                return status;
            }

            status = Kvrlib.NetworkGetAddressInfo(handle, out address, out mac, out netmask, out gateway, out dhcp);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could not get IP info(" + status + ")");
                return status;
            }

            // Assume IP v.4, i.e. address/netmask/gateway.type is kvrAddressType_IPV4
            Console.WriteLine("- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -");
            Console.WriteLine("DHCP: " + dhcp);
            status = Kvrlib.StringFromAddress(out tmp_address, mac);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could convert string from address (" + status + ")");
                return status;
            }
            Console.WriteLine("MAC address: " + tmp_address);
            status = Kvrlib.StringFromAddress(out tmp_address, address);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could convert string from address (" + status + ")");
                return status;
            }
            Console.WriteLine("IP Address: " + tmp_address);
            status = Kvrlib.StringFromAddress(out tmp_address, netmask);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could convert string from address (" + status + ")");
                return status;
            }
            Console.WriteLine("Netmask: " + tmp_address);
            status = Kvrlib.StringFromAddress(out tmp_address, gateway);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could convert string from address (" + status + ")");
                return status;
            }
            Console.WriteLine("Gateway: " + tmp_address);

            status = Kvrlib.NetworkGetHostName(handle, out tmp_address);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could not get hostname (" + status + ")");
            }
            else
            {
                Console.WriteLine("Hostname: " + tmp_address);
            }


            //Creates a string containing the host name
            ean_hi = (uint)Convert.ToUInt64(ean) >> 32;
            ean_lo = (uint)Convert.ToUInt64(ean) & 0xffffffff;
            status = Kvrlib.HostName(ean_hi, ean_lo, Convert.ToUInt32(serial), out tmp_address);
            if (!status.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could not create hostname (" + status + ")");
                return status;
            }
            Console.WriteLine("Default hostname: " + tmp_address);

            return status;
        }

        /*
         * Erases the last configuration on the device and selects the first one as the active configuration
         */
        static void ChangeProfiles(int canlib_channel, object ean, object serial, int no_profiles, string password)
        {
            Kvrlib.Status kvrstat;
            Console.WriteLine("Clear the last profile");
            kvrstat = Kvrlib.ConfigActiveProfileSet(canlib_channel, no_profiles - 1);
            if (!kvrstat.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Set profile " + (no_profiles - 1) + " failed (" + kvrstat + ")");
            }

            // Wait for reboot
            if (WaitForDevice(ean, serial, 10000) != 0)
            { //10s timeout
                Console.WriteLine("waitForDevice() failed.");
            }

            EraseConfiguration(canlib_channel, password);

            //Wait for reboot
            if (WaitForDevice(ean, serial, 10000) != 0)
            { //10s timeout
                Console.WriteLine("waitForDevice() failed.");
            }

            Console.WriteLine("Set profile 0 as active");
            kvrstat = Kvrlib.ConfigActiveProfileSet(canlib_channel, 0);
            if (!kvrstat.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Set profile " + 0 + " failed (" + kvrstat + ")");
            }

        }

        static int Main(string[] args)
        {

            string password = "";
            string xml_config = "";
            Kvrlib.Status kvrstat;
            Kvrlib.ConfigHnd configHandle = new Kvrlib.ConfigHnd();
            Kvrlib.Address address = new Kvrlib.Address();
            Kvrlib.CipherInfoElement cipherInfoElement = new Kvrlib.CipherInfoElement();

            int cur_profile;
            int canlib_channel;
            int no_profiles;

            object serial;
            object ean;

            Canlib.canStatus stat = new Canlib.canStatus();

            Canlib.canInitializeLibrary();
            Kvrlib.InitializeLibrary();


            switch (args.Length)
            {
                case 0:
                    ListDevices();
                    return 0;
                case 1:
                    canlib_channel = args[0][0] - '0';
                    break;
                default:
                    ListDevices();
                    return -1;
            }

            stat = Canlib.canGetChannelData(canlib_channel, Canlib.canCHANNELDATA_CARD_UPC_NO, out ean);
            stat = Canlib.canGetChannelData(canlib_channel, Canlib.canCHANNELDATA_CARD_SERIAL_NO, out serial);

            if (IsAvailableForConfig(canlib_channel, password))
            {
                Console.WriteLine("Channel " + canlib_channel + " is available for configuration");
            }
            else
            {
                Console.WriteLine("Channel " + canlib_channel + " can not be opened for configuration");
                Canlib.canUnloadLibrary();
                return -1;
            }
            Canlib.canUnloadLibrary();


            //----------------------------------------------------------------------------
            // List number of profiles

            kvrstat = Kvrlib.ConfigNoProfilesGet(canlib_channel, out no_profiles);
            if (!kvrstat.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Device does not support profiles " + kvrstat);
                return Convert.ToInt32(kvrstat);
            }
            Console.WriteLine("Device supports " + no_profiles + " profiles.");

            Kvrlib.ConfigActiveProfileGet(canlib_channel, out cur_profile);
            if (!kvrstat.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could not get active profile number " + kvrstat);
                return Convert.ToInt32(kvrstat);
            }
            Console.WriteLine("Active profile is: " + cur_profile);

            // Show profiles
            for (int i = 0; i < no_profiles; i++)
            {
                kvrstat = Kvrlib.ConfigInfoGet(canlib_channel, i, out xml_config);
                if (kvrstat.Equals(Kvrlib.Status.BLANK))
                {
                    Console.WriteLine("Profile " + i + " is blank.");
                }
                else if (!kvrstat.Equals(Kvrlib.Status.OK))
                {
                    Console.WriteLine("Could not read profile " + i);
                    return Convert.ToInt32(kvrstat);
                }
                else
                {
                    Console.WriteLine("Profile " + i + ":\n" + xml_config);
                }
            }

            //----------------------------------------------------------------------------
            // Start configuration - read only
            kvrstat = Kvrlib.ConfigOpenEx(canlib_channel,
                                          Kvrlib.ConfigMode.R,
                                          password,
                                          out configHandle,
                                          cur_profile);

            if (!kvrstat.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could not start config " + kvrstat);
                return Convert.ToInt32(kvrstat);
            }

            // List available networks. This information could be 
            // helpful when creating the XML configuration.

            kvrstat = ScanNetworks(configHandle);
            Kvrlib.ConfigClose(configHandle);

            //----------------------------------------------------------------------------
            // Start configuration again - read/write

            kvrstat = Kvrlib.ConfigOpen(canlib_channel, Kvrlib.ConfigMode.RW, password, out configHandle);

            if (!kvrstat.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could not start config " + kvrstat);
                return Convert.ToInt32(kvrstat);
            }

            kvrstat = DoConfigure(configHandle);

            // Done!
            Kvrlib.ConfigClose(configHandle);

            // Wait for reboot
            if (WaitForDevice(ean, serial, 10000) != 0)
            { //10s timeout
                Console.WriteLine("waitForDevice() failed.");
                return -1;
            }

            //----------------------------------------------------------------------------
            // Start configuration - read only
            kvrstat = Kvrlib.ConfigOpen(canlib_channel, Kvrlib.ConfigMode.R, password, out configHandle);

            if (!kvrstat.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("Could not start config " + kvrstat);
                return Convert.ToInt32(kvrstat);
            }

            kvrstat = TestConfiguration(configHandle, 5, ean, serial);
            if (!kvrstat.Equals(Kvrlib.Status.OK))
            {
                Console.WriteLine("getNetworkInfo failed (" + kvrstat + ")");
                return Convert.ToInt32(kvrstat);
            }
            Kvrlib.ConfigClose(configHandle);
            Console.WriteLine("\nDone!");

            //Erase the last profile on the device and set the active profile to 0, uncomment to test
            //ChangeProfiles(canlib_channel, ean, serial, no_profiles, password);
            

            Kvrlib.UnloadLibrary();
            return 0;
        }
    }
}
