/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html
*/

using System;
using Kvaser.KvlcLib;

namespace ConvertLog_net481
{
    internal class Program
    {
        static void usage()
        {
            Console.WriteLine("\nThis program tests converts CAN frames in a KME50 file to plain ASCII.");
            Console.WriteLine("Usage:\n");
            Console.WriteLine("  ConvertLog outfile infile\n");
            Console.WriteLine("  outfile   The name of the resulting ASCII file.\n");
            Console.WriteLine("  infile    A KME50 file with CAN frames.");
            Console.WriteLine("  Note that output file is will be overwritten if it exists.");
        }

        // This program converts KME50 files to plain ASCII
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                usage();
                return;
            }

            for (int i = 0; i < args.Length; i++)
            {
            }

            Kvlclib.Status status;
            Kvlclib.Handle hnd;
            string errorText = "";

            // Create a new converter that converts to plain ASCII format
            status = Kvlclib.CreateConverter(out hnd, args[0], Kvlclib.FileFormat.FILE_FORMAT_PLAIN_ASC);
            if (status != Kvlclib.Status.OK)
            {
                Kvlclib.GetErrorText(status, out errorText);
                Console.WriteLine("ERROR: Could not create converter with output file '" + args[0] + "': " + status + ":" + errorText);
                return;
            }

            // Use KME50 as input format
            status = Kvlclib.SetInputFile(hnd, args[1], Kvlclib.FileFormat.FILE_FORMAT_KME50);
            if (status != Kvlclib.Status.OK)
            {
                Kvlclib.GetErrorText(status, out errorText);
                Console.WriteLine("ERROR: Could not open input file: '" + args[1] + "': " + status + ":" + errorText);
                goto Finish;
            }


            // Set properties, e.g. always overwrite existing files, use five channels, truncate data bytes
            status = setConvertionProperties(hnd);
            if (status != Kvlclib.Status.OK)
            {
                Kvlclib.GetErrorText(status, out errorText);
                Console.WriteLine("ERROR: Could not open input file: " + status + " " + errorText);
                goto Finish;
            }

            // Convert all CAN frames in the input file
            do
            {
                status = Kvlclib.ConvertEvent(hnd);
            }
            while (status == Kvlclib.Status.OK);

            // Make sure that the whole file was converted
            if (status != Kvlclib.Status.EOF)
            {
                Kvlclib.GetErrorText(status, out errorText);
                Console.WriteLine("ERROR: Conversion failed failed: " + status + " " + errorText);
                goto Finish;
            }

            // Check for overruns and truncated data
            status = checkConvertionStatus(hnd);
            if (status != Kvlclib.Status.OK)
            {
                Console.WriteLine("ERROR: Conversion failed.");
            }

        Finish:
            // Delete converter and close any open files
            status = Kvlclib.DeleteConverter(hnd);
            Console.WriteLine("Press any key to exit ..");
            Console.ReadKey();

            return;

        }

        static Kvlclib.Status setConvertionProperties(Kvlclib.Handle hnd)
        {
            Kvlclib.Status status;
            string errorText = "";

            // Always overwrite existing files
            status = Kvlclib.SetProperty(hnd, Kvlclib.Property.PROPERTY_OVERWRITE, 1);
            if (status != Kvlclib.Status.OK)
            {
                Kvlclib.GetErrorText(status, out errorText);
                Console.WriteLine("ERROR: Could not set property PROPERTY_OVERWRITE: " + status + " " + errorText);
                return status;
            }

            // Convert five channels; each channel is represented by one bit, i.e. binary 11111
            // is 0x1F
            status = Kvlclib.SetProperty(hnd, Kvlclib.Property.PROPERTY_CHANNEL_MASK, 0x1F);
            if (status != Kvlclib.Status.OK)
            {
                Kvlclib.GetErrorText(status, out errorText);
                Console.WriteLine("ERROR: Could not set property PROPERTY_CHANNEL_MASK: " + status + " " + errorText);
                return status;
            }

            // Write an informative header
            status = Kvlclib.SetProperty(hnd, Kvlclib.Property.PROPERTY_WRITE_HEADER, 1);
            if (status != Kvlclib.Status.OK)
            {
                Kvlclib.GetErrorText(status, out errorText);
                Console.WriteLine("ERROR: Could not set property PROPERTY_WRITE_HEADER: " + status + " " + errorText);
                return status;
            }

            // Write CAN frame id in hex
            status = Kvlclib.SetProperty(hnd, Kvlclib.Property.PROPERTY_ID_IN_HEX, 1);
            if (status != Kvlclib.Status.OK)
            {
                Kvlclib.GetErrorText(status, out errorText);
                Console.WriteLine("ERROR: Could not set property PROPERTY_ID_IN_HEX: " + status + " " + errorText);
                return status;
            }

            // Write CAN data bytes in hex
            status = Kvlclib.SetProperty(hnd, Kvlclib.Property.PROPERTY_DATA_IN_HEX, 1);
            if (status != Kvlclib.Status.OK)
            {
                Kvlclib.GetErrorText(status, out errorText);
                Console.WriteLine("ERROR: Could not set property PROPERTY_DATA_IN_HEX: " + status + " " + errorText);
                return status;
            }

            // Write calendar time stamps 
            status = Kvlclib.SetProperty(hnd, Kvlclib.Property.PROPERTY_CALENDAR_TIME_STAMPS, 1);
            if (status != Kvlclib.Status.OK)
            {
                Kvlclib.GetErrorText(status, out errorText);
                Console.WriteLine("ERROR: Could not set property PROPERTY_CALENDAR_TIME_STAMPS: " + status + " " + errorText);
                return status;
            }

            // Truncate the number of data bytes to 8.
            status = Kvlclib.SetProperty(hnd, Kvlclib.Property.PROPERTY_LIMIT_DATA_BYTES, 1);
            if (status != Kvlclib.Status.OK)
            {
                Kvlclib.GetErrorText(status, out errorText);
                Console.WriteLine("ERROR: Could not set property PROPERTY_LIMIT_DATA_BYTES: " + status + " " + errorText);
                return status;
            }

            return Kvlclib.Status.OK;

        }
        static Kvlclib.Status checkConvertionStatus(Kvlclib.Handle hnd)
        {
            Kvlclib.Status status;
            string errorText = "";
            int truncated = 0;
            int overrun = 0;

            // Check if CAN data bytes were truncated
            status = Kvlclib.IsDataTruncated(hnd, out truncated);
            if (status != Kvlclib.Status.OK)
            {
                Kvlclib.GetErrorText(status, out errorText);
                Console.WriteLine("ERROR: IsDataTruncated() failed: " + status + " " + errorText);
                return status;
            }

            if (truncated == 1)
            {
                Console.WriteLine("Warning: One or more CAN FD frames truncated to 8 bytes.");
            }

            // Check for overruns during logging
            status = Kvlclib.IsOverrunActive(hnd, out overrun);
            if (status != Kvlclib.Status.OK)
            {
                Kvlclib.GetErrorText(status, out errorText);
                Console.WriteLine("ERROR: IsOverrunActive() failed: " + status + " " + errorText);
                return status;
            }

            if (overrun == 1)
            {
                Console.WriteLine("Warning: One or more overruns were detected in the input file.\n" +
                                  "         Data were lost during the logging operation.");
            }

            return Kvlclib.Status.OK;
        }
    }
}
