.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk
SOURCES = MakeCallArgument.java

all: writer.log reader.log readeri.log

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
	cd ..\SimpleROSE
	$(MAKE)
	cd ..\CSTA
	$(COPY) ..$(PS)SimpleROSE$(PS)SimpleROSE.dll .
	csc /nologo /optimize /lib:..\..,..\SimpleROSE /reference:asn1rt.dll,SimpleROSE.dll /out:Reader.exe /main:Sample_ber.CSTA.Reader *.cs
	csc /nologo /optimize /lib:..\..,..\SimpleROSE /reference:asn1rt.dll,SimpleROSE.dll /out:Writer.exe /main:Sample_ber.CSTA.Writer *.cs

$(SOURCES) : makeCallArgument.asn 
	$(ASN1C) makeCallArgument.asn -ber -csharp -print -namespace Sample_ber.CSTA

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

