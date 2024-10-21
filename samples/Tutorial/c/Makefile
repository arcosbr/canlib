# Compiler name
CC = gcc

# Include and lib directories
INCLUDE_DIR = C:/Kvaser/Canlib/include
LIB_DIR = C:/Kvaser/Canlib/lib/x64

# Executable name
TARGET = CanTest.exe

# Compiler flags
CFLAGS = -I$(INCLUDE_DIR)
LDFLAGS = -L$(LIB_DIR) -lcanlib32

# Source file
SRC = CanSend.c

# Default rule
all: $(TARGET)

$(TARGET): $(SRC)
	$(CC) $(CFLAGS) -o $(TARGET) $(SRC) $(LDFLAGS)

# Clean up
clean:
	del $(TARGET)
