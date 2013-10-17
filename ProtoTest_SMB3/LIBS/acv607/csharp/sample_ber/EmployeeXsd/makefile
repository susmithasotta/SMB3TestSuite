.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk

SOURCES = PersonnelRecord.cs

all: writer.log reader.log 

reader.log : Reader.exe
	Reader > reader.log

writer.log message.dat : Writer.exe
	Writer > writer.log

# indefinite length test

readeri.log : Reader.exe messagei.dat
	Reader -i messagei.dat > readeri.log

messagei.dat : message.dat ../../../bin/ber2indef$(EXE)
	$(BER2INDEF) message.dat messagei.dat

Reader.exe Writer.exe : Reader.cs Writer.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_ber.EmployeeXsd.Reader *.cs
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Writer.exe /main:Sample_ber.EmployeeXsd.Writer *.cs

$(SOURCES) : Employee.xsd
	$(ASN1C) Employee.xsd -ber -csharp -print -namespace Sample_ber.EmployeeXsd

clean:
	$(MV) Reader.cs Reader.cs_save
	$(MV) Writer.cs Writer.cs_save
	$(RM) *.cs 
	$(RM) *.exe
	$(RM) *.dll
	$(RM) *.pdb
	$(RM) *~
	$(RM) *.log
	$(RM) *.user 
	$(RM) *.mk 
	$(RM) *.xml 
	$(RM) *.bat 
	$(RM) *.suo
	$(MV) Reader.cs_save Reader.cs
	$(MV) Writer.cs_save Writer.cs

