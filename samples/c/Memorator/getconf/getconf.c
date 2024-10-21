/*
**                         Copyright 2009 by KVASER AB            
**                          WWW: http://www.kvaser.com
**
** This software is furnished under a license and may be used and copied
** only in accordance with the terms of such license.
**
** Description:
**  Example program for Kvmlib. List all logfiles on a Kvaser Memorator
**  Professional and then copy the configuration (param.lif) to current
**  directory.
** ---------------------------------------------------------------------------
*/

#include <stdio.h>
#include <stdlib.h>
#include <stdarg.h>
#include <io.h>
#include <time.h>

#include "kvmlib.h"
#include "kvaMemolib.h"


void usage(void) {
  printf("\nList all logfiles on Kvaser Memorator Professional and then\n");
  printf("copy the configuration (param.lif) to current directory.\n");
  printf("\n  getconf memoNr ");
  printf("\n  getconf memoNr [-ext]");
  printf("\n  getconf memoNr [-ext] -write  (*careful) \n\n");
}

int main(int argc, char **argv)
{
  
  int memoNr;
  kvmHandle h;
  kvmStatus status;
  uint32 fileCount;
  int64 eventCount;
  uint32 fileNo;
  time_t startTimeT;
  uint32 startTime;
  int format = 0, ext = 0, write = 0;
  char buf[230000]; // 7*64*512 = 229376 = param.lif
  size_t len;
  unsigned int size;
  FILE *fp;
  

  // Get the Card Number
  if (argc < 2) {
    usage();
    exit(0);
  }
  memoNr = atoi(argv[1]);
  
  // Initialize SD cards that are not formatted?
   if (argc == 3 && (strcmp(argv[2], "-format") == 0)) {
    format = 1;
   }
   if (argc >= 3 && (strcmp(argv[2], "-ext") == 0)) {
    ext = 1;
   }
   if (argc == 4 && (strcmp(argv[3], "-write") == 0)) {
    write = 1;
   }
  
  // Initialize the API
  // ------------------
  kvmInitialize();
  
  // Connect to a Memorator Professional
  // -----------------------------------
  printf("Connecting to a Memorator Professional with card number %d\n", memoNr);
  h = kvmDeviceOpen(memoNr, &status, ext ? kvmDEVICE_MHYDRA_EXT : kvmDEVICE_MHYDRA);
  
  if (status == MemoStatusERR_OCCUPIED) {
    printf("\nError: Only one program at the time can use the dll.\n");
    exit(1);
  }
  
  if (status == MemoStatusERR_NOT_FORMATTED) {
    printf("\nError: The SD Card is not initialized.\n");
    if (format) {
      printf("Formatting the SD Card. Please wait...");
      status = kvmDeviceFormatDisk(h, 1, 0, 0);
      printf("done.\n");
    }
    kvmClose(h);
    exit(1);
  }
  
  if (status != MemoStatusOK) {
    printf("\nError: Failed to connect to the device with card number %d. Error code: %d\n", memoNr, status);
    exit(1);
  }
  
  // List the contents of the SD memory card
  // ---------------------------------------
  printf("Counting log files...");
  status = kvmLogFileGetCount(h, &fileCount);
  if (status != MemoStatusOK) {
    printf("\nError: Could not count the logfiles. Error code: %d\n", status);
    kvmClose(h);
    exit(1);
  }
  printf("done.\n");
  
  printf("File\tEvents\tDate\n");
  for (fileNo=0; fileNo < fileCount;fileNo++) {
    status = kvmLogFileMountEx(h, fileNo, &eventCount);
    if (status != MemoStatusOK) {
      printf("\nError: Could not open logfile number %lu. Error code: %d\n",fileNo, status);
      kvmLogFileDismount(h);
      kvmClose(h);
      exit(1);
    }
    
    status = kvmLogFileGetStartTime(h, &startTime);
    if (status != MemoStatusOK) {
      printf("\nError: Could not get start time from logfile number %lu. Error code: %d\n",fileNo, status);
      kvmLogFileDismount(h);
      kvmClose(h);
      exit(1);
    }
    startTimeT = startTime;
    printf("%02d\t%llu\t%s", fileNo, eventCount, ctime(&startTimeT));
    kvmLogFileDismount(h);
  }

  // Get param.lif from the SD memory card
  // -------------------------------------
  status = kvmKmfReadConfig(h, buf, sizeof(buf), &len);
  if (status != MemoStatusOK) {
    printf("\nError: Could not read the configuration. Error code: %d\n", status);
    kvmClose(h);
    exit(1);
  }
  
  printf("\nWrite the buffer to a file with len=%d\n", (int)len);
  fp = fopen("param.lif", write ? "rb" : "wb");
  fseek (fp, 0, SEEK_END);
  size = ftell(fp);
  rewind(fp);
  
  if (write) printf("FileSize: %d \n", size);
  
  if (fp) {
    if (write)
      len = fread(buf, 1, sizeof(buf), fp);
    else
      fwrite(buf, 1, len, fp);
    fclose(fp);
  } else {
    printf("\nError: Could not create the file param.lif.\n");
    kvmClose(h);
    exit(1);
  }
  
  if (write) 
  {
    printf("\nWrite the file to device with len=%d %d", size, (int)len);
    status = kvmKmfWriteConfig(h, buf, size);
    printf("\nWrote device with len=%d", size);
  }
  if (status != MemoStatusOK) {
    printf("\nError: Could not write the configuration. Error code: %d\n", status);
    kvmClose(h);
    exit(1);
  }
  // Close the connection
  // --------------------
  kvmClose(h);
  return 0;
}
