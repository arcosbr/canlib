
//Global variables
variables {
  const int channel = 0;
  const int bitrate = 1000000;
  
  const int MAX_MSG = 50;
  const int LINE_LENGTH = 80;

  //Number of received messages
  int count = 0;

  typedef struct {
    char line[LINE_LENGTH];
  } Line;
  Line out[MAX_MSG];

  
  char outputfile[LINE_LENGTH] = "output.txt";
  char inputfile[LINE_LENGTH] = "input.txt";
}

//Forward declarations
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
   printf(out[count].line);
   count++;
  }
}

//Puts the CAN message's string representation
//into the "out" array
void msg_to_str (CanMessage m, int index){
  if(m.flags & canMSG_ERROR_FRAME){
    out[index].line = "***ERROR FRAME***";
  }
  else{
    char result[LINE_LENGTH];
    char buf[LINE_LENGTH];
    sprintf(result, "%d ", m.id);
  
    for(int i = 0; i<m.dlc; i++){
      sprintf(buf, "%02x", m.data[i]);
      strcat(result, buf);
    }
    if(m.flags & canMSG_RTR) {
      strcat(result, "r");
    }
    if(m.flags & canMSG_EXT) {
      strcat(result, "x");
    }
    strcat(result, ";\n");
    out[index].line = result;
  }
}

//Writes the messages in the "out" array to the output file
void write_messages() {
  FileHandle fh;
  if(fileOpen(fh, outputfile) >= 0) {
    for(int i = 0; i < count; i++) {
      filePuts(fh, out[i].line);
    }
    fileClose(fh);
  }
}

//Reads messages from the input file and writes them to the bus
int read_messages() {
  FileHandle fh;
  if(fileOpen(fh, inputfile, OPEN_READ) >= 0) {
    char buf[LINE_LENGTH];
    int length = fileGets(fh, buf);
    while (length > 0){
      CanMessage m;
      int pos = 0;
      //Skips through the id
      while(pos < length){
        if(buf[pos] == ' '){
          break;
        }
        else{
          pos++;
        }
      }
      //Skips any spaces between the id and the message
      while(buf[pos] == ' '){
        pos++;
      }
      
      m.id = atoi(buf);
      m.dlc=0;
      for(int i = 0; i < 8; i ++){
        int b = atoi(buf[pos, 2], 16);
        if(b == 0 && !(buf[pos] == '0' && buf[pos+1] == '0')){
          break;
        }
        else{
          printf("Pos: %d", pos);
          m.dlc = i+1;
          m.data[i] = b;
          pos+=2;
        }
      }
      
      m.flags=0;
      while(pos < length){
        if(buf[pos] == 'r'){
          m.flags += canMSG_RTR;
        }
        else if(buf[pos] == 'x'){
          m.flags += canMSG_EXT;
        }
        else if(buf[pos] == ';'){
          break;
        }
        pos++;
      }
      printf("sending message with id %d and dlc %d", m.id, m.dlc);
      canWrite(channel, m);
      length = fileGets(fh, buf);
    }
  }
  return 0;
}

