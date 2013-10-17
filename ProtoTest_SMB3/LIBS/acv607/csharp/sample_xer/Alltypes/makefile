.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk

SOURCES = TestType.cs

all: message.xml reader.log 

message.xml : Writer.exe
	Writer > writer.log

reader.log : Reader.exe message.xml
	Reader > reader.log

Writer.exe Reader.exe : Writer.cs Reader.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_xer.Alltypes.Reader *.cs
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Writer.exe /main:Sample_xer.Alltypes.Writer *.cs

$(SOURCES) : test.asn
	$(ASN1C) test.asn -xer -csharp -print -genjsources -namespace Sample_xer.Alltypes

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

