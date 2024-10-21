
//Global variables
variables {
  const int channel = 0;
  const int bitrate = 1000000;
  
  const int MAX_MSG = 50;

  //Number of received messages
  int count = 0;

  typedef struct {
    char text[sizeof(CanMessage)];
  } MsgBlock;
  MsgBlock out[MAX_MSG];

  
  char outputfile[11] = "output.txt";
  char inputfile[10] = "input.txt";
}

void msg_to_str(CanMessage m, int index);
void write_messages();
int read_messages();

on start {
  canBusOff(channel);
  canSetBitrate(channel, bitrate);
  canSetBusOutputControl(channel, canDRIVER_NORMAL);
  canBusOn(channel);
  printf("Went on bus\n");
  read_messages();
}

on stop {
  canBusOff(channel);
  write_messages();
}

on CanMessage<channel> * {
  if(count < MAX_MSG){
   msg_to_str(this, count);
   printf(out[count].text);
   count++;
  }
}

//Puts the CAN message's string representation
//into the "out" array
void msg_to_str (CanMessage m, int index){
  out[index].text = m;
}

//Writes the messages in the "out array to the output file
void write_messages() {
  FileHandle fh;
  if(fileOpen(fh, outputfile) >= 0) {
    for(int i = 0; i < count; i++) {
      fileWriteBlock(fh, out[i].text);
    }
    fileClose(fh);
  }
}

//Reads messages from the input file and writes them to the bus
int read_messages() {
  FileHandle fh;
  if(fileOpen(fh, inputfile, OPEN_READ) >= 0) {
    char buf[sizeof(CanMessage)];
    int length = fileReadBlock(fh, buf, sizeof(CanMessage));
    while (length > 0){
      CanMessage m;
      m = buf;
      printf("sending message with id %d and dlc %d", m.id, m.dlc);
      canWrite(channel, m);
      length = fileReadBlock(fh, buf);
    }
  }
  return 0;
}

