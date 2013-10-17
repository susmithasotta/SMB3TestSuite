# makefile to build test program

include ../platform.mk

all : ../bin/berfdump$(EXE) ../bin/ber2indef$(EXE) ../bin/ber2def$(EXE) \
../bin/dumpasn1$(EXE)

CFLAGS  = $(CFLAGS_) $(CVARS_) 
HFILES  = ../rtbersrc/asn1ber.h ../rtsrc/asn1type.h ../rtsrc/asn1intl.h
IPATHS  = -I. -I.. -I../rtsrc -I../rtbersrc $(IPATHS_)
LINKOPT	= $(LINKOPT_)
RTCLIBDIR = ../c/lib
LPATHS  = $(LPPFX)$(RTCLIBDIR) $(LPATHS_)

../bin/berfdump$(EXE) : berfdump$(OBJ) $(RTCLIBDIR)/$(BERLIBNAME)
	$(CC) berfdump$(OBJ) $(LINKOPT) $(LPATHS) $(LLBER) $(LLSYS)

../bin/ber2indef$(EXE) : ber2indef$(OBJ) $(RTCLIBDIR)/$(BERLIBNAME) \
$(RTCLIBDIR)/$(RTLIBNAME)
	$(CC) ber2indef$(OBJ) $(LINKOPT) $(LPATHS) $(LLBER) $(LLRT) $(LLSYS)

../bin/ber2def$(EXE) : ber2def$(OBJ) $(RTCLIBDIR)/$(BERLIBNAME) \
$(RTCLIBDIR)/$(RTLIBNAME)
	$(CC) ber2def$(OBJ) $(LINKOPT) $(LPATHS) $(LLBER) $(LLRT) $(LLSYS)

../bin/dumpasn1$(EXE) : dumpasn1$(OBJ)
	$(CC) dumpasn1$(OBJ) $(LINKOPT) $(LLSYS)

berfdump$(OBJ)  : berfdump.c $(HFILES)
ber2indef$(OBJ) : ber2indef.c $(HFILES)
ber2def$(OBJ)   : ber2def.c $(HFILES)
dumpasn1$(OBJ)  : dumpasn1.c

clean :
	$(RM) ..$(PS)bin$(PS)berfdump$(EXE)
	$(RM) ..$(PS)bin$(PS)ber2indef$(EXE)
	$(RM) ..$(PS)bin$(PS)ber2def$(EXE)
	$(RM) *$(OBJ)
	$(RM) *.exp
	$(RM) *.pdb
	$(RM) *.map
	$(RM) *.lib
	$(RM) *~
	$(RM) rtkey.h
