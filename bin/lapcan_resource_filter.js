/*
**                   Copyright 2010 by KVASER AB, SWEDEN      
**                        WWW: http://www.kvaser.com
**
** This software is furnished under a license and may be used and copied
** only in accordance with the terms of such license.
**
** Description:
**  JavaScript that can turn on or off resource filtering for the LAPcan driver
**  (4.4 or later).
**
**  The driver uses resource filtering to ensure the I/O address lies between
**  0 and 0x3FF.
**
** Note: drivers 3.3-4.3 used a registry entry
** "LAPcanNoAddressRestriction" with the opposite meaning.
** ---------------------------------------------------------------------------
*/

var shell = WScript.CreateObject("WScript.Shell");

var retval = shell.Popup(
                         "This script can enable or disable 'resource filtering' for " +
                         "the LAPcan driver. Resource filtering is normally disabled.\n" +
                         "\n" +
                         "Please consult support@kvaser.com before changing the " +
                         "resource filtering.\n" +
                         "\n" +
                         "Press Yes to enable resource filtering, " +
                         "No to disable it, or Cancel to quit " +
                         "without changing the current value.", 
                        
                          0, "LAPcan Resource Filtering", 3);

switch (retval) {
   case 6:
       // Yes
       shell.RegDelete("HKLM\\SYSTEM\\CurrentControlSet\\Services\\KvaserCommonOptions\\Parameters\\LAPcanNoAddressRestriction");
       shell.RegWrite ("HKLM\\SYSTEM\\CurrentControlSet\\Services\\KvaserCommonOptions\\Parameters\\LAPcanApplyAddressRestriction", 1, "REG_DWORD");
       break;
   case 7:
       // No
       shell.RegDelete("HKLM\\SYSTEM\\CurrentControlSet\\Services\\KvaserCommonOptions\\Parameters\\LAPcanNoAddressRestriction");
       shell.RegDelete("HKLM\\SYSTEM\\CurrentControlSet\\Services\\KvaserCommonOptions\\Parameters\\LAPcanApplyAddressRestriction");
       break;
   default:
       break;
}

