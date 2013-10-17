.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk

SOURCES = Notification.cs

all: writer.log reader.log

reader.log : Reader.exe
	Reader > reader.log

writer.log : Writer.exe input.dat
	Writer > writer.log

Reader.exe Writer.exe : Reader.cs Writer.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_ber.TAP0311Batch.Reader *.cs
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Writer.exe /main:Sample_ber.TAP0311Batch.Writer *.cs


$(SOURCES) : TAP-0311.asn
	$(ASN1C) TAP-0311.asn -ber -csharp -print -compact -stream \
	-namespace Sample_ber.TAP0311Batch -gencssources

clean:
	$(MV) Reader.cs Reader.cs_save
	$(MV) Writer.cs Writer.cs_save
	$(RM) *.cs 
	$(RM) *.exe
	$(RM) *.dll
	$(RM) *.pdb
	$(RM) *~
	$(RM) writer.log 
	$(RM) reader.log
	$(MV) Reader.cs_save Reader.cs
	$(MV) Writer.cs_save Writer.cs

