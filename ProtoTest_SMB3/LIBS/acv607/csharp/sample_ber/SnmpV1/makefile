# makefile to build sample program

.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk

SOURCES = _RFC1213_MIBValues.cs

all: writer.log reader.log

reader.log : Reader.exe
	Reader > reader.log

writer.log : Writer.exe
	Writer > writer.log

Reader.exe Writer.exe : Reader.cs Writer.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_ber.SnmpV1.Reader *.cs
	csc /nologo /optimize /lib:..\.. /reference:asn1rt.dll /out:Writer.exe /main:Sample_ber.SnmpV1.Writer *.cs

$(SOURCES) : RFC1155-SMI.asn RFC1157-SNMP.asn RFC1213-MIB2.asn 
	$(ASN1C) RFC1155-SMI.asn -asnstd x208 -ber -csharp -print -namespace Sample_ber.SnmpV1 -gencssources
	$(ASN1C) RFC1157-SNMP.asn -asnstd x208 -ber -csharp -print -namespace Sample_ber.SnmpV1 -gencssources
	$(ASN1C) RFC1213-MIB2.asn -asnstd x208 -ber -csharp -print -namespace Sample_ber.SnmpV1 -gencssources

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

