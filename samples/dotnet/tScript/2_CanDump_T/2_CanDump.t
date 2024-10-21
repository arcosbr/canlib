variables {
  const int channel = 0;  
}


on start {
  canSetBitrate(channel, canBITRATE_250K);
  canSetBusOutputControl(channel, canDRIVER_NORMAL);
  canBusOn(channel);
  printf("Waiting for messages");
}


on stop {
  canBusOff(channel);
}


on CanMessage<channel> 123 {
  //this following line will only be executed the first time
  //a matching message is received
  static int count = 0;

  count++;
  printf("Matching messages received: %d\n", count);
  
}


on CanMessage<*> * {
  printf("Non-matching message received on channel %d with id %d", this.channel, this.id);
}