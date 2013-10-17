.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk
SOURCES = RosePDU.cs

all: $(SOURCES)
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /target:library /out:SimpleROSE.dll *.cs

$(SOURCES) : simple-ROSE.asn
	$(ASN1C) simple-ROSE.asn -ber -csharp -print -nspfx Sample_ber

clean:
	$(RM) *.cs 
	$(RM) *.dll
	$(RM) *.pdb
	$(RM) *.exe
	$(RM) *~
