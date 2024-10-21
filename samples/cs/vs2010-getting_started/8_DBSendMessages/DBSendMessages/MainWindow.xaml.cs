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
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace DBSendMessages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Handle for the channel to listen for messages on
        int chanHandle;
        //Handle to the database and the message to send
        Kvadblib.Hnd dh;
        Kvadblib.MessageHnd msgHandle;
        //ID of the message
        int msgId;
        int msgFlags;

        public ObservableCollection<ComboBoxItem> boxItems { get; set; }
        List<Signal> signals = new List<Signal>();
        List<TextBox> textBoxes = new List<TextBox>();

        //Used to determine which actions are possible
        //e.g. we can't send a message unless we've opened a channel
        bool channelOn = false;
        bool hasMessage = false;
        bool autoTransmit = false;
        bool hasDb = false;

        //Used for running the AutoTransmit loop
        private readonly BackgroundWorker transmitter;

        public MainWindow()
        {
            InitializeComponent();

            //Initialize the combobox
            selectMsgBox.DataContext = this;
            boxItems = new ObservableCollection<ComboBoxItem>();

            //Set up the BackgroundWorker
            transmitter = new BackgroundWorker();
            transmitter.DoWork += SendMessageLoop;
            transmitter.WorkerReportsProgress = true;
            transmitter.ProgressChanged += new ProgressChangedEventHandler(ProcessMessage);

            UpdateButtons();
        }

        /*
         * Loads a database from the file selected by the user.
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
                    hasDb = true;
                    SetupMessagesBox();
                    selectBlock.Text = safeFile;
                }
            }
            UpdateButtons();
        }

        /*
         * Loads the selected message's signals to construct a form
         */
        private void loadMsgButton_Click(object sender, RoutedEventArgs e)
        {
            Kvadblib.MessageHnd mh;
            int id = (int)((ComboBoxItem)selectMsgBox.SelectedItem).Tag;
            Kvadblib.Status status = Kvadblib.GetMsgById(dh, id, out mh);
            if (status == Kvadblib.Status.OK)
            {
                Kvadblib.MESSAGE f;
                msgHandle = mh;
                Kvadblib.GetMsgId(mh, out msgId, out f);
                msgId = ((id & -2147483648) == 0) ? id : id ^ -2147483648;
                msgFlags = ((id & -2147483648) == 0) ? 0 : Canlib.canMSG_EXT;
                msgIdLabel.Text = "Message id: " + msgId;
                hasMessage = true;
                LoadSignals();
            }
            CheckDBStatus("Loading message", status);
            UpdateButtons();
        }

        /*
         * Initiates the channel
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
                CheckCANStatus("Set bus parameters", status);

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
            UpdateButtons();
        }

        /*
         * Sends a message if one is constructed and we are on bus
         */
        private void sendMsgButton_Click(object sender, RoutedEventArgs e)
        {
            if (hasMessage && channelOn)
            {
                SendMessage();
            }
            UpdateButtons();
        }

        /*
         * Starts auto transmission
         */
        private void startAutoButton_Click(object sender, RoutedEventArgs e)
        {
            autoTransmit = true;
            int interval;
            bool parsed = Int32.TryParse(intervalBox.Text, out interval);
            if (!transmitter.IsBusy && parsed)
            {
                transmitter.RunWorkerAsync(interval);
                statusText.Text = "Started auto transmit";
            }
            else if (!parsed)
            {
                MessageBox.Show("Interval must be an integer value");
            }
            UpdateButtons();
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
            if (status != Kvadblib.Status.Err_NoMsg)
            {
                CheckDBStatus("Reading message", status);
            }
            UpdateButtons();
        }


        /*
         * Constructs a form for creating messages.
         * Consists of one TextBox for every signal in the loaded message.
         */
        private void LoadSignals()
        {
            //Empty the current form
            SignalGrid.RowDefinitions.Clear();
            SignalGrid.Children.Clear();
            signals.Clear();

            int row = 0;
            Kvadblib.SignalHnd sh;
            Kvadblib.Status status = Kvadblib.GetFirstSignal(msgHandle, out sh);

            while (status == Kvadblib.Status.OK)
            {
                string name;
                string unit;
                double min, max, scale, offset;

                //Construct the text for the label
                status = Kvadblib.GetSignalName(sh, out name);
                status = Kvadblib.GetSignalUnit(sh, out unit);
                status = Kvadblib.GetSignalValueLimits(sh, out min, out max);
                status = Kvadblib.GetSignalValueScaling(sh, out scale, out offset);
                String inputString = String.Format("{0} ({1} - {2} {3})", name, min, max, unit);

                //Create the label and textbox
                TextBlock signalTextBlock = new TextBlock() { Text = inputString, MaxWidth = 230, TextWrapping = TextWrapping.Wrap };
                Label signalLabel = new Label() { Content = inputString, MaxWidth=230 } ;
                TextBox signalTextBox = new TextBox() { MaxLines = 1, Height = 23, Name = name, Text = min.ToString(), Tag=row };
                signalTextBox.TextChanged += UpdateSignal;
                textBoxes.Add(signalTextBox);
                Signal s = new Signal(name, max, min, min, scale);
                signals.Add(s);
                

                //Create a new row for the signal
                RowDefinition newRow = new RowDefinition();
                newRow.Height = GridLength.Auto;
                SignalGrid.RowDefinitions.Add(newRow);

                //Add the label and the textbox to the grid
                Grid.SetColumn(signalTextBlock, 0);
                Grid.SetRow(signalTextBlock, row);
                Grid.SetColumn(signalTextBox, 1);
                Grid.SetRow(signalTextBox, row);
                SignalGrid.Children.Add(signalTextBlock);
                SignalGrid.Children.Add(signalTextBox);

                status = Kvadblib.GetNextSignal(msgHandle, out sh);
                row++;
            }
            if (status == Kvadblib.Status.Err_NoSignal)
            {
                statusText.Text = row + "Signals loaded";
            }
            else
            {
                CheckDBStatus("Loading signals", status);
            }

        }

        /*
         * Sends messages periodically
         */
        private void SendMessageLoop(object sender, DoWorkEventArgs e)
        {
            int interval = (int)e.Argument;
            BackgroundWorker worker = sender as BackgroundWorker;

            while (autoTransmit)
            {
                System.Threading.Thread.Sleep(interval);
                worker.ReportProgress(0);
            }
        }

        /*
         * Used by SendMessageLoop to send messages
         */
        private void ProcessMessage(object sender, ProgressChangedEventArgs e)
        {
            SendMessage();
        }

        /*
         * Constructs a message from the form and sends it
         * to the channel
         */
        private void SendMessage()
        {
            byte[] data = new byte[8];

            Kvadblib.Status status = Kvadblib.Status.OK;
            Kvadblib.SignalHnd sh;
            bool error = false;

            foreach (Signal s in signals)
            {
                double min, max;
                status = Kvadblib.GetSignalByName(msgHandle, s.name, out sh);

                if (status != Kvadblib.Status.OK)
                {
                    CheckDBStatus("Finding signal", status);
                    error = true;
                    break;
                }

                Kvadblib.GetSignalValueLimits(sh, out min, out max);

                status = Kvadblib.StoreSignalValuePhys(sh, data, 8, s.Value);

                //Check if the signal value was successfully stored and that it's in the correct interval
                if (status != Kvadblib.Status.OK || s.Value < min || s.Value > max)
                {
                    MessageBox.Show("Invalid value for " + s.name);
                    autoTransmit = false;
                    error = true;
                    break;
                }
            }

            if (!error)
            {
                
                Canlib.canWriteWait(chanHandle, msgId, data, 8, msgFlags, 50);
            }
            if(randomizeCheckBox.IsChecked == true)
            {
                Randomize();
            }
        }

        /*
         * Changes the value of a signal with up to 20% of the difference between max and min
         * (or the scale factor, whichever is largest) from the present value. Called whenever a message
         * is sent if the "Randomize" checkbox is checked.
         */
        private void Randomize()
        {
            Random rnd = new Random();
            for (int i = 0; i < signals.Count; i++ )
            {
                Signal s = signals[i];
                double interval = Math.Max(0.2 * (s.max - s.min), s.scale);
                double diff = rnd.NextDouble() * interval * 2 - interval;
                double rDiff = Math.Round(diff / s.scale) * s.scale;
                if (s.Value + rDiff > s.max || s.Value + rDiff < s.min)
                {
                    s.Value -= rDiff;
                }
                else
                {
                    s.Value += rDiff;
                }
                textBoxes[i].Text = s.Value.ToString();
            }
        }


        /*
         * Stops automatic transmission
         */
        private void stopTransmitButton_Click(object sender, RoutedEventArgs e)
        {
            autoTransmit = false;
            UpdateButtons();
        }

        /*
         * Closes the channel. Also stops any automatic transmission.
         */
        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            autoTransmit = false;
            channelOn = false;
            Canlib.canBusOff(chanHandle);
            Canlib.canClose(chanHandle);
            UpdateButtons();
        }


        /*
         * Updates the status bar, prints error message if something goes wrong
         * (for errors from Canlib calls)
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
         * Updates the status bar, prints error message if something goes wrong
         * (for errors in Kvadblib calls)
         */
        private void CheckDBStatus(String action, Kvadblib.Status status)
        {
            if (status != Kvadblib.Status.OK)
            {
                statusText.Text = action + " failed: " + status.ToString();
            }
            else
            {
                statusText.Text = action + " succeeded";
            }
        }

        /*
         * Updates the signal values whenever the textbox value is updated
         */
        private void UpdateSignal(object sender, TextChangedEventArgs e)
        {
            TextBox box = (TextBox)sender;
            int index = (int)box.Tag;
            double val = Double.Parse(box.Text);
            signals[index].Value = val;
        }

        /*
         * Makes sure only buttons that should be clicked are enabled
         */
        private void UpdateButtons()
        {
            loadMsgButton.IsEnabled = hasDb;
            initButton.IsEnabled = !channelOn;
            closeButton.IsEnabled = channelOn;
            sendMsgButton.IsEnabled = channelOn && hasMessage;
            startAutoButton.IsEnabled = channelOn && hasMessage && !autoTransmit;
            stopTransmitButton.IsEnabled = autoTransmit;
        }
        
    }

    /*
     * Used for controling the textboxes. All properties except the current value are 
     * effectively immutable.
     */
    class Signal
    {
        public string name { get; private set; }
        public double max { get; private set; }
        public double min { get; private set; }
        public double scale { get; private set; }
        private double val;
        public double Value
        {
            get { return val; }
            set
            {
                if(value >= min && value <= max)
                {
                    val = value;
                }
            }
        }

        public Signal(string name, double max, double min, double val, double scale)
        {
            this.name = name;
            this.max = max;
            this.min = min;
            this.val = val;
            this.scale = scale;
        }
    }
}
