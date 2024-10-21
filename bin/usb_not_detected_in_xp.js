/*
**                         Copyright 2002 by KVASER AB            
**                   P.O Box 4076 SE-51104 KINNAHULT, SWEDEN
**                          WWW: http://www.kvaser.com
**
** This software is furnished under a license and may be used and copied
** only in accordance with the terms of such license.
**
** Description:
**  JavaScript that applies a fix to MS KB Q314634.
**
**
** ---------------------------------------------------------------------------
*/

var shell = WScript.CreateObject("WScript.Shell");

var retval = shell.Popup(
"This script can enable or disable a fix for a problem " +
"in Microsoft Windows XP. See Microsoft Knowledgebase " +
"article id Q314634 for more details.\n" +
"You must install Servicepack 1 (or later) for Windows XP first.\n" +
"You may want to consult support@kvaser.com before applying this fix.\n\n" +
"Press Yes to enable the fix,\n" +
"No to disable it,\n" +
"or Cancel to quit without making any changes.", 
                        
                          0, "Windows XP USB Selective Suspend Fix", 3);

switch (retval) {
   case 6:
       // Yes
       shell.RegWrite ("HKLM\\SYSTEM\\CurrentControlSet\\Services\\Usb\\DisableSelectiveSuspend", 1, "REG_DWORD");
       break;
   case 7:
       // No
       shell.RegWrite ("HKLM\\SYSTEM\\CurrentControlSet\\Services\\Usb\\DisableSelectiveSuspend", 0, "REG_DWORD");
       break;
   default:
       break;
}
