.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk
SOURCES = PersonnelRecord.cs

all: writer.log reader.log readeri.log

writer.log message.dat : Writer.exe
	Writer > writer.log

reader.log : Reader.exe message.dat
	Reader > reader.log

Reader.exe Writer.exe : Reader.cs Writer.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_ber.Real.Reader *.cs
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Writer.exe /main:Sample_ber.Real.Writer *.cs

# indefinite length test

readeri.log : Reader.exe messagei.dat
	Reader -i messagei.dat > readeri.log

messagei.dat : message.dat ../../../bin/ber2indef$(EXE)
	$(BER2INDEF) message.dat messagei.dat

$(SOURCES) : realTest.asn
	$(ASN1C) realTest.asn -ber -csharp -print -nspfx Sample_ber -gencssources

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

