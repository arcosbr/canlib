/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html
*/

using System;
using System.Collections.Generic;
using System.IO;
using Kvaser.KvmLib;

namespace ReadLogs_net481
{
    internal class Program
    {
        /*
         * TODO: Add command line options for card number, output type etc. 
         */

        static readonly String[] triggerTypes = new String[10] { "MSG_ID", "MSG_DLC", "MSG_FLAG", "SIGVAL", "EXTERNAL", "TIMER", "DISK_FULL", "", "", "STARTUP" };

        //This program reads all the log files from a device and saves them in one plain text file per log file
        static void Main(string[] args)
        {
            //Initialization
            Kvmlib.Status status;
            int deviceNumber = 0;

            Kvmlib.Initialize();

            Kvmlib.Handle handle = Kvmlib.DeviceOpen(deviceNumber, out status, Kvmlib.DeviceType.kvmDEVICE_MHYDRA);
            Console.WriteLine(status);

            int logFileCount;
            status = Kvmlib.LogFileGetCount(handle, out logFileCount);
            Console.WriteLine("Number of log files: " + logFileCount);

            Console.WriteLine(status);

            //Going through each log file, saving the events to plain text files
            for (int i = 0; i < logFileCount; i++)
            {
                List<Kvmlib.Log> events = ReadLogFile(handle, i);

                Console.WriteLine("{0} events read from log file {1}", events.Count, i);
                string filename = "log" + i + ".txt";
                WriteListToText(events, filename);
                WriteMessagesCsv(events, "log" + i + ".csv");
                Console.WriteLine("Events written to " + filename);
            }

            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
            Kvmlib.Close(handle);
        }



        //Returns a list containing all the events found in the log file
        //with the supplied index
        static List<Kvmlib.Log> ReadLogFile(Kvmlib.Handle handle, int index)
        {
            List<Kvmlib.Log> events = new List<Kvmlib.Log>();

            int eventCount;
            Kvmlib.LogFileMount(handle, index, out eventCount);

            Kvmlib.Log log;
            Kvmlib.Status status = Kvmlib.LogFileReadEvent(handle, out log);

            //Loop until all events have been read.
            while (status == Kvmlib.Status.OK)
            {
                events.Add(log);
                status = Kvmlib.LogFileReadEvent(handle, out log);
            }
            Kvmlib.LogFileDismount(handle);
            return events;
        }


        //Writes the lists of events to the file with the supplied name.
        static void WriteListToText(List<Kvmlib.Log> events, string filename)
        {
            System.IO.StreamWriter writer = new StreamWriter(filename, true);
            foreach (Kvmlib.Log evnt in events)
            {
                string output = "";

                //Creates different messages depending on the type of the Log.
                if (typeof(Kvmlib.LogMsg) == evnt.GetType())
                {
                    Kvmlib.LogMsg msg = (Kvmlib.LogMsg)evnt;
                    output = String.Format("Msg   Channel: {0} Id: {1}, dlc: {2}, data: {3}, flags: {4}, time: {5} ",
                        msg.channel, msg.id, msg.dlc, String.Join(" ", msg.data), msg.flags, msg.timeStamp);
                }
                else if (typeof(Kvmlib.LogRtcClock) == evnt.GetType())
                {
                    Kvmlib.LogRtcClock rtcEvent = (Kvmlib.LogRtcClock)evnt;
                    DateTime deviceTime = new DateTime((Int64)(rtcEvent.calendarTime) * 10000000);
                    deviceTime = deviceTime.AddYears(1969);
                    output = "Clock   " + deviceTime.ToString();
                }
                else if (typeof(Kvmlib.LogTrigger) == evnt.GetType())
                {
                    Kvmlib.LogTrigger trigger = (Kvmlib.LogTrigger)evnt;
                    output = String.Format("Trigger   Pre: {0}  Post{1}  Mask: {2}  Time: {3} Type: {4}",
                        trigger.preTrigger, trigger.postTrigger, trigger.trigMask, trigger.timeStamp, triggerTypes[trigger.type]);
                }
                //Writes the message to the file
                writer.WriteLine(output);
            }
            writer.Close();
        }

        //Writes all logged CAN messages to a csv file
        static void WriteMessagesCsv(List<Kvmlib.Log> events, string filename)
        {
            System.IO.StreamWriter writer = new StreamWriter(filename, true);
            String header = "channel, id, dlc, data1, data2, data3, data4, data5, data6, data7, data8, flags, timestamp";
            writer.WriteLine(header);
            foreach (Kvmlib.Log evnt in events)
            {
                if (evnt.GetType() == typeof(Kvmlib.LogMsg))
                {
                    Kvmlib.LogMsg msg = (Kvmlib.LogMsg)evnt;
                    String output = String.Format("{0},{1},{2},{3},{4},{5} ",
                        msg.channel, msg.id, msg.dlc, String.Join(",", msg.data), msg.flags, msg.timeStamp);
                    writer.WriteLine(output);
                }
            }
            writer.Close();
        }
    }
}
