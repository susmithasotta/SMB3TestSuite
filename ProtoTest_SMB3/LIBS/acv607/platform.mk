.SUFFIXES : .c .cc .cpp .obj

.cpp.obj:
	$(CCC) $(CCFLAGS) -c $(IPATHS) $<

.c.obj:
	$(CC) $(CFLAGS) -c $(IPATHS) $<

.cc.obj:
	$(CC) $(CFLAGS) -c $(IPATHS) $<

# The following symbols should be valid on all Windows systems
CC	  = cl
CCC	  = cl
CFLAGS_	  = -nologo -GX -W3
CVARS0_	  = -DASN1RT -DWIN32 -D_WIN32 -DMSDOS -GF
CVARSDLL_ = $(CVARSRTDLL_) -DASN1DLL
COPTIMIZE0_ = -Ob2 -Gy -Ox -G6
CDEBUG0_  = -Zi -D_DEBUG
FS	  = ;
LIBCMD	  = lib /NOLOGO /OUT:$@
LIBADD	  = lib /NOLOGO /OUT:$@ $@
LINK	  = cl
LINKOPT0  = /nologo /link /OUT:$@ /OPT:REF  
LINKDBG_  = /MAP /DEBUG:FULL /DEBUGTYPE:BOTH
LINKOPTM_ = /OPT:REF  
OBJOUT	  = -Fo$@
PLATFORM  = WIN32
PS	  = \#
PURECVAR  = -TC

CVARSR_	  = $(CVARS0_) -ML
CVARSMTR_  = $(CVARS0_) -D_MT -MT
CVARSMDR_  = $(CVARS0_) -D_MT -D_DLL -MD

CVARSCRTDLLR_ = $(CVARS0_) -D_MT -D_DLL -MD
CVARSRTDLLR_ = $(CVARS0_) -D_MT -D_DLL -MD $(USEDLL)
LINKOPTR_  = $(LINKOPT0)
LINKOPT2R  = /nologo /link /OUT:$@ /OPT:REF

CVARSD_	  = $(CVARS0_) -MLd
CVARSMTD_  = $(CVARS0_) -D_MT -MTd
CVARSMDD_  = $(CVARS0_) -D_MT -D_DLL -MDd

CVARSCRTDLLD_ = $(CVARS0_) -D_MT -D_DLL -MDd
CVARSRTDLLD_ = $(CVARS0_) -D_MT -D_DLL -MDd $(USEDLL)
LINKOPTD_  = $(LINKOPT0) $(LINKDBG_)
LINKOPT2D  = /nologo /link /OUT:$@ /OPT:REF $(LINKDBG_)
COPTIMIZE_ = $(COPTIMIZE0_) -D_OPTIMIZED

CDEV_     = -D_TRACE -Od
CDEBUG_   = $(CDEV_) $(CDEBUG0_)
CBLDTYPE_ = $(COPTIMIZE_)
CVARS_    = $(CVARSR_)
CVARSMT_  = $(CVARSMTR_)
CVARSMD_  = $(CVARSMDR_)
CVARSCRTDLL_ = $(CVARSCRTDLLR_)
CVARSRTDLL_ = $(CVARSRTDLLR_)
LINKOPT_  = $(LINKOPTR_)
LINKOPT2  = $(LINKOPT2R)


# File extensions
EXE	= .exe
OBJ	= .obj

# Run-time library
LIBPFX	=
LIBEXT	= lib
LPPFX	= -LIBPATH:
LLPFX   =
LLEXT   = .lib
A       = _a.$(LIBEXT)
MTA     = mt_a.$(LIBEXT)
MDA     = md_a.$(LIBEXT)
IMP     = .$(LIBEXT)
DLL     = .dll

# O/S commands
COPY	 = -copy
MOVE	 = -move
MV	 = $(MOVE)
RM	 = -del
STRIP	 = strip -g -S
MAKE     = nmake

LLSYS	= user32.lib ws2_32.lib advapi32.lib

# LIBXML2 defs
LIBXML2ROOT = c:/libxml2
LIBXML2INC  = $(LIBXML2ROOT)/include
LIBXML2LIBDIR = $(LIBXML2ROOT)/lib
LIBXML2NAME = libxml2_a.lib
LIBXML2LINK = libxml2_a.lib

# START ASN1C
COMPACT	  = -D_COMPACT
NOLIC_	  = -D_NO_LICENSE_CHECK
# Link libraries
# The template is: LLxxxSSS, where:
#    xxx - BER. PER, XER or RT;
#    SSS - suffix (Windows only):
#       MT  - multi-threaded
#       MD  - DLL-ready
#       IMP - import library of DLL
LLBER	= asn1ber$(A)
LLBERMT	= asn1ber$(MTA)
LLBERMD	= asn1ber$(MDA)
LLBERIMP= asn1ber$(IMP)
LLPER	= asn1per$(A)
LLPERMT	= asn1per$(MTA)
LLPERMD	= asn1per$(MDA)
LLPERIMP= asn1per$(IMP)
LLXER	= asn1xer$(A)
LLXERMT	= asn1xer$(MTA)
LLXERMD	= asn1xer$(MDA)
LLXERIMP= asn1xer$(IMP)
LLXML	= asn1xml$(A)
LLXMLMT	= asn1xml$(MTA)
LLXMLMD	= asn1xml$(MDA)
LLXMLIMP= asn1xml$(IMP)
LLRT	= asn1rt$(A)
LLRTMT	= asn1rt$(MTA)
LLRTMD	= asn1rt$(MDA)
LLRTIMP = asn1rt$(IMP)
LLLIC	= license.lib
LLASN1C = asn1c.lib
LLOSCOM = $(OSLIBNAME)
LLX2A   = $(X2ALIBNAME)
LLX2AAC = $(X2AACLIBNAME)
USEDLL  = -DUSERTDLL -DUSERTXDLL -DUSEASN1RTDLL -DUSEASN1BERDLL -DUSEASN1PERDLL -DUSEASN1XERDLL -DUSEXMLDLL

# Library names
# The template is: xxxLLLsssNAME, where:
#    xxx - BER. PER, XER or RT;
#    LLL - LIB or DLL:
#    sss - suffix (Windows only):
#       MT  - multi-threaded
#       MD  - DLL-ready
#       IMP - import library of DLL
BERLIBNAME   = asn1ber$(A)
BERLIBMTNAME = asn1ber$(MTA)
BERLIBMDNAME = asn1ber$(MDA)
BERLIBIMPNAME= asn1ber$(IMP)
BERDLLNAME   = asn1ber$(DLL)
PERLIBNAME   = asn1per$(A)
PERLIBMTNAME = asn1per$(MTA)
PERLIBMDNAME = asn1per$(MDA)
PERLIBIMPNAME= asn1per$(IMP)
PERDLLNAME   = asn1per$(DLL)
LICLIBNAME   = license.lib
OSLIBNAME    = oscom_a.lib
RTLIBNAME    = asn1rt$(A)  
RTLIBMTNAME  = asn1rt$(MTA)
RTLIBMDNAME  = asn1rt$(MDA)
RTLIBIMPNAME = asn1rt$(IMP)  
RTDLLNAME    = asn1rt$(DLL)  
XERLIBNAME   = asn1xer$(A)
XERLIBMTNAME = asn1xer$(MTA)
XERLIBMDNAME = asn1xer$(MDA)
XERLIBIMPNAME= asn1xer$(IMP)
XERDLLNAME   = asn1xer$(DLL)
XMLLIBNAME   = asn1xml$(A)
XMLLIBMTNAME = asn1xml$(MTA)
XMLLIBMDNAME = asn1xml$(MDA)
XMLLIBIMPNAME= asn1xml$(IMP)
XMLDLLNAME   = asn1xml$(DLL)
X2ALIBNAME   = xsd2asn1_a.lib
X2AACLIBNAME = xsd2asn1ac_a.lib
# END ASN1C

# Added because of -TP mismatch
MCOMFLAGS    = -TP
