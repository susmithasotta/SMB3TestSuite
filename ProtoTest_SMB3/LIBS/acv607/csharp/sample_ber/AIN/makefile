.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk
SOURCES = AINDigits.java

all: AIN.dll

AIN.dll: $(SOURCES)
	csc /nologo /warn:0 /debug /lib:..\.. /reference:asn1rt.dll\
	/target:library /out:AIN.dll *.cs

$(SOURCES) : ainop.asn
	$(ASN1C) ainop.asn -ber -csharp -print -asnstd x208 -namespace Sample_ber.AIN

clean:
	$(RM) *.cs 
	$(RM) *.dll 
	$(RM) *.pdb
	$(RM) *.exe
	$(RM) *~

