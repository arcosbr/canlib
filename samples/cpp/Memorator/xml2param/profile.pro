include ($(CANLIB_ROOT)\mkspecs\standard.pro)

TEMPLATE        = app

CONFIG          += warn_on

DEFINES         = WIN32

RES_FILE        = $$OBJECTS_DIR\version.res

INCLUDEPATH     = . $(CANLIB_ROOT)\include \
                    $(CANLIB_ROOT)\src\dll\memolibxml\memolibxml_mhydra \
                    $(CANLIB_ROOT)\src\dll\kvaconverter \
                    $(COMMON_INC_ROOT)\inc \
                    $(COMMON_INC_ROOT)\inc\memolib_mhydra

SOURCES         = xml2param.cpp 

TARGET          = $$OBJECTS_DIR\xml2param

QMAKE_CLEAN     += $$RES_FILE $${TARGET}.exe


contains( CONFIG, debug) {
  QMAKE_LIBS      += $(CANLIB_ROOT)\src\dll\memolibxml\memolibxml_mhydra\out.db\kvamemolibxml.lib
  QMAKE_CFLAGS    += /MTd -Od
  QMAKE_CXXFLAGS  += /MTd -Od
  QMAKE_LFLAGS    += /debug
  DEFINES += CRTMEMCHECK  _DEBUG DEBUG=1
}

!contains( CONFIG, debug) {
  QMAKE_LIBS      += $(CANLIB_ROOT)\src\dll\memolibxml\memolibxml_mhydra\out.ndb\kvamemolibxml.lib
  QMAKE_CFLAGS    += /MT
  QMAKE_CXXFLAGS  += /MT
}
