using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Kvaser.CanLib;
using System.ComponentModel;
using Microsoft.Win32;

namespace Logger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Handle to the CAN channel
        private int handle;

        //Used for the dumping loop
        private readonly BackgroundWorker dumper;
        private bool looping = false;

        //Used for writing to file
        private bool hasFile = false;
        private System.IO.StreamWriter streamWriter;


        public MainWindow()
        {
            InitializeComponent();
            dumper = new BackgroundWorker();
            dumper.DoWork += DumpMessageLoop;
            dumper.WorkerReportsProgress = true;
            dumper.ProgressChanged += new ProgressChangedEventHandler(ProcessMessage);
            Canlib.canInitializeLibrary();
        }

        /*
         * Opens up a CAN channel
         */
        private void InitializeButton_Click(object sender, RoutedEventArgs e)
        {
            int channel = Int32.Parse(channelBox.Text);
            handle = Canlib.canOpenChannel(channel, Canlib.canOPEN_ACCEPT_VIRTUAL);
            Canlib.canSetBitrate(handle, Canlib.canBITRATE_250K);
            Canlib.canBusOn(handle);

            if (!dumper.IsBusy)
            {
                dumper.RunWorkerAsync();
            }
        }

        /*
         * Lets the user create a log file
         */
        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".csv";
            bool? result = saveFileDialog.ShowDialog();
            if (result == true)
            {
                System.IO.Stream fileStream = saveFileDialog.OpenFile();
                streamWriter = new System.IO.StreamWriter(fileStream);
                streamWriter.WriteLine("id,dlc,data1,data2,data3,data4,data5,data6,data7,flags,time");
                hasFile = true;
                MessageBox.Show("File loaded");
            }

        }
        
        /*
         * Close CAN channel and stream writer when exiting the program
         */
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            looping = false;
            Canlib.canBusOff(handle);
            Canlib.canClose(handle);
            Canlib.canUnloadLibrary();
            if (hasFile)
            {
                streamWriter.Close();
            }
            base.OnClosing(e);
        }

        /*
         * Listens for messages on the channel
         */
        private void DumpMessageLoop(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Canlib.canStatus status;
            int id;
            byte[] data = new byte[8];
            int dlc;
            int flags;
            long time;
            Message message;

            looping = true;

            while (looping)
            {
                status = Canlib.canReadWait(handle, out id, data, out dlc, out flags, out time, 50);

                while (status == Canlib.canStatus.canOK)
                {
                    message = new Message(id, data, dlc, flags, time);
                    
                    worker.ReportProgress(0, message);
                    status = Canlib.canRead(handle, out id, data, out dlc, out flags, out time);
                }

                if (status != Canlib.canStatus.canERR_NOMSG)
                {
                    looping = false;
                    worker.ReportProgress(100, "Unexpected error while reading");
                }
            }
        }

        /*
         * Prints the messages to screen and writes them to the log file
         */
        private void ProcessMessage(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                Message message = (Message)e.UserState;
                OutputBox.AppendText(message.toString());
                OutputBox.ScrollToEnd();

                if (hasFile)
                {
                    streamWriter.WriteLine(message.toCSV());
                }
            }
            else if(e.ProgressPercentage == 100)
            {
                string error = (string)e.UserState;
                OutputBox.AppendText(error);
                OutputBox.ScrollToEnd();
            }
        }
    }

    /*
     * This class holds all the info about messages
     */
    public class Message
    {
        public int id { get; set; }
        public byte[] data { get; set; }
        public int dlc { get; set; }
        public int flags { get; set; }
        public long time { get; set; }

        public Message(int id, byte[] data, int dlc, int flags, long time)
        {
            this.id = id;
            this.data = data;
            this.dlc = dlc;
            this.flags = flags;
            this.time = time;
        }

        public string toString()
        {
            String ret;
            if ((flags & Canlib.canMSG_ERROR_FRAME) == Canlib.canMSG_ERROR_FRAME)
            {
                ret = "***ERROR FRAME RECEIVED***";
            }
            else
            {
                ret = String.Format("{0}  {1}  {2:x2} {3:x2} {4:x2} {5:x2} {6:x2} {7:x2} {8:x2} {9:x2}  {10}   {11}\r",
                                         id, dlc, data[0], data[1], data[2], data[3], data[4],
                                         data[5], data[6], data[7], flagsString(flags), time);
            }
            return ret;
        }

        public string toCSV()
        {
            return String.Format("{0},{1},{2:x2},{3:x2},{4:x2},{5:x2},{6:x2},{7:x2},{8:x2},{9:x2},{10},{11}",
                                         id, dlc, data[0], data[1], data[2], data[3], data[4],
                                         data[5], data[6], data[7], flagsString(flags), time);
        }

        private static string flagsString(int flags)
        {
            return ((flags & Canlib.canMSG_ERROR_FRAME) != 0 ? "E" : "-") +
                ((flags & Canlib.canMSG_EXT) != 0 ? "x" : "-")+
                ((flags & Canlib.canMSG_RTR) != 0 ? "r" : "-")+
                ((flags & Canlib.canMSGERR_OVERRUN) != 0 ? "o" : "-")+
                ((flags & Canlib.canMSG_NERR) != 0 ? "n" : "-");
        }
    }
}
