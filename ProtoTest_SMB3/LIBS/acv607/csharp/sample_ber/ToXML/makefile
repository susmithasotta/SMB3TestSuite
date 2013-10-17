.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk

SOURCES = PersonnelRecord.cs

all: writer.log reader.log

reader.log : Reader.exe
	Reader > reader.log

writer.log : Writer.exe
	Writer > writer.log

Reader.exe Writer.exe : Reader.cs Writer.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_ber.ToXML.Reader *.cs
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Writer.exe /main:Sample_ber.ToXML.Writer *.cs

$(SOURCES) : employee.asn
	$(ASN1C) employee.asn -ber -csharp -print -events -namespace Sample_ber.ToXML

clean:
	$(MV) Reader.cs Reader.cs_save
	$(MV) Writer.cs Writer.cs_save
	$(MV) XMLHandler.cs XMLHandler.cs_save
	$(RM) *.cs 
	$(RM) *.exe
	$(RM) *.dll
	$(RM) *.pdb
	$(RM) *~
	$(RM) *.log
	$(MV) Reader.cs_save Reader.cs
	$(MV) Writer.cs_save Writer.cs
	$(MV) XMLHandler.cs_save XMLHandler.cs

