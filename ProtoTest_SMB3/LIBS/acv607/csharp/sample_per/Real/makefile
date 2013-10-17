# makefile to build the realTest sample program

.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk

SOURCES = PersonnelRecord.cs

all: writer_a.log reader_a.log writer_u.log reader_u.log

reader_a.log : Reader.exe 
	Reader -a > reader_a.log

writer_a.log : Writer.exe 
	Writer -a > writer_a.log

reader_u.log : Reader.exe 
	Reader -u > reader_u.log

writer_u.log : Writer.exe 
	Writer -u > writer_u.log

Reader.exe Writer.exe : Reader.cs Writer.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_per.Real.Reader *.cs
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Writer.exe /main:Sample_per.Real.Writer *.cs

$(SOURCES) : realTest.asn
	$(ASN1C) realTest.asn -per -csharp -print -nspfx Sample_per -trace -gencssources

clean:
	$(MV) Reader.cs Reader.cs_save
	$(MV) Writer.cs Writer.cs_save
	$(RM) *.cs 
	$(MV) Reader.cs_save Reader.cs
	$(MV) Writer.cs_save Writer.cs
	$(RM) *.dll
	$(RM) *.pdb
	$(RM) *.exe
	$(RM) *.log
	$(RM) *~

