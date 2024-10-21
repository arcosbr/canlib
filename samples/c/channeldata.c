/*
**                   Copyright 2007-2016 by KVASER AB, SWEDEN
**                        WWW: http://www.kvaser.com
**
** This software is furnished under a license and may be used and copied
** only in accordance with the terms of such license.
**
** Description:
** This sample program enumerates all CAN channels in the computer and displays
** some information about them, such as hardware type, serial number, firmware revision.
** ---------------------------------------------------------------------------
*/
#include <stdlib.h>
#include <stdio.h>
#include <memory.h>
#include <windows.h>
#include <conio.h>

#include <canlib.h>

#ifdef __BORLANDC__
#pragma argsused
#endif


char CANCHANNEL_OPMODE_TEXT[5][20] = {"Unknown","None","Infrastructure","Reserved","AdHoc"};


void Usage(int argc, char* argv[])
{
  printf("\nCANLIB Channel Enumeration & Data\n");
  printf("\nA Part of the CANLIB SDK - see www.kvaser.com for more info.\n");
  printf("\n");
  printf("\nUsage: channeldata \n");

  exit(1);
}


void Check(char* id, canStatus stat)
{
  if (stat != canOK) {
    char buf[50];
    buf[0] = '\0';
    canGetErrorText(stat, buf, sizeof(buf));
    printf("%s: failed, stat=%d (%s)\n", id, (int)stat, buf);
  }
}

void main(int argc, char* argv[])
{
  canStatus stat;
  int i, chanCount;

  printf("Starting...\n");

  canInitializeLibrary();

  stat = canGetNumberOfChannels(&chanCount);
  Check("canGetNumberOfChannels", stat);
  if (chanCount<0 || chanCount > 1000) {
    printf("ChannelCount = %d but I don't believe it.\n", chanCount);
    exit(1);
  } else {
    printf("%d channels.\n", chanCount);
  }

  for (i=0; i<chanCount; i++) {
    DWORD tmp;
    DWORD quad[2];
    DWORD64 quad64[2];
    char name[64];
    unsigned long ean[2];
    canHandle hnd;
    DWORD flags;
    DWORD  drivertype;
    char cust_channel_name[64];

    printf("== Channel %d ===============================\n", i);

    stat = canGetChannelData(i, canCHANNELDATA_CHANNEL_CAP, &tmp, sizeof(tmp));
    Check("canGetChannelData", stat);
    stat = canGetChannelData(i, canCHANNELDATA_CHANNEL_CAP_EX, &quad64, sizeof(quad64));
    Check("canGetChannelData", stat);
    printf("Channel Capabilities Ex =  0x%I64x ", quad64[0]);

    printf("Channel Capabilities =  0x%08lx ", tmp);
    if (tmp & canCHANNEL_CAP_EXTENDED_CAN) printf("Ext ");
    if (tmp & canCHANNEL_CAP_BUS_STATISTICS) printf("Stat ");
    if (tmp & canCHANNEL_CAP_ERROR_COUNTERS) printf("ErrCnt ");
    if (tmp & canCHANNEL_CAP_CAN_DIAGNOSTICS) printf("Diag ");
    if (tmp & canCHANNEL_CAP_GENERATE_ERROR) printf("ErrGen ");
    if (tmp & canCHANNEL_CAP_GENERATE_OVERLOAD) printf("OvlGen ");
    if (tmp & canCHANNEL_CAP_TXREQUEST) printf("TxRq ");
    if (tmp & canCHANNEL_CAP_TXACKNOWLEDGE) printf("TxAck ");
    if (tmp & canCHANNEL_CAP_VIRTUAL) printf("Virt ");
    if (tmp & canCHANNEL_CAP_SIMULATED) printf("Simulated ");
    if (tmp & canCHANNEL_CAP_REMOTE) printf("Remote ");
    if (tmp & canCHANNEL_CAP_CAN_FD) printf("Fd ");
    if (tmp & canCHANNEL_CAP_CAN_FD_NONISO) printf("Non-ISO ");
    if (tmp & canCHANNEL_CAP_SILENT_MODE) printf("Silent ");
    if (tmp & canCHANNEL_CAP_SINGLE_SHOT) printf("Single-shot ");
    if (tmp & canCHANNEL_CAP_CANTEGRITY) printf("CANtegrity ");
    if (tmp & canCHANNEL_CAP_IO_API) printf("IoApi ");

    if (quad64[0] & canCHANNEL_CAP_EX_BUSPARAMS_TQ) printf ("BusParamsTQ ");
    printf("\n");

    stat = canGetChannelData(i, canCHANNELDATA_TRANS_CAP, &tmp, sizeof(tmp));
    Check("canGetChannelData", stat);
    printf("DRVcan Capabilities =   0x%08lx ", tmp);
    if (tmp & canDRIVER_CAP_HIGHSPEED) printf("HiSpd ");
    printf("\n");

    stat = canGetChannelData(i, canCHANNELDATA_CHANNEL_FLAGS, &tmp, sizeof(tmp));
    Check("canGetChannelData", stat);
    printf("Channel Flags =         0x%08lx ", tmp);
    if (tmp & canCHANNEL_IS_OPEN)  printf("Open ");
    if (tmp & canCHANNEL_IS_CANFD) printf("as canFD ");
    printf("\n");
    
    if (tmp & canCHANNEL_IS_CANFD)
      hnd = canOpenChannel(i, canOPEN_NO_INIT_ACCESS + canOPEN_OVERRIDE_EXCLUSIVE + canOPEN_ACCEPT_VIRTUAL + canOPEN_CAN_FD);
    else
      hnd = canOpenChannel(i, canOPEN_NO_INIT_ACCESS + canOPEN_OVERRIDE_EXCLUSIVE + canOPEN_ACCEPT_VIRTUAL);

    stat = canReadStatus(hnd, &flags);
    Check("canReadStatus", stat);
    printf("Channel Status Flags =  0x%08lx ", flags);
    if (flags & canSTAT_BUS_OFF) printf("BusOff ");
    printf("\n");

    stat = canGetBusOutputControl(hnd, &drivertype);
    Check("canGetBusOutputControl", stat);
    printf("Drivertype =            0x%08lx ", drivertype);
    canClose(hnd);
    if (drivertype & canDRIVER_SILENT) 
      printf("Silent ");
    else
      printf("Normal ");
    printf("\n");
    
    stat = canGetChannelData(i, canCHANNELDATA_BUS_TYPE, &tmp, sizeof(tmp));
    Check("canCHANNELDATA_BUS_TYPE", stat);
    printf("Bustype      =          0x%08lx\n", tmp);

    stat = canGetChannelData(i, canCHANNELDATA_CARD_NUMBER, &tmp, sizeof(tmp));
    Check("canCHANNELDATA_CARD_NUMBER", stat);
    printf("Board Number =          0x%08lx\n", tmp);

    stat = canGetChannelData(i, canCHANNELDATA_CHAN_NO_ON_CARD, &tmp, sizeof(tmp));
    Check("canCHANNELDATA_CHAN_NO_ON_CARD", stat);
    printf("Channel no on board =   0x%08lx\n", tmp);

    stat = canGetChannelData(i, canCHANNELDATA_CARD_SERIAL_NO, quad, sizeof(quad));
    Check("canCHANNELDATA_CARD_SERIAL_NO", stat);
    printf("Board S/N =             0x%08lx 0x%08lx\n", quad[0], quad[1]);

    stat = canGetChannelData(i, canCHANNELDATA_TRANS_SERIAL_NO, quad, sizeof(quad));
    Check("canCHANNELDATA_TRANS_SERIAL_NO", stat);
    printf("DRVcan S/N =            0x%08lx 0x%08lx\n", quad[0], quad[1]);

    stat = canGetChannelData(i, canCHANNELDATA_CARD_FIRMWARE_REV, quad, sizeof(quad));
    Check("canCHANNELDATA_CARD_FIRMWARE_REV", stat);
    printf("Board F/W version =     0x%08lx 0x%08lx\n", quad[0], quad[1]);

    stat = canGetChannelData(i, canCHANNELDATA_CARD_HARDWARE_REV, quad, sizeof(quad));
    Check("canCHANNELDATA_CARD_HARDWARE_REV", stat);
    printf("Board H/W version =     0x%08lx 0x%08lx\n", quad[0], quad[1]);

    stat = canGetChannelData(i, canCHANNELDATA_CARD_UPC_NO, ean, sizeof(ean));
    Check("canCHANNELDATA_CARD_UPC_NO", stat);
    printf("Board UPC/EAN =         0x%08lx 0x%08lx\n", ean[1], ean[0]);

    stat = canGetChannelData(i, canCHANNELDATA_TRANS_UPC_NO, ean, sizeof(ean));
    Check("canCHANNELDATA_TRANS_UPC_NO", stat);
    printf("DRVcan UPC/EAN =        0x%08lx 0x%08lx\n", ean[1], ean[0]);

    stat = canGetChannelData(i, canCHANNELDATA_CHANNEL_NAME, name, sizeof(name));
    Check("canCHANNELDATA_CHANNEL_NAME", stat);
    printf("Channel name =          '%s'\n", name);

    stat = canGetChannelData(i, canCHANNELDATA_CUST_CHANNEL_NAME, cust_channel_name, 
                             sizeof(cust_channel_name));
    if (stat == canOK) {
      printf("Cust. Channel name =    '%s'\n", cust_channel_name);
    }

    stat = canGetChannelData(i, canCHANNELDATA_REMOTE_OPERATIONAL_MODE, &tmp, sizeof(tmp));
    if ((stat != canERR_NOT_IMPLEMENTED) && (stat != canERR_HARDWARE)) {
      Check("canCHANNELDATA_REMOTE_OPERATIONAL_MODE", stat);
      printf("Operational mode =      %lu (%s)\n", tmp, CANCHANNEL_OPMODE_TEXT[tmp]);

      stat = canGetChannelData(i, canCHANNELDATA_REMOTE_PROFILE_NAME, name, sizeof(name));
      Check("canCHANNELDATA_REMOTE_PROFILE_NAME", stat);
      printf("Profile name =          '%s'\n", name);

      stat = canGetChannelData(i, canCHANNELDATA_REMOTE_HOST_NAME, name, sizeof(name));
      Check("canCHANNELDATA_REMOTE_HOST_NAME", stat);
      printf("Remote host name =      '%s'\n", name);

      stat = canGetChannelData(i, canCHANNELDATA_REMOTE_MAC, name, sizeof(name));
      Check("canCHANNELDATA_REMOTE_MAC", stat);
      printf("MAC =                   '%s'\n", name);
    } else {
      printf("Device is not capable of acting in remote operational mode\n");
    }   
    
    printf("\n");
  }
}
