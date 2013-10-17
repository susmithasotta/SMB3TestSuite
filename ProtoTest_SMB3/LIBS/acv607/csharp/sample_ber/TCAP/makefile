.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk

SOURCES = Abort.cs

all: $(SOURCES)
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /warn:0 /out:tcap.dll /target:library *.cs


$(SOURCES) : tcap.asn
	$(ASN1C) tcap.asn -asnstd x208 -ber -csharp -print -namespace Sample_ber.TCAP -gencssources

clean:
	$(RM) *.cs 
	$(RM) *.exe
	$(RM) *.dll
	$(RM) *.pdb
	$(RM) *~

