using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Kvaser.CanLib;

namespace NotifyTest
{
    public partial class Notify : Form
    {
        public Notify()
        {
            InitializeComponent();
            Canlib.canInitializeLibrary();
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

        private void DisplayMessage(int id, int dlc, byte[] data, int flags, long time)
        {
            String s;
            if ((flags & Canlib.canMSG_ERROR_FRAME) == Canlib.canMSG_ERROR_FRAME)
                s = String.Format("ErrorFrame                                          {0}", time) + Environment.NewLine;
            else
            {
                s = String.Format("{0:x8} ", id);
                if ((flags & Canlib.canMSG_EXT) == Canlib.canMSG_EXT)
                    s += "X";
                else
                    s += " ";
                if ((flags & Canlib.canMSG_RTR) == Canlib.canMSG_RTR)
                    s += "R";
                else
                    s += " ";
                if ((flags & Canlib.canMSG_TXACK) == Canlib.canMSG_TXACK)
                    s += "A";
                else
                    s += " ";
                if ((flags & Canlib.canMSG_WAKEUP) == Canlib.canMSG_WAKEUP)
                    s += "W";
                else
                    s += " ";
                s += String.Format("  {0:x1} ", dlc);
                for (int i = 0; i < 8; i++)
                {
                    if (i < dlc)
                        s += String.Format("  {0:x2}", data[i]);
                    else
                        s += "    ";
                }
                s += String.Format("   {0}", time) + Environment.NewLine;
            }
            RxMsgsTbox.AppendText(s);
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

                // The canSetNotify() function associates a window handle with the CAN circuit.
                // A WM__CANLIB message is sent to that window when certain events (specified by the canNOTIFY_xxx flags) occur.
                // In this case we will get a CAN message reception notification.
                status = Canlib.canSetNotify(canHandle, Handle, Canlib.canNOTIFY_RX);
                DisplayError(status, "canSetNotify");

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
                status = Canlib.canSetNotify(canHandle, (System.IntPtr)null, Canlib.canNOTIFY_RX);
                DisplayError(status, "unregister canSetNotify");

                status = Canlib.canBusOff(canHandle);
                DisplayError(status, "canBusOff");

                buson = 0;
            }
            status = Canlib.canClose(canHandle);
            DisplayError(status, "canClose");
            canHandle = -1;
        }

        protected override void WndProc(ref Message m)
        {
            Canlib.canStatus status;

            if (m.Msg == Canlib.WM__CANLIB)
            {
                // You can check m.LParam low word for the notification type

                // regardless of notification type you must empty the receive buffer
                int id = 0;
                byte[] data = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
                int dlc = 0;
                int flag = 0;
                long time = 0;

                // empty the receive buffer.
                while ((status = Canlib.canRead(canHandle, out id, data, out dlc, out flag, out time))
                        == Canlib.canStatus.canOK)
                {
                    DisplayMessage(id, dlc, data, flag, time);
                }

                if (status != Canlib.canStatus.canERR_NOMSG)
                {
                    // an error communicating with the hardware detected so shutdown
                    Canlib.canBusOff(canHandle);
                    Canlib.canClose(canHandle);
                    canHandle = -1;
                    DisplayError(status, "canRead");
                }
            }

            base.WndProc(ref m);
        }

        private int canHandle;
        private int buson = 0;
    }
}
