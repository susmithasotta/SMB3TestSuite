.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk

SOURCES = TimesSeq.cs

all: writer.log reader.log

reader.log : Reader.exe
	Reader > reader.log

writer.log : Writer.exe
	Writer > writer.log

Writer.exe Reader.exe : Writer.cs Reader.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_der.TimeString.Reader *.cs
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Writer.exe /main:Sample_der.TimeString.Writer *.cs

$(SOURCES) : time.asn
	$(ASN1C) time.asn -der -csharp -print -nspfx Sample_der -gencssources

clean:
	$(MV) Reader.cs Reader.cs_save
	$(MV) Writer.cs Writer.cs_save
	$(RM) *.cs 
	$(MV) Reader.cs_save Reader.cs
	$(MV) Writer.cs_save Writer.cs
	$(RM) *.exe
	$(RM) *.dll
	$(RM) *.pdb
	$(RM) *.log
	$(RM) *~

