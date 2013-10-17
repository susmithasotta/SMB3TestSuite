.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk

SOURCES = TimesSeq.cs

all: writer.log reader.log

reader.log : Reader.exe
	Reader > reader.log

writer.log : Writer.exe
	Writer > writer.log

Reader.exe Writer.exe : Reader.cs Writer.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_ber.TimeString.Reader *.cs
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Writer.exe /main:Sample_ber.TimeString.Writer *.cs

$(SOURCES) : time.asn
	$(ASN1C) time.asn -ber -csharp -print -nspfx Sample_ber -gencssources

clean:
	$(MV) Reader.cs Reader.cs_save
	$(MV) Writer.cs Writer.cs_save
	$(RM) *.cs 
	$(RM) *.dll 
	$(MV) Reader.cs_save Reader.cs
	$(MV) Writer.cs_save Writer.cs
	$(RM) *.exe
	$(RM) *.log
	$(RM) *~

