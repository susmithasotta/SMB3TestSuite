# common makefile defs for all sample programs

OSROOTDIR = ../../..
LIBDIR  = ../../lib
DLLDIR  = ../../dll
SRCDIR  = ../../src
RTSRCDIR = ../../../rtsrc
RTXSRCDIR = ../../../rtxsrc
BERSRCDIR = ../../../rtbersrc
PERSRCDIR = ../../../rtpersrc
XERSRCDIR = ../../../rtxersrc
XMLSRCDIR = ../../../rtxmlsrc
SPECSDIR = ../../../specs

BERHFILES = $(RTSRCDIR)/asn1type.h $(BERSRCDIR)/asn1ber.h
BERCPPHFILES = \
$(BERHFILES) $(RTSRCDIR)/asn1CppTypes.h $(BERSRCDIR)/asn1BerCppTypes.h
PERHFILES = $(RTSRCDIR)/asn1type.h $(PERSRCDIR)/asn1per.h
PERCPPHFILES = \
$(PERHFILES) $(RTSRCDIR)/asn1CppTypes.h $(PERSRCDIR)/asn1PerCppTypes.h

ASN1C	= ..\\..\\..\\bin\\asn1c$(EXE)
BER2INDEF = ..\\..\\..\\bin\\ber2indef
CFLAGS	= -D_TRACE $(CBLDTYPE_) $(CVARS_) $(MCFLAGS) $(CFLAGS_)  -DCPP
CCFLAGS = $(CFLAGS) $(XMLDEFS)
RTCFLAGS = $(CFLAGS)
BERCFLAGS = $(CFLAGS)
PERCFLAGS = $(CFLAGS)
RTCCFLAGS = $(CCFLAGS)
BERCCFLAGS = $(CCFLAGS)
PERCCFLAGS = $(CCFLAGS)
IPATHS = \
-I. -I$(SRCDIR) -I$(RTSRCDIR) -I$(RTXSRCDIR) -I$(BERSRCDIR) -I$(PERSRCDIR) \
-I$(XERSRCDIR) -I$(OSROOTDIR) $(IPATHS_)
LINKOPT	= $(LINKOPT_)
LPATHS    = $(LPPFX)$(LIBDIR) $(LPATHS_)
LDPATHS   = $(LPPFX)$(LIBDDIR) $(LPATHS_) 
LDLLPATHS = $(LPPFX)$(DLLDIR) $(LPATHS_)
BSLIBS  = $(LLBER) $(LLRT) $(LLSYS) 
PSLIBS  = $(LLPER) $(LLRT) $(LLSYS) 
XERLIBS = $(LLXER) $(LLRT) $(LLSYS) 
XMLLIBS = $(LLXML) $(LLRT) $(LLSYS) 
XERLINKOPT = $(LINKOPT)
