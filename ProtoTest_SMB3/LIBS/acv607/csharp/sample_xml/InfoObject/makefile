# makefile to build sample program

.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk

SOURCES = Format.cs

all: writer.log reader.log 

reader.log : Reader.exe 
	Reader > reader.log

writer.log : Writer.exe 
	Writer > writer.log

Reader.exe Writer.exe : Reader.cs Writer.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_xml.InfoObject.Reader *.cs
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Writer.exe /main:Sample_xml.InfoObject.Writer *.cs

$(SOURCES) : test.asn
	$(ASN1C) test.asn -xml -csharp -print -genjsources -pkgname Sample_xml.InfoObject -trace -targetNS "http://www.obj-sys.com"
	$(ASN1C) test.asn -xsd -targetNS "http://www.obj-sys.com"

clean:
	$(MV) Reader.cs Reader.cs_save
	$(MV) Writer.cs Writer.cs_save
	$(RM) *.cs 
	$(RM) *.exe
	$(RM) *.dll
	$(RM) *.pdb
	$(RM) *~
	$(RM) writer*.log 
	$(RM) reader*.log
	$(MV) Reader.cs_save Reader.cs
	$(MV) Writer.cs_save Writer.cs

