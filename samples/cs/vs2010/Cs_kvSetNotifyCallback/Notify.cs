using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Collections.Concurrent;
using Kvaser.CanLib;

namespace NotifyTest
{
    public class CanMsg
    {
        public int id;
        public int dlc;
        public int flags;
        public long time;
        public byte[] data;

        public CanMsg()
        {
            id    = 0;
            dlc   = 0;
            flags = 0;
            time  = 0;
            data  = new byte[8] {0, 0, 0, 0, 0, 0, 0, 0};
        }
    }

    public partial class Notify : Form
    {
        public Notify()
        {
            InitializeComponent();
            Canlib.canInitializeLibrary();

            WorkEvent = new AutoResetEvent(false);
            q = new ConcurrentQueue<CanMsg>();

            t = new Thread(Worker);
            t.Start();
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
        }

        private void DisplayError(Canlib.canStatus status, String routineName)
        {
            String errText = "";
            if (status != Canlib.canStatus.canOK)
            {
                Canlib.canGetErrorText(status, out errText);
                errText += ".\nError code = " + status.ToString() + ".";
                MessageBox.Show(errText, routineName, MessageBoxButtons.OK);
                Environment.Exit(0);
            }
        }

        delegate void AppendMessageCallback(string msg);

        private void AppendMessage(string msg)
        {
            // When calling from a different thread (in this case the Worker thread) we must 
            // invoke the main thread to update the control.
            if (RxMsgsTbox.InvokeRequired)
            {
                AppendMessageCallback d = new AppendMessageCallback(AppendMessage);
                this.Invoke(d, new object[] { msg });
            }
            else
            {
                RxMsgsTbox.AppendText(msg);
            }
        }

        private void DisplayMessage(CanMsg msg)
        {
            String s;
            if ((msg.flags & Canlib.canMSG_ERROR_FRAME) == Canlib.canMSG_ERROR_FRAME)
                s = String.Format("ErrorFrame                                          {0}", msg.time) + Environment.NewLine;
            else
            {
                s = String.Format("{0:x8} ", msg.id);
                if ((msg.flags & Canlib.canMSG_EXT) == Canlib.canMSG_EXT)
                    s += "X";
                else
                    s += " ";
                if ((msg.flags & Canlib.canMSG_RTR) == Canlib.canMSG_RTR)
                    s += "R";
                else
                    s += " ";
                if ((msg.flags & Canlib.canMSG_TXACK) == Canlib.canMSG_TXACK)
                    s += "A";
                else
                    s += " ";
                if ((msg.flags & Canlib.canMSG_WAKEUP) == Canlib.canMSG_WAKEUP)
                    s += "W";
                else
                    s += " ";
                s += String.Format("  {0:x1} ", msg.dlc);
                for (int i = 0; i < 8; i++)
                {
                    if (i < msg.dlc)
                        s += String.Format("  {0:x2}", msg.data[i]);
                    else
                        s += "    ";
                }
                s += String.Format("   {0}", msg.time) + Environment.NewLine;
            }
            AppendMessage(s);
        }

        private void ConfigChannel_Click(object sender, EventArgs e)
        {
            Canlib.canStatus status;

            if (buson == 0)
            {
                canHandle = Canlib.canOpenChannel(0, Canlib.canOPEN_ACCEPT_VIRTUAL);
                if (canHandle < 0)
                {
                    DisplayError((Canlib.canStatus)canHandle, "canOpenChannel");
                }

                status = Canlib.canSetBusParams(canHandle, Canlib.canBITRATE_250K, 0, 0, 0, 0, 0);
                DisplayError(status, "canSetBusParams");

                // The kvSetNotifyCallback() function registers a callback function which is called when certain events occur.
                // In this case we will get a CAN message reception notification.
                status = Canlib.kvSetNotifyCallback(canHandle, new Canlib.kvCallbackDelegate(Callback), new IntPtr(0), Canlib.canNOTIFY_RX);
                DisplayError(status, "kvSetNotifyCallback");

                status = Canlib.canBusOn(canHandle);
                DisplayError(status, "canBusOn");

                buson = 1;
            }
        }

        private void CloseChannel_Click(object sender, EventArgs e)
        {
            Canlib.canStatus status;

            if (buson == 1)
            {
                status = Canlib.kvSetNotifyCallback(canHandle, null, new IntPtr(0), Canlib.canNOTIFY_RX);
                DisplayError(status, "unregister kvSetNotifyCallback");

                status = Canlib.canBusOff(canHandle);
                DisplayError(status, "canBusOff");

                buson = 0;
                status = Canlib.canClose(canHandle);
                DisplayError(status, "canClose");
                canHandle = -1;
            }
        }

        private void Callback(int handle, IntPtr context, UInt32 notifyEvent)
        {
            // The callback function is called in the context of a high-priority thread created by CANLIB.
            // You should take precaution not to do any time consuming tasks in the callback.

            CanMsg msg = new CanMsg();

            // Empty the receive buffer and enqueue messages to the Worker thread.
            while (Canlib.canRead(handle, out msg.id, msg.data, out msg.dlc, out msg.flags, out msg.time) == Canlib.canStatus.canOK)
            {
                q.Enqueue(msg);
            }

            WorkEvent.Set();
        }

        private void Worker()
        {
            CanMsg msg;
            while (true)
            {
                WorkEvent.WaitOne();

                while (q.TryDequeue(out msg))
                {
                    DisplayMessage(msg);
                }
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            t.Abort();
        }

        private Thread t;
        private AutoResetEvent WorkEvent;
        private ConcurrentQueue<CanMsg> q;
        private int canHandle;
        private int buson = 0;
    }
}
