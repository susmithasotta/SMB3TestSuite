# makefile to build sample program

.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk

SOURCES = PDUs.cs

all: writer.log reader.log

reader.log : Reader.exe
	Reader > reader.log

writer.log : Writer.exe
	Writer > writer.log

Reader.exe Writer.exe : Reader.cs Writer.cs $(SOURCES)
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
	csc /nologo /optimize /warn:0 /lib:..\.. /reference:asn1rt.dll /out:Reader.exe /main:Sample_ber.SnmpV2.Reader *.cs
	csc /nologo /optimize /warn:0 /lib:..\.. /reference:asn1rt.dll /out:Writer.exe /main:Sample_ber.SnmpV2.Writer *.cs

$(SOURCES) : rfc2578-smiv2.asn rfc1905-snmpv2.asn udpMib.asn 
	$(ASN1C) rfc2578-smiv2.asn -asnstd x208 -ber -csharp -print -namespace Sample_ber.SnmpV2 -gencssources 
	$(ASN1C) rfc1905-snmpv2.asn -asnstd x208 -ber -csharp -print -namespace Sample_ber.SnmpV2 \
	   -config snmpV2.cfg  -gencssources 
	$(ASN1C) udpMib.asn -asnstd x208 -ber -csharp -print -namespace Sample_ber.SnmpV2 \
	   -config snmpV2.cfg -noencode -nodecode -gencssources 

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

