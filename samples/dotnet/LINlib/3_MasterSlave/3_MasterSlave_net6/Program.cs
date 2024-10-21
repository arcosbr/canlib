/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html
*/

using Kvaser.LinLib;

namespace _3_MasterSlave_net6;

internal class Program
{
    //Channel numbers for the devices
    private static readonly int MASTER_CHANNEL = 0;
    private static readonly int SLAVE_CHANNEL = 1;

    static void Main(string[] args)
    {
        //Initializing and setting up the channels
        Linlib.linInitializeLibrary();

        Int32 master = Linlib.linOpenChannel(MASTER_CHANNEL, Linlib.LIN_MASTER);
        Int32 slave = Linlib.linOpenChannel(SLAVE_CHANNEL, Linlib.LIN_SLAVE);

        Linlib.linSetBitrate(master, 20000);
        Linlib.linSetBitrate(slave, 20000);

        Linlib.linBusOn(master);
        Linlib.linBusOn(slave);

        Console.WriteLine("Running with master device on channel {0} and slave on chanel {1}",
             MASTER_CHANNEL, SLAVE_CHANNEL);


        //Writing and reading messages
        Console.WriteLine("Sending some messages from master to slave...");
        for (int i = 0; i < 5; i++)
        {
            byte[] message = new byte[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
            Linlib.linWriteMessage(master, 100 + (int)i, message, 8);
        }
        Linlib.linWriteSync(master, 50);

        Console.WriteLine("Reading messages from slave: ");
        PrintAllMessages(slave);

        Console.WriteLine("Reading messages on master: ");
        PrintAllMessages(master);


        //Using the slave's response function
        Console.WriteLine("Adding a response to the slave...");
        Linlib.linUpdateMessage(slave, 123, new byte[8] { 8, 7, 6, 5, 4, 3, 2, 1 }, 8);

        Linlib.linRequestMessage(master, 123);

        Console.WriteLine("Reading messages from slave: ");
        PrintAllMessages(slave);

        Console.WriteLine("Reading messages on master: ");
        PrintAllMessages(master);


        //Sending wakeup messages
        Console.WriteLine("Sending 10 wakeup frames to slave, 500 ms interval");

        Linlib.linWriteWakeup(master, 10, 500);

        for (int i = 0; i < 60; i++)
        {
            PrintAllMessages(slave);
            Thread.Sleep(100);
        }

        Console.WriteLine("Press any key to exit");
        Console.ReadKey();
    }

    //Prints all received messages on a channel
    private static void PrintAllMessages(Int32 handle)
    {
        Linlib.LinStatus status = Linlib.LinStatus.linOK;
        int id = 0;
        byte[] msg = new byte[8];
        int dlc = 0;
        int flags = 0;
        Linlib.LinMessageInfo msgInfo = new Linlib.LinMessageInfo();

        while (status == Linlib.LinStatus.linOK)
        {
            status = Linlib.linReadMessage(handle, out id, msg, out dlc, out flags, msgInfo);
            if (status == Linlib.LinStatus.linOK)
            {
                PrintMessage(id, msg, dlc, flags, msgInfo);
            }
        }
        if (status != Linlib.LinStatus.linERR_NOMSG)
        {
            Console.WriteLine("Error when reading message");
        }
    }

    //Prints an incoming message
    private static void PrintMessage(int id, byte[] data, int dlc, int flags, Linlib.LinMessageInfo msgInfo)
    {
        //Check if the message was transmitted by us or received
        if ((flags & Linlib.LIN_TX) != 0)
        {
            Console.Write("TX: ");
        }
        else if ((flags & Linlib.LIN_RX) != 0)
        {
            Console.Write("RX: ");
        }
        //Check for error or wakeup frames
        if ((flags & (Linlib.LIN_CSUM_ERROR | Linlib.LIN_PARITY_ERROR | Linlib.LIN_BIT_ERROR | Linlib.LIN_SYNCH_ERROR)) != 0)
        {
            Console.WriteLine("***ERROR FRAME RECEIVED***");
        }
        else if ((flags & Linlib.LIN_WAKEUP_FRAME) != 0)
        {
            Console.WriteLine("***WAKEUP FRAME RECEIVED***");
        }
        //Print any regular messages
        else
        {
            Console.WriteLine("  {0}  {1}  {2:x2} {3:x2} {4:x2} {5:x2} {6:x2} {7:x2} {8:x2} {9:x2}    {10}",
                                                 id, dlc, data[0], data[1], data[2], data[3], data[4],
                                                 data[5], data[6], data[7], msgInfo.timestamp);
        }
    }

    //Displays an error message if the status is not OK
    private static void CheckStatus(Linlib.LinStatus status, string action)
    {
        if (status < 0)
        {
            Console.WriteLine("{0} failed, status {1}", action, status);
        }
    }
}
