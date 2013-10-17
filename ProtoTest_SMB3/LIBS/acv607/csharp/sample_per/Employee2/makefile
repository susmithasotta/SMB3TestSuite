# makefile to build the employee sample program

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
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_per.Employee2.Reader *.cs
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Writer.exe /main:Sample_per.Employee2.Writer *.cs

$(SOURCES) : employee.asn
	$(ASN1C) employee.asn -per -csharp -print -namespace Sample_per.Employee2 -trace -gencssources

clean:
	$(MV) Reader.cs Reader.cs_save
	$(MV) Writer.cs Writer.cs_save
	$(RM) *.cs 
	$(RM) *.dll
	$(RM) *.pdb
	$(RM) *.exe
	$(RM) *~
	$(RM) *.log
	$(MV) Reader.cs_save Reader.cs
	$(MV) Writer.cs_save Writer.cs

