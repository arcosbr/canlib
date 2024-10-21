/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kvaser.CanLib;

namespace _5_EnvVar_net481
{
    internal class Program
    {
        private static int channel = 0;
        private static int slot = 0;
        private static int handle = -1;
        private static string scriptname = @"..\..\..\env_demo.txe";

        static void Main(string[] args)
        {
            Canlib.canInitializeLibrary();

            if (args.Length > 0)
            {
                channel = Int32.Parse(args[0]);
            }
            if (args.Length > 1)
            {
                slot = Int32.Parse(args[1]);
            }
            Console.WriteLine("Channel: {0}, slot: {1}", channel, slot);
            handle = Canlib.canOpenChannel(channel, 0);
            CheckStatus((Canlib.canStatus)handle);
            CheckStatus(Canlib.kvScriptLoadFile(handle, slot, scriptname));
            CheckStatus(Canlib.kvScriptStart(handle, slot));

            Console.WriteLine("This is a demo of environment values");
            Console.WriteLine("Usage: type \"t <string>\" to set text");
            Console.WriteLine("            \"v <float>\" to set float value");
            Console.WriteLine("            \"p\" to print values");
            Console.WriteLine("            \"q\" to quit");

            bool loop = true;
            while (loop)
            {
                string input = Console.ReadLine();
                string[] tokens = input.Split(' ');

                switch (tokens[0].ToLower())
                {
                    case "t":
                        if (tokens.Length < 2)
                        {
                            Console.WriteLine("Invalid command: no new value supplied");
                        }
                        else
                        {
                            UpdateText(tokens[1]);
                        }
                        break;
                    case "v":
                        if (tokens.Length < 2)
                        {
                            Console.WriteLine("Invalid command: no new value supplied");
                        }
                        else
                        {
                            float newVal;
                            bool hasValue = float.TryParse(tokens[1], out newVal);
                            if (hasValue)
                            {
                                UpdateValue(newVal);
                            }
                            else
                            {
                                Console.WriteLine("Invalid value, must be float");
                            }
                        }
                        break;
                    case "p":
                        PrintEnvVars();
                        break;
                    case "q":
                        loop = false;
                        break;
                    default:
                        break;
                }
            }
            Console.WriteLine("Quitting program");
            Canlib.kvScriptStop(handle, slot, Canlib.kvSCRIPT_STOP_NORMAL);
            Canlib.canClose(handle);
            Canlib.canUnloadLibrary();

        }

        private static void UpdateText(string text)
        {
            int size, type;
            long envHandle = Canlib.kvScriptEnvvarOpen(handle, "text", out type, out size);
            byte[] bytes = new byte[size];
            byte[] input = Encoding.UTF8.GetBytes(text);
            if (input.Length <= size)
            {
                Array.Copy(input, bytes, input.Length);
                CheckStatus(Canlib.kvScriptEnvvarSetData(envHandle, bytes, 0, size));
                Console.WriteLine("Updated text");
            }
            else
            {
                Console.WriteLine("Input too large, max {0} characters allowed", size);
            }
            CheckStatus(Canlib.kvScriptEnvvarClose(envHandle));
        }

        private static void UpdateValue(float value)
        {
            int size, type;
            long envHandle = Canlib.kvScriptEnvvarOpen(handle, "value", out type, out size);
            CheckStatus(Canlib.kvScriptEnvvarSetFloat(envHandle, value));
            CheckStatus(Canlib.kvScriptEnvvarClose(envHandle));
            Console.WriteLine("Updated value");
        }

        private static void PrintEnvVars()
        {
            Console.WriteLine("Printing environment variables: ");
            int size, type;
            long envHandle = Canlib.kvScriptEnvvarOpen(handle, "text", out type, out size);
            byte[] bytes = new byte[size];
            Canlib.kvScriptEnvvarGetData(envHandle, out bytes, 0, size);
            string text = Encoding.UTF8.GetString(bytes);
            //Remove trailing null characters
            text = text.Split('\0')[0];
            Console.WriteLine("Text : " + text);
            Canlib.kvScriptEnvvarClose(envHandle);

            float value;
            envHandle = Canlib.kvScriptEnvvarOpen(handle, "value", out size, out type);
            Canlib.kvScriptEnvvarGetFloat(envHandle, out value);
            Console.WriteLine("Value: " + value);
            Canlib.kvScriptEnvvarClose(envHandle);

            int counter;
            envHandle = Canlib.kvScriptEnvvarOpen(handle, "counter", out size, out type);
            Canlib.kvScriptEnvvarGetInt(envHandle, out counter);
            Console.WriteLine("Counter: " + counter);
            Canlib.kvScriptEnvvarClose(envHandle);
        }

        private static void CheckStatus(Canlib.canStatus status)
        {
            if (status < 0)
            {
                string errorText;
                Canlib.canGetErrorText(status, out errorText);
                Console.WriteLine(errorText);
            }
        }
    }
}
