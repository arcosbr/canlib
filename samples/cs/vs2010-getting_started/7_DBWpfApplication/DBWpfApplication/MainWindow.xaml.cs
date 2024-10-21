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
using Kvaser.Kvadblib;
using System.Collections.ObjectModel;
using System.Windows.Controls.DataVisualization.Charting;
using System.ComponentModel;

namespace DBWpfApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Handle for the channel to listen for messages on
        int chanHandle;

        //Handle to the database and the message to listen to
        Kvadblib.Hnd dh;
        Kvadblib.MessageHnd msgHandle;

        //ID of the message, incoming messages should match this
        int msgId, msgFlags;

        //Used for running the DumpMessageLoop method
        private readonly BackgroundWorker dumper;

        //Used for containing the elements of the message combo box,
        //gets updated every time a new database is loaded.
        public ObservableCollection<ComboBoxItem> boxItems{get; set;}

        //Must be true before we start listening to messages
        bool channelOn = false;
        bool hasMessage = false;

        //Used for exiting the DumpMessageLoop
        bool finished = false;

        //Determines the horizontal scale of the chart when it becomes too big for the window.
        private int pixelsPerSecond = 100;

        //This list contain ObservableCollections of data points, these collections
        //are bound to the lines in the chart.
        List<ObservableCollection<KeyValuePair<long, double>>> chartValues;

        public MainWindow()
        {
            InitializeComponent();

            //Initialize the combobox binding
            selectMsgBox.DataContext = this;
            boxItems = new ObservableCollection<ComboBoxItem>();

            //Create a new list for the line data collections
            chartValues = new List<ObservableCollection<KeyValuePair<long, double>>>();

            //Set up the BackgroundWorker
            dumper = new BackgroundWorker();
            dumper.DoWork += DumpMessageLoop;
            dumper.WorkerReportsProgress = true;
            dumper.ProgressChanged += new ProgressChangedEventHandler(ProcessMessage);

        }

        /*
         * Opens a dialog which lets the user browse for a database file, then loads that database
         */
        private void loadDbButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog filedialog = new Microsoft.Win32.OpenFileDialog();
            filedialog.DefaultExt = ".dbc";

            Nullable<bool> hasResult = filedialog.ShowDialog();

            if (hasResult == true)
            {
                string dbFile = filedialog.FileName;        //File name with full path
                string safeFile = filedialog.SafeFileName;  //File name without the path

                if (LoadDB(dbFile))
                {
                    SetupMessagesBox();
                    selectBlock.Text = safeFile;
                }
            }
        }

        /*
         * Sets up the selected channel, goes on bus.
         */
        private void initButton_Click(object sender, RoutedEventArgs e)
        {
            int channel = Int32.Parse(channelBox.Text);
            Canlib.canStatus status;

            Canlib.canInitializeLibrary();
            int chanHandle = Canlib.canOpenChannel(channel, Canlib.canOPEN_ACCEPT_VIRTUAL);
            if (chanHandle >= 0)
            {
                status = Canlib.canSetBusParams(chanHandle, Canlib.canBITRATE_250K, 0, 0, 0, 0, 0);
                status = Canlib.canBusOn(chanHandle);
                CheckCANStatus("On bus", status);

                if (status == Canlib.canStatus.canOK)
                {
                    channelOn = true;
                }
            }
            else
            {
                CheckCANStatus("Opening channel", (Canlib.canStatus)hnd);
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

            CheckDBStatus("Loading database", status);

            return status == Kvadblib.Status.OK;

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

            //Iterate through all messages in the database
            status = Kvadblib.GetFirstMsg(dh, out mh);
            while(status == Kvadblib.Status.OK)
            {
                string name;
                int id;
                Kvadblib.MESSAGE flags;
                status = Kvadblib.GetMsgName(mh, out name);
                status = Kvadblib.GetMsgId(mh, out id, out flags);

                //Add the message to the ComboBox
                boxItems.Add(new ComboBoxItem{Content = name, Tag = id});
                
                status = Kvadblib.GetNextMsg(dh, out mh);
            }
            if (status != Kvadblib.Status.Err_NoMsg)
            {
                CheckDBStatus("Reading message", status);
            }
        }

        /*
         * Loads the message selected in the ComboBox.
         */
        private void loadMsgButton_Click(object sender, RoutedEventArgs e)
        {
            Kvadblib.MessageHnd mh;
            int id = (int) ((ComboBoxItem) selectMsgBox.SelectedItem).Tag;
            Kvadblib.Status status = Kvadblib.GetMsgById(dh, id, out mh);
            if (status == Kvadblib.Status.OK)
            {
                msgHandle = mh;
                msgId = ((id & -2147483648) == 0) ? id : id ^ -2147483648;
                msgFlags = ((id & -2147483648) == 0) ? 0 : Canlib.canMSG_EXT;
                msgIdLabel.Text = "Message id: " + msgId;
                hasMessage = true;
                LoadSignals();
                UpdateChartWidth();
            }
            CheckDBStatus("Loading message", status);
        }
        /*
         * Once a message is loaded, all the signals are fetched and added to the chart
         */
        private void LoadSignals()
        {
            chartValues.Clear();
            signalChart.Series.Clear();

            //Iterate through all the signals
            Kvadblib.SignalHnd sh;
            Kvadblib.Status status = Kvadblib.GetFirstSignal(msgHandle, out sh);
            while (status == Kvadblib.Status.OK)
            {
                string name;
                string unit;

                status = Kvadblib.GetSignalName(sh, out name);
                status = Kvadblib.GetSignalUnit(sh, out unit);

                AddLine(name, unit);
                status = Kvadblib.GetNextSignal(msgHandle, out sh);
            }
            if (status == Kvadblib.Status.Err_NoSignal)
            {
                statusText.Text = "Signals loaded";
            }
            else
            {
                CheckDBStatus("Loading signals", status);
            }
        }

        /*
         * Starts the DumpMessageLoop if a channel is open and a message is loaded
         */
        private void startLoggingButton_Click(object sender, RoutedEventArgs e)
        {
            if (hasMessage && channelOn)
            {
                if (!dumper.IsBusy)
                {
                    dumper.RunWorkerAsync();
                    statusText.Text = "Started logging, waiting for messages...";
                }
            }
            else
            {
                MessageBox.Show("Please load a message and open channel");
            }
        }

        /*
         * Called once for each signal in a message
         * Adds a line to the chart for the signal, 
         * also adds an ObservableCollection for the data points.
         */
        private void AddLine(string name, string unit)
        {
            ObservableCollection<KeyValuePair<long, double>> dataPoints = new ObservableCollection<KeyValuePair<long, double>>();
            chartValues.Add(dataPoints);

            LineSeries series = new LineSeries();
            //Only display the unit if it is not empty
            unit = (unit.Length == 0) ? "" : "(" + unit + ")"; 
            series.Title = String.Format("{0} {1}", name, unit);
            series.DependentValuePath = "Value";
            series.IndependentValuePath = "Key";
            series.ItemsSource = dataPoints;
            signalChart.Series.Add(series);
        }

        /*
         * Loops and checks for new messages, sends the data and timestamp to ProcessMessage
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
            bool noError = true;
            while (noError && !finished && chanHandle >= 0)
            {
                status = Canlib.canReadWait(chanHandle, out id, data, out dlc, out flags, out time, 50);

                if (status == Canlib.canStatus.canOK)
                {
                    if (id == msgId)
                    {
                        Message m = new Message(id, data, dlc,flags, time);
                        worker.ReportProgress(0, m);
                    }
                }
                else if(status != Canlib.canStatus.canERR_NOMSG)
                {
                    noError = false;
                }
            }
            Canlib.canBusOff(chanHandle);
        }

        /*
         * Receives a new message and adds data points on all signals
         */
        private void ProcessMessage(object sender, ProgressChangedEventArgs e)
        {
            Message m = (Message)e.UserState;

            //Iterate through the signals
            Kvadblib.SignalHnd sh;
            Kvadblib.Status status = Kvadblib.GetFirstSignal(msgHandle, out sh);
            int i = 0;
            while (status == Kvadblib.Status.OK)
            {
                //Get the signal's physical value and add a new point to the corresponding line
                double value;
                status = Kvadblib.GetSignalValueFloat(sh, out value, m.data, m.dlc); 
                chartValues.ElementAt(i).Add(new KeyValuePair<long, double>(m.time, value));
                i++;
                status = Kvadblib.GetNextSignal(msgHandle, out sh);
            }
            statusText.Text = m.id.ToString();
            UpdateChartWidth();
        }


        /*
         * Updates the width of the chart to add a scroll bar
         * whenever the width becomes too large
         */
        private void UpdateChartWidth()
        {
            ObservableCollection<KeyValuePair<long, double>> firstLine = chartValues.First();

            //deltaTime is the difference in time between the first and the last point
            long deltaTime;
            if (firstLine.Count > 1)
            {
                long firstTime = firstLine.First().Key;
                long lastTime = firstLine.Last().Key;
                deltaTime = lastTime - firstTime;
            }
            else
            {
                deltaTime = 0;
            }

            if (deltaTime * pixelsPerSecond / 1000 > 750)
            {
                signalChart.Width = (int)(deltaTime * pixelsPerSecond / 1000);
                chartScroll.ScrollToRightEnd();
            }
            else
            {
                signalChart.Width = 720;
            }
        }

        /*
         * Updates the status bar, prints error message if something goes wrong
         */
        private void CheckCANStatus(String action, Canlib.canStatus status)
        {
            if (status != Canlib.canStatus.canOK)
            {
                String errorText = "";
                Canlib.canGetErrorText(status, out errorText);
                statusText.Text = action + " failed: " + errorText;
            }
            else
            {
                statusText.Text = action + " succeeded";
            }
        }

        /*
         * Updates the status bar, prints error message if something goes wrong (for Kvadblib calls)
         */
        private void CheckDBStatus(String action, Kvadblib.Status status)
        {
            if (status != Kvadblib.Status.OK)
            {
                statusText.Text = action + " failed: " + status.ToString() ;
            }
            else
            {
                statusText.Text = action + " succeeded";
            }
        }
    }

    /*
     * Class used to hold a message, used by DumpMessageLoop and ProcessMessage
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
    }
}
