envvar {
  int counter;
  char text[64];
  float value;
}

on start {
  envvarSetValue(counter, 0);
  envvarSetValue(text, "");
  envvarSetValue(value, 0.0);
}

on envvar text {
  int tmp;
  envvarGetValue(counter, &tmp);
  envvarSetValue(counter, tmp+1);
}

on envvar value {
  int tmp;
  envvarGetValue(counter, &tmp);
  envvarSetValue(counter, tmp+1);
}
  

