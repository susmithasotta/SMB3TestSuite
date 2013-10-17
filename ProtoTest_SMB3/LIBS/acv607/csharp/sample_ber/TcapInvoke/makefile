.SUFFIXES : .cs .exe

include ../../../platform.mk
include ../../../sample.mk


all: writer.log reader.log

reader.log : Reader.exe
	Reader > reader.log

writer.log : Writer.exe
	Writer > writer.log

Reader.exe Writer.exe : Reader.cs Writer.cs
	$(COPY) ..$(PS)..$(PS)asn1rt.dll .
# START UNIX
#	cd ../AIN  ; $(MAKE)
#	cd ../TCAP ; $(MAKE)
# END UNIX
# START WINDOWS
	cd ..\AIN
	$(MAKE)
	cd ..\TCAP
	$(MAKE)
	cd ..\TcapInvoke
# END WINDOWS
	$(COPY) ..$(PS)AIN$(PS)AIN.dll .
	$(COPY) ..$(PS)TCAP$(PS)TCAP.dll .
	csc /nologo /optimize /lib:..\..,..\AIN,..\TCAP	\
	/reference:asn1rt.dll,AIN.dll,tcap.dll /out:Reader.exe Reader.cs
	csc /nologo /optimize /lib:..\..,..\AIN,..\TCAP	\
	/reference:asn1rt.dll,AIN.dll,tcap.dll /out:Writer.exe Writer.cs


clean:
	$(RM) *.exe
	$(RM) *.dll
	$(RM) *.pdb
	$(RM) *~
	$(RM) writer.log 
	$(RM) reader.log

