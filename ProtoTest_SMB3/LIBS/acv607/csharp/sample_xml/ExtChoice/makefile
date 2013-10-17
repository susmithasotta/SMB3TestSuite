# makefile to build the employee sample program

.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk

SOURCES = ExtChoice.cs

all: writer.log reader.log 

reader.log : Reader.exe 
	Reader > reader.log

writer.log : Writer.exe 
	Writer > writer.log

Reader.exe Writer.exe : Reader.cs Writer.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_xml.ExtChoice.Reader *.cs
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Writer.exe /main:Sample_xml.ExtChoice.Writer *.cs

$(SOURCES) : extChoice.asn
	$(ASN1C) extChoice.asn -xml -csharp -print -genjsources -pkgpfx Sample_xml -trace -targetNS "http://www.obj-sys.com"
	$(ASN1C) extChoice.asn -xsd -targetNS "http://www.obj-sys.com"

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

