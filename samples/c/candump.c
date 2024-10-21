/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html

** Description:
** This is a sample console program that dumps the traffic on the CAN bus on
** the screen, or to a file. The purpuse of this program is to demonstrate
** certain programming techniques. It may or may not be useful to you.
**
*/

// First, include the whole known universe.
#include <stdlib.h>
#include <stdio.h>
#include <memory.h>
#include <windows.h>
#include <conio.h>
#include <time.h>

// CANLIB requires the following #include.
#include <canlib.h>

static void Usage(void)
{
  printf("\nCANLIB Dump Program\n");
  printf("(Part of the CANLIB SDK from KVASER AB - http://www.kvaser.se)\n");
  printf("\n");
  printf("This is a sample program that just dumps all incoming messages on the screen,\n");
  printf("or to a file.\n");
  printf("\nUsage: candump [flags] [filename]\n");
  printf("   -X          Listen to CAN channel number X.\n");
  printf("   -B<value>   Set the bitrate. Value is any of 1000,500,250,125.\n");
  printf("   -A<value>   Set the CAN FD Arbitration bitrate. Value is any of 500,1000.\n");
  printf("   -D<value>   Set the CAN FD Data bitrate. Value is any of 1000,2000,4000,8000.\n");
  printf("               Default bitrate is 125 kbit/s.\n");
  printf("   -h          Print this help text.\n");
  printf("   -q          Be more quiet than usual.\n");
  printf("   -fd         Use CAN FD.\n");
  printf("   -virtual    Accept virtual channels.\n");
  printf("\nIf no filename is specified, standard output is used.\n\n");
  printf("The following keys can be used during logging:\n");
  printf("   ESC         Stop logging.\n");
  printf("   Q           Be more quiet.\n");
  printf("   q           Be less quiet.\n");
  printf("\nExample:\n");
  printf("candump -B250 -0 logfile.log\n");
  printf("   would set CAN channel 0 to 250 kbit/s and log to logfile.log\n");

  exit(1);
}

//
// Global variables for the command-line options.
//
int  Bitrate = 0;          // Selected bit rate.
int  BitrateFdA = 0;       // Selected CAN FD Arbritation bit rate.
int  BitrateFdD = 0;       // Selected CAN FD Data bit rate.
int  Source = 0;           // Channel #
char Filename[512];        // Name of log file
int  Quiet = 0;            // Defines the verbosity.
int  can_open_flags = 0;   // Flags for canOpen, e.g. set CAN FD.

//
// Check a status code and issue an error message if the code isn't canOK.
//
void ErrorExit(char* id, canStatus stat)
{
  if (stat != canOK) {
    char buf[50];
    buf[0] = '\0';
    canGetErrorText(stat, buf, sizeof(buf));
    fprintf(stderr, "%s: failed, stat=%d (%s)\n", id, (int)stat, buf);
    exit(1);
  }
}

//
// Setup a CAN controller.
//
int InitCtrl(int ctrl)
{
  canStatus stat;
  canHandle hnd;

  //
  // First, open a handle to the CAN circuit. (We could specify
  // canOPEN_EXCLUSIVE to ensure we get a circuit that noone else
  // is using, if desired.)
  //
  hnd = canOpenChannel(ctrl, can_open_flags);
  if (hnd < 0) {
    ErrorExit("canOpenChannel", (canStatus)hnd);
  }

  //
  // Using our new shiny handle, we specify the baud rate
  // using one of the convenient canBITRATE_xxx constants.
  //
  // The bit layout is in depth discussed in most CAN
  // controller data sheets, and on the web at
  // http://www.kvaser.se.
  //

  if (Bitrate != 0) {
    stat = canSetBusParams(hnd, Bitrate, 0, 0, 0, 0, 0);
    if (stat < 0) {
      ErrorExit("canSetBusParams", stat);
    }
  } else if (BitrateFdA != 0 && BitrateFdD != 0) {
    stat = canSetBusParams(hnd, BitrateFdA, 0, 0, 0, 0, 0);
    if (stat < 0) {
      ErrorExit("canSetBusParams", stat);
    }
    stat = canSetBusParamsFd(hnd, BitrateFdD, 0, 0, 0);
    if (stat < 0) {
      ErrorExit("canSetBusParamsFd", stat);
    }
  } else {
    Usage();
  }

  //
  // Then we start the ball rolling.
  //
  stat = canBusOn(hnd);
  if (stat < 0) {
    ErrorExit("canBusOn", stat);
    exit(1);
  }

  // Return the handle; our caller will need it for
  // further exercising the CAN controller.
  return hnd;
}

//
////////////////////////////////////////////////////////////////////////////
//
void main(int argc, char* argv[])
{
  canStatus stat;
  int i;
  canHandle hnd;
  FILE *f;
  int Ready;
  unsigned long MessageCount;
  unsigned int timeOffset;
  int Overrun;

  Filename[0] = '\0';
  //
  // First, parse the command-line arguments.
  //
  for (i=1; i<argc; i++) {
    int tmp;
    char c;
    if (sscanf(argv[i], "-%d%c", &tmp, &c) == 1) {
      Source = tmp;
    } else if (sscanf(argv[i], "-B%d%c", &tmp, &c) == 1) {
      // We'll use the canBITRATE_xxx constants here for simplicity;
      // of course CANLIB supports other bit rates but writing that code
      // is left as an exercise.
      switch (tmp) {
      case 1000 : Bitrate = canBITRATE_1M; break;
      case 500  : Bitrate = canBITRATE_500K; break;
      case 250  : Bitrate = canBITRATE_250K; break;
      case 125  : Bitrate = canBITRATE_125K; break;
      default : Usage();
      }
    } else if (sscanf(argv[i], "-A%d%c", &tmp, &c) == 1) {
      switch (tmp) {
      case 1000 : BitrateFdA = canFD_BITRATE_1M_80P; break;
      case  500 : BitrateFdA = canFD_BITRATE_500K_80P; break;
      default : Usage();
      }
    } else if (sscanf(argv[i], "-D%d%c", &tmp, &c) == 1) {
      switch (tmp) {
      case 8000 : BitrateFdD = canFD_BITRATE_8M_60P; break;
      case 4000 : BitrateFdD = canFD_BITRATE_4M_80P; break;
      case 2000 : BitrateFdD = canFD_BITRATE_2M_80P; break;
      case 1000 : BitrateFdD = canFD_BITRATE_1M_80P; break;
      default : Usage();
      }
    } else if (strcmp(argv[i], "-q") == 0) {
      Quiet++;
    } else if (strcmp(argv[i], "-fd") == 0) {
      can_open_flags |= canOPEN_CAN_FD;
    } else if (strcmp(argv[i], "-virtual") == 0) {
      can_open_flags |= canOPEN_ACCEPT_VIRTUAL;
    } else if (strcmp(argv[i], "-h") == 0) {
      Usage();
    } else if (argv[i][0] != '-') {
      strcpy(Filename, argv[i]);
    } else {
      Usage();
    }
  }

  if (Quiet == 0) {
    fprintf(stderr, "Logging to %s (candump -h for help; ESC to quit.)\n",
            Filename[0]? Filename : "standard output");
  }

  //
  // Initialize CANLIB.
  //
  canInitializeLibrary();

  //
  // Setup the selected CAN controller.
  //
  hnd = InitCtrl(Source);

  //
  // Open (i.e create) the log file.
  //
  if (strlen(Filename) > 0) {
    f = fopen(Filename, "w");
    if (!f) {
      fprintf(stderr, "Can't create log file '%s'.\n", Filename);
      canClose(hnd);
      exit(1);
    }
  } else {
    f = stdout;
  }

  //
  // Read the timer so we can print normalized time stamps later on.
  // Note that we are on-bus now, so IF some messages have arrived already,
  // they will be logged with a negative time value.
  //
  stat = kvReadTimer(hnd, &timeOffset);
  if (stat < 0) {
    ErrorExit("kvReadTimer", stat);
  }

  //
  // Print a little header, unless we are requested not to do so.
  //
  if (Quiet == 0) {
    time_t t;
    time(&t);
    fprintf(f, "; Logging started at %s", ctime(&t));
    fprintf(f, "; (x = Extended Id, R = Remote Frame, o = Overrun, N = NERR)\n");
    fprintf(f, "; Ident xRoN DLC  Data 0........................7    Time\n");
  }

  Ready = FALSE;
  MessageCount = 0;
  Overrun = FALSE;

  while (!Ready) {

    while (!_kbhit()) {
      long id;
      unsigned int dlc, flags;
      unsigned char msg[64];
      DWORD time;
      long t, timeFrac, timeInt;
      unsigned int i;

      do {
        //
        // Read a message.
        //
        stat = canRead(hnd, &id, msg, &dlc, &flags, &time);
        if (stat == canOK && (Quiet <= 1)) {
          if ((flags & canMSG_ERROR_FRAME) == 0) {
            // A message.
            // Print identifier and flags.
            fprintf(f, "%8ld%c%c%c%c%c%c  %02u ",
                    id,
                    (flags & canMSG_EXT)        ? 'x' : ' ',
                    (flags & canMSG_RTR)        ? 'R' : ' ',
                    (flags & canMSGERR_OVERRUN) ? 'o' : ' ',
                    (flags & canMSG_NERR)       ? 'N' : ' ', // TJA 1053/1054 transceivers only
                    (flags & canFDMSG_EDL)      ? 'F' : ' ',
                    (flags & canFDMSG_BRS)      ? 'B' : ' ',
                    dlc);
          } else {
            // An error frame.
            // CANLIB will set the id to something controller
            // dependent.
            fprintf(f, "  (%04lx) Error    ", id);
          }

          // Print the data bytes, but not for Remote Frames.
          // Print at most 8 bytes - the DLC might be larger
          // but there are never more than 8 bytes in a
          // message.
          if (flags & canFDMSG_EDL) {
            if ((flags & canMSG_RTR) == 0) {
              for (i=0; i < dlc; i++) fprintf(f, "%3u ", msg[i]);
              for (; i<8; i++) fprintf(f, "    ");
            } else {
              fprintf(f, "                                ");
            }
          }
          else {
            if ((flags & canMSG_RTR) == 0) {
              for (i=0; i < ((dlc > 8)? 8: dlc); i++) fprintf(f, "%3u ", msg[i]);
              for (; i<8; i++) fprintf(f, "    ");
            } else {
              fprintf(f, "                                ");
            }
          }

          // Print the time stamp, formatted as seconds.
          t = time - timeOffset;
          if (t < 0) {
            // Yes, negative values may occur.. see above.
            timeFrac = (-t) % 1000;
            timeInt  = (-t) / 1000;
            fprintf(f, "-%5ld.%03ld\n", timeInt, timeFrac);
          } else {
            timeFrac = t % 1000;
            timeInt  = t / 1000;
            fprintf(f, "%6ld.%03ld\n", timeInt, timeFrac);
          }

          // Keep some statistics.
          if (flags & canMSGERR_OVERRUN) Overrun = TRUE;
          MessageCount++;
        }
        //
        // When there are no more messages in the queue,
        // stat will be equal to canERR_NOMSG.
        //
      } while (stat == canOK);
      // Have a nap while the driver accumulates messages for us..
      Sleep(50);
    }

    //
    // React to whatever the user has entered at the keyboard.
    //
    switch (_getch()) {
    case 3:
    case 27:
      Ready = TRUE;
      break;
    case 'q':
      if (Quiet > 0) Quiet--;
      fprintf(stderr, "Quiet=%d\n", Quiet);
      break;
    case 'Q':
      if (Quiet < 5) Quiet++;
      fprintf(stderr, "Quiet=%d\n", Quiet);
      break;
    }
  }

  //
  // Print a little footer, unless we are requested not to do so.
  //
  if (Quiet == 0) {
    time_t t;
    time(&t);
    fprintf(f, "; Logging ended at %s", ctime(&t));
    fprintf(f, "; %lu messages.%s\n",
            MessageCount,
            Overrun? " There were overruns." : "");
  }

  //
  // Clean up.
  //
  fclose(f);
  (void)canBusOff(hnd);
  (void)canClose(hnd);

}

