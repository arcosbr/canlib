================================================================================
                             Kvaser Drivers
                               Wed Sep 18 21:26:07 2024
                     Version: canlib 5.46
================================================================================

These scripts and programs can be useful when troubleshooting a driver
installation.


IsaToPciRouting-Readme.txt
IsaToPciRoutingDisable.reg
IsaToPciRoutingEnable.reg

Relevant for LAPcan/LAPcan II used together with PCMCIA-PCI adapters
on newer computers.
The scripts can disable or enable ISA to PCI interrupt routing. Read
IsaToPciRouting-Readme.txt for more information.


kcanc_enable_disable.js

Run this script if you need to enable the kcanc.sys driver. It was used
for Kvaser Creator and is normally disabled.



lapcan_resource_filter.js

Run this script to enable or disable resource filtering for LAPcan or LAPcan II.
This can be useful in certain hardware configurations when Windows assign
a I/O address that the card can't use.


regmaint.exe

A program used to tweak certain registry settings for the drivers.


usb_not_detected_in_xp.js

Run this script to enable/disable USB selective suspend.

