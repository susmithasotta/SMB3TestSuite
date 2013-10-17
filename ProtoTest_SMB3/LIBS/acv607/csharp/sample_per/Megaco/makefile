# makefile to build sample program

.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk

SOURCES = MegacoMessage.cs

all: reader_a.log 

reader_a.log : Reader.exe 
	Reader -a > reader_a.log

Reader.exe : Reader.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_per.Megaco.Reader *.cs

$(SOURCES) : megaco.asn
	$(ASN1C) megaco.asn -per -csharp -print -namespace Sample_per.Megaco -trace -gencssources

clean:
	$(MV) Reader.cs Reader.cs_save
	$(RM) *.cs 
	$(RM) *.dll
	$(RM) *.pdb
	$(RM) *.exe
	$(RM) *~
	$(RM) *.log
	$(MV) Reader.cs_save Reader.cs

