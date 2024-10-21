include ($(CANLIB_ROOT)\mkspecs\standard.pro)


DEF_FILE        = 

TEMPLATE        = app

DEFINES         +=  WIN32_LEAN_AND_MEAN \
                    _CRT_SECURE_NO_WARNINGS \
                    WIN32 \
                    __WIN32__

QMAKE_CFLAGS    +=

CONFIG          += warn_on

RES_FILE        = $$OBJECTS_DIR\version.res

INCLUDEPATH     = . \
                  $(COMMON_INC_ROOT)\inc \

SOURCES         = getconf.c

TARGET          = $$OBJECTS_DIR\getconf

QMAKE_CLEAN     += $$RES_FILE $${TARGET}.exe

QMAKE_LIBS      += $(CANLIB_ROOT)\src\dll\memolib\kvmlib\windows\out.ndb\kvmlib.lib
QMAKE_LIBS      += $(CANLIB_ROOT)\src\dll\memolib\memolib_wrap\windows\out.ndb\kvamemolib.lib
