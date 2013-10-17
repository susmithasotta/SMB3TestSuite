.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk

SOURCES = TokenInfo.cs

all: writer.log reader.log

reader.log : Reader.exe
	Reader > reader.log

writer.log : Writer.exe
	Writer > writer.log

Reader.exe Writer.exe : Reader.cs Writer.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_der.PKCS15TokenInfo.Reader *.cs
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Writer.exe /main:Sample_der.PKCS15TokenInfo.Writer *.cs

$(SOURCES) : PKCS15TokenInfo.asn
	$(ASN1C) PKCS15TokenInfo.asn -der -csharp -print -nspfx Sample_der -gencssources

clean:
	$(MV) Reader.cs Reader.cs_save
	$(MV) Writer.cs Writer.cs_save
	$(RM) *.cs 
	$(RM) *.exe
	$(RM) *.dll
	$(RM) *.pdb
	$(RM) *~
	$(RM) *.log
	$(MV) Reader.cs_save Reader.cs
	$(MV) Writer.cs_save Writer.cs

