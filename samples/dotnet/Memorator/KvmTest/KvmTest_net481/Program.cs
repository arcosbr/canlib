/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html
*/

using System;
using Kvaser.KvmLib;

namespace KvmTest_net481
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int deviceNumber = 0;
            Kvmlib.Status status;

            Kvmlib.Initialize();

            //Opening the device and getting a handle to it
            Console.Write("Attempting to open device... ");
            Kvmlib.Handle h = Kvmlib.DeviceOpen(deviceNumber, out status, Kvmlib.DeviceType.kvmDEVICE_MHYDRA);
            Console.WriteLine(status);

            //Checking serial number
            int serial;
            status = Kvmlib.DeviceGetSerialNumber(h, out serial);
            CheckStatus(status, "Reading serial number");
            Console.WriteLine("Serial number: " + serial);

            //Reading the time of the devices and converting it to a DateTime object
            int unixTime;
            status = Kvmlib.DeviceGetRTC(h, out unixTime);
            CheckStatus(status, "Reading time");
            DateTime unixStartTime = new DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            DateTime deviceTime = unixStartTime.AddSeconds(unixTime).ToLocalTime();
            Console.WriteLine("Device time: " + deviceTime.ToString());

            //Checking if there is an SD card in the device
            int present;
            status = Kvmlib.DeviceDiskStatus(h, out present);
            CheckStatus(status, "Reading disk status");
            if (present == 0)
            {
                Console.WriteLine("No SD card in device!");
            }
            else
            {
                Console.WriteLine("SD card found");
            }

            //Checking the number of log files on the SD card
            int nFiles;
            status = Kvmlib.LogFileGetCount(h, out nFiles);
            CheckStatus(status, "Reading number of log files");
            Console.WriteLine("Number of log files on card: " + nFiles);

            //Checking disk usage
            int total, used;
            status = Kvmlib.KmfGetUsage(h, out total, out used);
            CheckStatus(status, "Reading disk usage");
            Console.WriteLine("{0} sectors used out of {1}", used, total);

            Kvmlib.Close(h);
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }


        //Print an error message if something goes wrong
        static void CheckStatus(Kvmlib.Status status, string action)
        {
            if (status != Kvmlib.Status.OK)
            {
                Console.WriteLine(action + " failed: " + status.ToString());
            }
        }
    }
}
