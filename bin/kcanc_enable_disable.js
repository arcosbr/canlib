/*
**                         Copyright 2002 by KVASER AB            
**                   P.O Box 4076 SE-51104 KINNAHULT, SWEDEN
**                          WWW: http://www.kvaser.com
**
** This software is furnished under a license and may be used and copied
** only in accordance with the terms of such license.
**
** Description:
**  JavaScript that can turn on or off the "Simulate CAN bus for Kvaser
**  Creator"
**
**  This driver was enabled by default in CANLIB 3.3? - 3.5.
** ---------------------------------------------------------------------------
*/

var shell = WScript.CreateObject("WScript.Shell");

var retval = shell.Popup(
"This script can enable or disable the device driver " +
"for the simulated CAN bus used by Kvaser Creator." +
"\n" +
"\n" +
"It is normally disabled. " +
"\n" +
"\n" +
"Enable it only if you are using Kvaser Creator." +
"If in doubt, please consult support@kvaser.com." +
"\n" +
"\n" +
"Press Yes to enable the device driver, " +
"No to disable it, or Cancel to quit " +
"without changing the current value.", 
                        
                          0, "Simulated CAN bus enable/disable", 3);

switch (retval) {
   case 6:
       // Yes, enable
       shell.RegWrite ("HKLM\\Software\\KVASER AB\\CANdriver 1.0\\Drivers\\kcanc\\Ignore", 0, "REG_DWORD");
       break;
   case 7:
       // No, disable
       shell.RegWrite ("HKLM\\Software\\KVASER AB\\CANdriver 1.0\\Drivers\\kcanc\\Ignore", 1, "REG_DWORD");
       break;
   default:
       break;
}
