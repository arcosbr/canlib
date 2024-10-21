/*
** This software is furnished as Redistributable under the Kvaser Software Licence
** https://www.kvaser.com/canlib-webhelp/page_license_and_copyright.html
** Description:
** This is a sample console program that uses kvadblib to create and access CAN
** databases. It creates a J1939 database file named 'my_J1939_db.dbc' in
** the directory where it is invoked.
** The program is just intended as an example of how to use kvadblib. It's not
** performance optimized in any way. It may, or may not, be useful for you.
*/

#include <stdio.h>

// kvadblib requires the following #include.
#include "kvaDbLib.h"

// Forward declarations
void Check(const char* id, KvaDbStatus stat);
KvaDbHnd createJ1939Db(char *path);

int main(int argc, char* argv[])
{
  KvaDbHnd db_handle;
  KvaDbStatus status;

  char* path = "my_J1939_db.dbc";
  db_handle = createJ1939Db(path);

  // Write database to file
  status = kvaDbWriteFile(db_handle, path);
  Check("kvaDbWriteFile", status);
}

void Check(const char* id, KvaDbStatus stat)
{
  if (stat < 0) {
    printf("%s: failed, stat=%d\n", id, (int)stat);
  }
}

/* Construct database from scratch with a single J1939 message
*  with necessary J1939 attributes.
*/
KvaDbHnd createJ1939Db(char* path) {
  KvaDbStatus status;
  KvaDbHnd db_handle;
  KvaDbMessageHnd mh;
  KvaDbSignalHnd sh;
  KvaDbAttributeDefHnd ProtocolType_attr, VFrameFormat_attr;

  char* VFrameFormat_enum_names[4] = {"StandardCAN", "ExtendedCAN", "reserved", "J1939PG"};

  /* ######################################
            Construct J1939 database
     ###################################### */

  // Open handle to database
  status = kvaDbOpen(&db_handle);
  Check("kvaDbOpen", status);

  // Associate a new database with database handle
  status = kvaDbCreate(db_handle, path, NULL);
  Check("kvaDbCreate", status);

  // Add necessary J1939 attributes to database
  status = kvaDbAddAttributeDefinition(db_handle, &ProtocolType_attr);
  Check("kvaDbAddAttributeDefinition protocolType", status);
  status = kvaDbSetAttributeDefinitionType(ProtocolType_attr, kvaDb_AttributeString);
  Check("kvaDbSetAttributeDefinitionType protocolType", status);
  status = kvaDbSetAttributeDefinitionName(ProtocolType_attr, "ProtocolType");
  Check("kvaDbSetAttributeDefinitionName protocolType", status);
  status = kvaDbSetAttributeDefinitionOwner(ProtocolType_attr, kvaDb_AttributeOwnerDb);
  Check("kvaDbSetAttributeDefinitionOwner protocolType", status);
  status = kvaDbSetAttributeDefinitionString(ProtocolType_attr, "J1939");
  Check("kvaDbSetAttributeDefinitionString protocolType", status);

  status = kvaDbAddAttributeDefinition(db_handle, &VFrameFormat_attr);
  Check("kvaDbAddAttributeDefinition VFrameFormat", status);
  status = kvaDbSetAttributeDefinitionType(VFrameFormat_attr, kvaDb_AttributeEnumeration);
  Check("kvaDbSetAttributeDefinitionType VFrameFormat", status);
  status = kvaDbSetAttributeDefinitionName(VFrameFormat_attr, "VFrameFormat");
  Check("kvaDbSetAttributeDefinitionName VFrameFormat", status);
  status = kvaDbSetAttributeDefinitionOwner(VFrameFormat_attr, kvaDb_AttributeOwnerMessage);
  Check("kvaDbSetAttributeDefinitionOwner VFrameFormat", status);

  for (int i = 0; i < 4; i++) {
    status = kvaDbAddAttributeDefinitionEnum(VFrameFormat_attr, VFrameFormat_enum_names[i], i);
    Check("kvaDbAddAttributeDefinitionEnum", status);
  }

  /* ######################################
        Define a message with J1939 flag
     ###################################### */

  // Add message to database
  status = kvaDbAddMsg(db_handle, &mh);
  Check("kvaDbAddMsg", status);

  status = kvaDbSetMsgName(mh, "NewTestMsgJ1939");
  Check("kvaDbSetMsgName", status);

  status = kvaDbSetMsgDlc(mh, 8);
  Check("kvaDbSetMsgDlc", status);

  status = kvaDbSetMsgIdEx(mh, 0xCF00401);
  Check("kvaDbSetMsgId", status);

  // Set message flags, note flag argument KVADB_MESSAGE_J1939
  status = kvaDbSetMsgFlags(mh, KVADB_MESSAGE_J1939);
  Check("kvaDbSetMsgFlags", status);

  /* ######################################
        Add a signal to the message
     ###################################### */

  // Add signal to message
  status = kvaDbAddSignal(mh, &sh);
  Check("kvaDbAddSignal", status);

  status = kvaDbSetSignalName(sh, "NewTestSignal");
  Check("kvaDbSetSignalName", status);

  status = kvaDbSetSignalComment(sh, "A test signal.");
  Check("kvaDbSetSignalComment", status);

  status = kvaDbSetSignalUnit(sh, "-");
  Check("kvaDbSetSignalUnit", status);

  status = kvaDbSetSignalEncoding(sh, kvaDb_Intel);
  Check("kvaDbSetSignalEncoding", status);

  status = kvaDbSetSignalRepresentationType(sh, kvaDb_Unsigned);
  Check("kvaDbSetSignalRepresentationType", status);

  status = kvaDbSetSignalValueSize(sh, 0, 8);
  Check("kvaDbSetSignalValueSize", status);

  status = kvaDbSetSignalValueLimits(sh, 0, 255);
  Check("kvaDbSetSignalValueLimits", status);

  return db_handle;
}