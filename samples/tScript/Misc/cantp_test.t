void txIndication(int handle);
void rxIndication(int handle);
void ffIndication(int handle);//implement those?

#include <cantp.inc>

variables {
  const int can_channel = 1;
  int handle;
}

void txIndication(int handle) {
  printf("Transmit complete on tx-id: 0x%x\n", canTpSessions[handle].tx_id);
}

void rxIndication(int handle) {
  printf("Incoming message on rx-id: 0x%x\n", canTpSessions[handle].rx_id);
}

void ffIndication(int handle) {
  printf("First frame indication on rx-id: 0x%x\n", canTpSessions[handle].rx_id);
}

on start {
  byte SomeMessage[4095];
  canTpInit();
  if (canTpOpen(&handle, 0x0708, 0x0700, "ISO-15765") != RET_CANTP_NO_ERR) {
    printf("Error opening cantp handle\n");
    return;
  }
  canTpSetAttr(handle, CANTP_ISO15765_CHANNEL, can_channel);
  canTpSetAttr(handle, CANTP_ISO15765_STMIN, 5);
  canTpSetAttr(handle, CANTP_ISO15765_TX_TIMEOUT, 100);
  canSetBitrate(can_channel, canBITRATE_1M);
  canBusOn(can_channel);
  canTpTransmit(handle, SomeMessage);
}

on stop {
  canTpClose(handle);
  canBusOff();
}