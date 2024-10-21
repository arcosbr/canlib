/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html

** Program to read User Data
*/
#include <stdio.h>
#include <windows.h>
#include <canlib.h>

int main(int argc, char **argv)
{
  int i;
  canHandle hnd;
  canStatus stat;
  unsigned char buf[8];

  if (argc != 2) {
    printf("Usage: read_customer_data channel\n");
    exit(1);
  }

  canInitializeLibrary();
  hnd = canOpenChannel(atoi(argv[1]), 0);
  if (hnd < 0) {
    printf("Cannot open channel %s, error code %d\n", argv[1], hnd);
    exit(1);
  }

  stat = kvReadDeviceCustomerData(hnd,
                                  100 /*userNumber*/,
                                  0, /* reserved, MBZ */
                                  buf, sizeof(buf));

  if (stat != canOK) {
    printf("kvReadDeviceCustomerData returned error code %d\n", stat);
    exit(1);
  }

  printf("User data: ");
  for (i=0; i<(signed)sizeof(buf); i++) {
    printf("%02x ", buf[i]);
  }
  printf("\n");

  canClose(hnd);
  return 0;
}
