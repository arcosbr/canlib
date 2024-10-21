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
using Kvaser.Kvadblib;
using System.Collections.ObjectModel;
using System.Globalization;

namespace DBLog
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int handle;
        private readonly BackgroundWorker dumper;
        private bool looping = false;
        private bool hasFile = false;
        private System.IO.StreamWriter streamWriter;
        private Kvadblib.MessageHnd msgHandle;
        Kvadblib.Hnd dh;
        bool hasMessage = false;
        int msgId;
        private int msgFlags;


        //Used for containing the elements of the message combo box,
        //gets updated every time a new database is loaded.
        public ObservableCollection<ComboBoxItem> boxItems { get; set; }


        public MainWindow()
        {

            InitializeComponent();

            //Initialize the combobox binding
            selectMsgBox.DataContext = this;
            boxItems = new ObservableCollection<ComboBoxItem>();

            //Setup the background worker
            dumper = new BackgroundWorker();
            dumper.DoWork += DumpMessageLoop;
            dumper.WorkerReportsProgress = true;
            dumper.ProgressChanged += new ProgressChangedEventHandler(ProcessMessage);

            Canlib.canInitializeLibrary();
        }

        /*
         * Opens a CAN channel
         */
        private void InitializeButton_Click(object sender, RoutedEventArgs e)
        {
            int channel = Int32.Parse(channelBox.Text);
            handle = Canlib.canOpenChannel(channel, Canlib.canOPEN_ACCEPT_VIRTUAL);
            Canlib.canSetBitrate(handle, Canlib.canBITRATE_250K);
            Canlib.canBusOn(handle);
        }

        /*
         * Opens a dialog for creating a csv file
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
                hasFile = true;
                fileNameLabel.Content = saveFileDialog.SafeFileName;
            }
        }

        /*
         * Opens a file dialog and lets the user select a database file
         */
        private void loadDbButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog filedialog = new Microsoft.Win32.OpenFileDialog();
            filedialog.DefaultExt = ".dbc";

            Nullable<bool> hasResult = filedialog.ShowDialog();

            if (hasResult == true)
            {
                string dbFile = filedialog.FileName;
                string safeFile = filedialog.SafeFileName;

                if (LoadDB(dbFile))
                {
                    SetupMessagesBox();
                    DBFileLabel.Content = safeFile;
                }
            }
        }

        /*
         * Loads the selected database
         * returns true if OK.
         */
        private bool LoadDB(string filename)
        {
            Kvadblib.Hnd hnd = new Kvadblib.Hnd();
            Kvadblib.Status status;

            Kvadblib.Open(out hnd);

            status = Kvadblib.ReadFile(hnd, filename);

            if (status == Kvadblib.Status.OK)
            {
                dh = hnd;
            }

            return status == Kvadblib.Status.OK;

        }

        /*
         * Loads the selected message from the database
         */
        private void loadMsgButton_Click(object sender, RoutedEventArgs e)
        {
            Kvadblib.MessageHnd mh;
            int id = (int)((ComboBoxItem)selectMsgBox.SelectedItem).Tag;
            Kvadblib.Status status = Kvadblib.GetMsgById(dh, id, out mh);
            if (status == Kvadblib.Status.OK)
            {
                msgHandle = mh;
                msgId = ((id & -2147483648) == 0) ? id : id ^ -2147483648;
                msgFlags = ((id & -2147483648) == 0) ? 0 : Canlib.canMSG_EXT;
                hasMessage = true;
            }
        }

        /*
         * Generate a header for the CSV file from the signal names
         */
        private string createHeader()
        {
            string s = "";
            if (hasMessage)
            {
                List<string> signals = new List<string>();
                Kvadblib.SignalHnd signal;
                Kvadblib.Status status = Kvadblib.GetFirstSignal(msgHandle, out signal);
                while (status == Kvadblib.Status.OK)
                {
                    string signalName;
                    Kvadblib.GetSignalName(signal, out signalName);
                    signals.Add(signalName);
                    status = Kvadblib.GetNextSignal(msgHandle, out signal);
                }
                signals.Add("time");
                s = String.Join(",", signals);

            }
            return s;
        }

        /*
         * Once a database is loaded, this method will be called
         * to add all the messages to the ComboBox.
         */
        private void SetupMessagesBox()
        {
            Kvadblib.MessageHnd mh;
            Kvadblib.Status status;

            boxItems.Clear();

            status = Kvadblib.GetFirstMsg(dh, out mh);
            while (status == Kvadblib.Status.OK)
            {
                string name;
                int id;
                Kvadblib.MESSAGE flags;
                status = Kvadblib.GetMsgName(mh, out name);
                status = Kvadblib.GetMsgId(mh, out id, out flags);

                boxItems.Add(new ComboBoxItem { Content = name, Tag = id });

                status = Kvadblib.GetNextMsg(dh, out mh);
            }
        }

        /*
         * Closes the CAN channel and the stream writer
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
                    message = new Message(id, data, dlc, flags, time, msgHandle);

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
         * If the message matches the one loaded from the database, the physical values
         * will be printed to screen and written to file
         */
        private void ProcessMessage(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                Message message = (Message)e.UserState;

                if (hasFile && message.id == msgId)
                {
                    OutputBox.AppendText(message.ToString() + "\r");
                    OutputBox.ScrollToEnd();
                    streamWriter.WriteLine(message.ToCSV());
                }
            }
            else if (e.ProgressPercentage == 100)
            {
                string error = (string)e.UserState;
                OutputBox.AppendText(error);
                OutputBox.ScrollToEnd();
            }
        }


        /*
         * Starts the listening loop
         */
        private void StartLoggingButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(hasMessage && hasFile))
            {
                MessageBox.Show("Please load a database file and select output file");
            }
            else if (!dumper.IsBusy)
            {
                streamWriter.WriteLine(createHeader());
                dumper.RunWorkerAsync();
            }
        }
    }

    /*
     * Contains data about a message, including a handle to the database message
     */
    public class Message
    {
        public int id { get; set; }
        public byte[] data { get; set; }
        public int dlc { get; set; }
        public int flags { get; set; }
        public long time { get; set; }
        public Kvadblib.MessageHnd mh { get; set; }

        public Message(int id, byte[] data, int dlc, int flags, long time, Kvadblib.MessageHnd mh)
        {
            this.id = id;
            this.data = data;
            this.dlc = dlc;
            this.flags = flags;
            this.time = time;
            this.mh = mh;
        }

        public override string ToString()
        {
            String ret;
            if ((flags & Canlib.canMSG_ERROR_FRAME) == Canlib.canMSG_ERROR_FRAME)
            {
                ret = "***ERROR FRAME RECEIVED***";
            }
            else
            {
                List<String> values = new List<string>();
                Kvadblib.SignalHnd sh;
                Kvadblib.Status status = Kvadblib.GetFirstSignal(mh, out sh);

                while (status == Kvadblib.Status.OK)
                {
                    double physVal;
                    string name;
                    status = Kvadblib.GetSignalName(sh, out name);
                    status = Kvadblib.GetSignalValueFloat(sh, out physVal, data, dlc);
                    if (status == Kvadblib.Status.OK)
                    {
                        values.Add(name + ": " + physVal.ToString(CultureInfo.CreateSpecificCulture("en-GB")));
                    }
                    status = Kvadblib.GetNextSignal(mh, out sh);
                }
                values.Add("Time: " + time.ToString());


                return String.Join(",", values);
            }
            return ret;
        }

        public string ToCSV()
        {
            List<String> values = new List<string>();
            Kvadblib.SignalHnd sh;
            Kvadblib.Status status = Kvadblib.GetFirstSignal(mh, out sh);

            while (status == Kvadblib.Status.OK)
            {
                double physVal;
                status = Kvadblib.GetSignalValueFloat(sh, out physVal, data, dlc);
                if (status == Kvadblib.Status.OK)
                {
                    values.Add(physVal.ToString(CultureInfo.CreateSpecificCulture("en-GB")));
                }
                status = Kvadblib.GetNextSignal(mh, out sh);
            }
            values.Add(time.ToString());

            return String.Join(",", values);
        }

    }
}
