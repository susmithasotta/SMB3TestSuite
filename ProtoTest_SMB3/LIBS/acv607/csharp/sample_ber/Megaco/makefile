.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk
SOURCES = MegacoMessage.cs

all: reader.log

reader.log : Reader.exe
	Reader -i msg05.bin> reader.log

Reader.exe : Reader.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll\
	/out:Reader.exe /main:Sample_ber.Megaco.Reader *.cs

$(SOURCES) : megaco.asn
	$(ASN1C) megaco.asn -ber -csharp -print -namespace Sample_ber.Megaco -gencssources

clean:
	$(MV) Reader.cs Reader.cs_save
	$(RM) *.cs 
	$(RM) *.exe
	$(RM) *.dll
	$(RM) *.pdb
	$(RM) *~
	$(RM) *.log
	$(MV) Reader.cs_save Reader.cs


