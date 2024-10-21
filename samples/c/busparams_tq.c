/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html

** Description:
** Set bus parameters using setBusParamsTq and setBusParamsFdTq read them back
*/

#include <canlib.h>
#include <stdio.h>


static void check(char* id, canStatus stat)
{
  if (stat != canOK) {
    char buf[50];
    buf[0] = '\0';
    canGetErrorText(stat, buf, sizeof(buf));
    printf("%s: failed, stat=%d (%s)\n", id, (int)stat, buf);
  }
}

int main(int argc, char *argv[])
{
  canStatus stat = -1;
  canHandle hndCan, hndCanFd;

  // Structs for bus parameters
  kvBusParamsTq busParams = {8, 2, 2, 1, 3, 20};
  kvBusParamsTq busParamsReturn;

  kvBusParamsTq arbitration = {40, 8, 8, 8, 23, 2};
  kvBusParamsTq data = {20, 15, 4, 4, 0, 2};
  kvBusParamsTq arbitrationReturn;
  kvBusParamsTq dataReturn;

  canInitializeLibrary();

  // Open first channel in classic CAN mode
  hndCan = canOpenChannel(0, 0);
  if ((canStatus)hndCan < 0) {
      printf("canOpenChannel in classic CAN mode failed with status: (%d)\n", (canStatus)hndCan);
      return -1;
  }

  // Set bus parameters
  stat = canSetBusParamsTq(hndCan, busParams);
  check("canSetBusParamsTq", stat);

  // Open second channel in CAN FD mode
  hndCanFd = canOpenChannel(1, canOPEN_CAN_FD);
  if ((canStatus)hndCanFd < 0) {
      printf("canOpenChannel in CAN FD mode failed with status: (%d)\n", (canStatus)hndCanFd);
      return -1;
  }

  // Set bus parameters
  stat = canSetBusParamsFdTq(hndCanFd, arbitration, data);
  check("canSetBusParamsTq", stat);

  // Read classic CAN bus parameters back
  stat = canGetBusParamsTq(hndCan, &busParamsReturn);
  check("canGetBusParamsTq", stat);
  printf("\nClassic CAN: tq %u, phase1 %u, phase2 %u, sjw %u, prop %u, prescaler %u\n",
          busParamsReturn.tq, busParamsReturn.phase1, busParamsReturn.phase2,
          busParamsReturn.sjw, busParamsReturn.prop, busParamsReturn.prescaler);

  // Read CAN FD bus parameters back
  stat = canGetBusParamsFdTq(hndCanFd, &arbitrationReturn, &dataReturn);
  check("canGetBusParamsFdTq", stat);
  printf("\nArbitration phase: tq %u, phase1 %u, phase2 %u, sjw %u, prop %u, prescaler %u\n",
          arbitrationReturn.tq, arbitrationReturn.phase1, arbitrationReturn.phase2,
          arbitrationReturn.sjw, arbitrationReturn.prop, arbitrationReturn.prescaler);

  printf("\nData phase: tq %u, phase1 %u, phase2 %u, sjw %u, prop %u, prescaler %u\n",
          dataReturn.tq, dataReturn.phase1, dataReturn.phase2,
          dataReturn.sjw, dataReturn.prop, dataReturn.prescaler);

  canClose(hndCan);
  canClose(hndCanFd);

  stat = canUnloadLibrary();
  check("canUnloadLibrary", stat);

  return 0;
}
