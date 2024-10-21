
variables {
	const int channel = 0;  
}

on start {
  canSetBitrate(channel, canBITRATE_250K);
  canSetBusOutputControl(channel, canDRIVER_NORMAL);
  canBusOn(channel);
  CanMessage msg;
  msg.id     = 0;
  msg.dlc    = 8;
  msg.flags  = 0;
  msg.data   = "\x01\x02\x03\x04\x05\x06\x07\x08";
  
  int stat = canWrite(channel, msg);
  if(stat == 0){
    printf("Message sent to channel %d\n", channel);
  }
  else{
    printf("Error when sending message\n");
  }
  
}

on stop {
  //canBusOff(channel);
}

