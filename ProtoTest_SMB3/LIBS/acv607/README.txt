	  
ASN1C V6.0 README 
This file contains release notes on the latest release of the ASN1C
compiler (version 6.0x).  
Contents 
	  
Introduction 
Release Notes 
Compatibility 
Documentation 
Windows Installation 
Contents of the Release 
Getting Started with C or C++ 
Getting Started with Java 
Getting Started with C# 
Reporting Problems 
	  
Introduction 
Thank you for downloading this release of the ASN1C software.
ASN1C is a powerful ASN.1 compiler capable of parsing and generating C, C++,
C#, or Java source code for the most advanced ASN.1 syntax. 
This package contains the ASN1C compiler executables and run-time
libraries. Documentation is available online at
		  
http://www.obj-sys.com/docs/documents.shtml  
		  
Release Notes 
This release of ASN1C adds the following new capabilities: 
		 
Compilation of XML Schema (XSD) Files  
The capability to generate code for XSD files has been added in
addition to ASN.1 files. This is accomplished by doing an internal
transformation of XSD-to-ASN.1 as specified in the X.694 standard. The
resulting types and encoders/decoders make it possible to encode data in
W3C-compliant XML form as well as the fast, efficient binary forms as specified
in the ASN.1 encoding rules standards (BER, DER, CER, or PER). 
			   
New Cross-Platform GUI  
A new GUI Wizard has been added in this release that works on Linux
and some UNIX platforms in addition to Windows. The layout of the GUI has been
changed in an attempt to group related options in a logical way. Source code
for the GUI is included in the package to allow customization to a particular
style if desired. 
			   
Use of an XML Pull-Parser for C/C++ Decoding  
The generated C/C++ XML code has been changed to use a custom-built
pull parser for C/C++ decoding of XML files. This is commonly known in other
XML libraries as a reader interface. This replaces the generation of SAX
handling code which proved to be awkward in preserving state between
operations. The newly generated code is much more efficient and performs much
better than the SAX code of past releases. 
Note that this just refers to code generated when the -xml
option is specified, not -xer. Generated XER code still uses the SAX
interface. 
			  
Redesign of C++ Encode/Decode Stream Classes  
C++ stream classes for BER/DER/CER and XER were redesigned to use
common stream classes internally instead of inherting from them. This provided
a cleaner interface for working with the streams and separated the
encode/decode functionality for the individual encoding rules from the stream
transport mechanics. 
			   
Generation of Visual Studio project files  
The generate VC project option (-vcproj) has been added to
allow generation of Visual Studio project files. These allow a user who is
familair with Visual Studio to get up and running quickly by generating both
the code and project files necessary to build the project from a given set of
XSD or ASN.1 schemas. 
			   
Additional Makefile / VC Project options  
The DLL (-dll) and multi-threaded (-mt) options make
it possible to generate makefiles and/or Visual Studio project files that will
link against DLL's or add multi-threaded compialtion options respectively. 
			   
		 
Compatibility 
In an ongoing effort to improve the product, changes have been
made in how code is generated in some cases. Version 6.0 is a major new release
level and, as such, much has changed. In order to implement the compilation of
XSD files, much of the code from our XBinder project had to be
integrated with ASN1C. This resulted in a new name scheme for common types and
for the use of many internal run-time functions. 
Users of previous versions of the compiler can achieve backward
compatility with their existing code bases in several different ways: 
		 
By running a Perl script - rtport.pl - that is included in
the top level directory of the installation. This will edit an existing source
file and change all of the names necessary to make it compatible with the 6.0
version of ASN1C. 
By adding the asn1compat.h header file to an existing
application that uses ASN1C generated or run-time code. This header file
contains macro definitions that convert the old names into the new naming
scheme. 
By using the compiler -compat switch (for example, '-compat
5.6' will generate code compatible with the 5.6 version of the compiler). Note
that for version 6.0, it will also be necessary to do step 1 or 2 in addition
to using -compat to generate compatible code. 
		  
Documentation 
Documentation for release is available online at the following
web-link: 
		
http://www.obj-sys.com/docs/documents.shtml  
Windows Installation 
	  
The steps to install ASN1C on a Windows system are as follows: 
		 
Download the ASN1C package that is of interest to you. Packages are
available online for C/C++, C#, and Java. The C/C++ package contains run-time
libraries built with the Microsoft Visual C++ v6.0, v7.0 (.NET 2003), and v8.0
(VS 2005) compilers, the Borland v5.5 C++ compiler, and the GNU Cygwin 3.3.1
compiler. 
			  
ASN1C for Windows is packaged in a self-extracting executable file
format. To install, all that should be necessary is to double-click this file
after downloading and then following the setup wizard instructions from that
point. 
			  
After installation is complete, the license file must be installed
to allow the product to operate. This was sent in the osyslic.txt file
that was attached to the E-mail message that was sent at the time the product
was downloaded. If you did not receive a license file, please contact us. 
			  
The osyslic.txt file must be copied to a location where
the compiler can find it. This can be done in one of the following ways: 
			  
			  
a. The file can be copied to the same directory that the
ASN1C compiler executable file is located in. This is in the bin subdirectory
located under the top-level install directory. This is the preferred option as
it keeps the license separate from other similar license files that may be
present on a given computer system. 
b. The file can be copied into any of the directories
specified within your PATH environment variable (copying to the c:\windows
directory works on most systems), or  
c. The file can be copied to a different directory and
an environment variable named OSLICDIR created to point at this
directory location. 
			  
Note that licenses from versions of ASN1C older than 5.7 are not
compatible with this release of the product.  
The compiler should now be operational. The following command can
be executed:  
<rootdir>\bin\asn1c  
to verify operation.  
		 
Contents of the Release 
The following subdirectories contain the following files
(note: <installdir> refers to the installation directory that
was specified during the installation process): 
Base Compiler Package 
		 
<installdir>\bin\asn1c.exe  
The command-line compiler executable file. This is invoked on ASN.1
or XSD source files to generate C, C++, C#, or Java encode/decode classes and
functions. It is recommended you modify your PATH environment variable to
include <installdir>\bin to allow the compiler executable to be
run from anywhere. 
			  
<installdir>\bin\asn1cGUI.exe  
The compiler graphic user interface (GUI) wizard executable file.
This GUI guides a user through the process of specifying ASN.1 or XSD source
files and options. This is the program invoked from the start menu or desktop
icon. 
			  
<installdir>\bin\berfdump.exe
<installdir>\bin\ber2def.exe
<installdir>\bin\ber2indef.exe  
Utility programs for operating on BER-encoded files. The first
program allows a file to be dumped in a human-readable format. The other two
utilities convert files from the use of indefinite to definite length encoding
and vice-versa. 
			  
<installdir>\bin\dumpasn1.exe  
A public-domain ASN.1 BER/DER encoded data dump tool. Thanks to
Peter Gutmann for making this available for public use. The full source code
for this program can be found in the utils subdirectory of the installation. 
			  
<installdir>\bin\xsd2asn1.exe  
XSD-to-ASN.1 translation program executable file. This program
translates an XSD file to its ASN.1 equivalent as per the ITU-T X.694 standard.
			 
			  
<installdir>\doc  
This directory contains documentation files. Note that the bulk of
the documentation items must be downloaded as a separate package (see the
Documentation section above). 
			  
<installdir>\scripts  
This directory contains Perl script files for doing source code
editing and other transformations. The rtport.pl script is included in
this release to port existing applications that use ASN1C generated code to be
compatible with the latest release of the product. 
			  
<installdir>\utils  
This directory contains the source code and build makefile for some
of the utility programs included in the bin subdirectory. 
			  
<installdir>\xsd\lib\asn1.xsd  
This directory contains the common XML schema definitions (XSD)
library. This contains type mappings for built-in ASN.1 types that do not have
an equivalent types defined in XSD. 
			  
<installdir>\xsd\sample  
This directory contains sample programs related to the conversion
of ASN.1 to XML Schema.  
		 
C/C++ run-time libraries and source files 
		 
<installdir>\c\lib\* (Visual C++ 6.0)  
<installdir>\c_mscv7\lib\* (Visual C++ 7.1 / .NET
2003)  
<installdir>\c_mscv8\lib\* (Visual C++ 8 / .NET
2005)  
<installdir>\c_gnu\lib\*.a (GNU gcc 3.3.1)  
The ASN1C C run-time library files. These contain BER/DER, PER,
XER, and XML run-time functions. For each encoding rules type, there is a
dynamic link library (.dll) and standard library file (.lib) for linking with
the DLL. There is also a static library for direct linkage to the object
modules (this is the library file with the '_a.lib' suffix). The static library
included in the evaluation version is not thread-safe. The licensed version of
the product also contains a thread-safe static library (compiled with -MT
option) and a DLL-ready library (compiled with -MD option) for building your
own value-added DLL's. Also note that the evaluation and development libraries
are not fully optimized (they contain diagnostic tracing and are not compiled
with compiler optimization turned on). The deployment libraries are fully
optimized. 
			  
<installdir>\cpp\lib\*.lib (Visual C++ 6.0)  
<installdir>\cpp_mscv7\lib\*.lib (Visual C++ 7.1 /
			 .NET 2003)  
<installdir>\cpp_mscv8\lib\*.lib (Visual C++ 8.0 /
			 .NET 2005)  
<installdir>\cpp_borland\lib\*.lib (Borland C++)
			  
<installdir>\cpp_gnu\lib\*.a (GNU g++ 3.3.1)  
The ASN1C C/C++ run-time library files. These are the same as the C
run-time libraries above except they contain run-time C++ classes as well as C
run-time functions. 
			  
<installdir>\c*\lib_opt\*  
<installdir>\cpp*\lib_opt\*  
The optimized version of the ASN1C run-time libraries. This version
has all diagnostic messages, error stack trace and text, and non-essential
status checks removed. (Note: these libraries are only available in the
licensed deployment version of the product). 
			  
<installdir>\c*\lib_debug\*  
<installdir>\cpp*\lib_debug\*  
The debug DLL versions of the ASN1C run-time libraries. These are
the same as the DLL C/C++ run-time libraries above except they are linked with
debug versions of Standard C Run-time DLLs. (Note: these libraries are only
available in the licensed development version of the product (SDK)). 
			  
<installdir>\c\sample_*  
<installdir>\cpp\sample_*  
The sample directories contain sample programs demonstrating the
use of the compiler. There are a set of sample programs that correspond to each
encoding rule set supported by ASN1C. Most sample programs are broken down into
a writer and a reader. The writer encodes a sample data record and writes it to
a disk file. The reader reads the encoded message from the file, decodes it,
and then prints the results of the decode operation. 
			  
<installdir>\rtsrc\*  
<installdir>\rtxsrc\*  
Run-time source directories containing common type and class
definitions used by all encoding rules. The installation run-time source
directories contain the header files required to compile the compiler generated
code. The C or C++ source files will also be located here if the run-time
source code kit option was selected. 
			  
<installdir>\rtbersrc\*  
BER/DER/CER specific run-time source directories. These contain
common code for encoding/decoding BER, DER, or CER messages. 
			  
<installdir>\rtpersrc\*  
PER specific run-time source directories. These contain common code
for encoding/decoding PER messages. 
			  
<installdir>\rtxersrc\*  
<installdir>\rtxmlsrc\*  
XML specific run-time source directories. These contain common code
for encoding/decoding XER or XML messages. 
			  
<installdir>\expatsrc\*  
The XML parser run-time source directories contain the source files
for the Expat C XML parser.  
		 
Java run-time libraries 
		 
<installdir>\java\asn1rt.jar  
ASN.1 Java run-time libraries. These contain the low-level BER,
PER, and/or XER encode/decode classes. The asn1rt.jar file contains
classes compatible with the Java 2 JRE. 
			  
<installdir>\java\sample_ber
<installdir>\java\sample_der <installdir>\java\sample_per
<installdir>\java\sample_xer
<installdir>\java\sample_xml  
Sample programs illustrating the use of the Java version of ASN1C.
As was the case for C/C++, most have a writer and a reader. Some contain
support code used by other samples (for example, SimpleROSE contains the ROSE
headers used by CSTA). 
			  
<installdir>/java/doc/*  
The ASN.1 Java run-time libraries documentation files. These are
html files generated with the javadoc documentation tool. To view the
documentation, open the index.html file with a web browser and follow
the hyperlinks. 
			  
<installdir>/java/xerces/*  
The Apache Xerces Java XML parser implementation. This parser is
used in the generated XER and XML decode classes.
		 
C# run-time libraries 
		 
<installdir>\csharp\asn1rt.dll  
The ASN.1 C# run-time library DLL. This contains the low-level BER,
PER, and/or XER encode/decode classes. It supports .NET 2002 through 2005
versions. 
			  
<installdir>\csharp\sample_ber
<installdir>\csharp\sample_der
<installdir>\csharp\sample_per
<installdir>\csharp\sample_xer  
Sample programs demonstrating the use of the C# version of ASN1C.
As was the case for C/C++, most have a writer and a reader. Some contain
support code used by other samples (for example, SimpleROSE contains the ROSE
headers used by CSTA). 
			  
<installdir>/csharp/doc/*  
The ASN.1 C# run-time libraries documentation files. Documentation
is contained within the ASN1CLibrary.chm file. This is in Microsoft help
format.  
		 
Getting Started with C or C++ 
The compiler can be run using either the GUI wizard or from
the command line. To run the GUI wizard, launch the application and follow
these steps. To run a simple test from the command line, do the following: 
		 
		 
Open an MS-DOS or other command shell window. 
		 
		 
		 
Change directory (cd) to one of the employee sample directories
(for example, c/sample_ber/employee). 
		 
		 
		 
Execute the nmake utility program: 
nmake  
(note: nmake is a make utility program that comes with the
Microsoft Visual C++ compiler. It may be necessary to execute the batch file
vcvars32.bat that comes with Visual C++ in order to set up the
environment variables to use this utility). If you are using Borland or another
compiler, execute the make utility for that compiler.  
This should cause the compiler to be invoked to compile the
employee.asn sample file. It will then invoke the Visual C++ compiler
to compile the generated C file and the test drivers. The result should be a
writer.exe and reader.exe program file which, when invoked,
will encode and decode a sample employee record.  
It is also possible to compile the sample programs using the
Visual Studio IDE. Microsoft Visual C++ 6.0 workspace and project files are
included in most sample programs. Double-clicking the workspace file should
cause it to be opened and updated to be compatible with whatever version of
Visual Studio is being used. 
			  
Invoke writer from the command line: 
writer  
This will generate an encoded record and write it to a disk file.
By default, the file generated is message.dat (in the case of XER, it is
message.xml). The test program has a number of command line switches
that control the encoding. To view the switches, enter writer ? and a
usage display will be shown.  
			  
Invoke reader from the command line: 
reader  
This will read the disk file that was just created by the writer
program and decode its contents. The resulting decoded data will be written to
standard output. The test program has a number of command line switches that
control the encoding. To view the switches, enter reader ? and a usage
display will be shown.  
		 
Getting Started with Java 
The compiler can be run using either the GUI wizard or from
the command line. To run the GUI wizard, launch the application and follow the
steps. To run a simple test from the command line, do the following: 
		 
		 
Open an MS-DOS or other command shell window. 
			  
Change directory (cd) to one of the employee sample directories
(for example, java/sample_ber/Employee). 
			  
Execute the build batch file: 
build  
This will cause the ASN1C compiler to be invoked to compile the
employee.asn sample file. It will then invoke the Java compiler
(javac) to compile all generated java files and the reader and writer
programs (Note: JDK 1.4 was used to build all the run-time library classes). It
will also automatically execute the writer and reader programs. These programs
will produce a writer.log and reader.log file respectively.  
Note: a makefile is also available for use if you have a make
utility program available. The makefile is compatible with the GNU make utility
and with the Microsoft Visual C++ make utility (nmake).  
			  
View the writer and reader log files. The writer.log file will
contain a dump of the encoded message contents. The reader.log file will
contain a printout of the decoded data.  
		 
Getting Started with C# 
The compiler can be run using either the GUI wizard or from
the command line. To run the GUI wizard, launch the application and follow the
steps. To run a simple test from the command line, do the following: 
		 
		 
Make sure Microsoft .NET 2003 or 2005 is installed on your system.
			  
Open the Visual Studio .NET command prompt (This can be
found using: Start->Programs->Microsoft Visual Studio .NET->Visual
Studio .NET Tools) 
Execute the nmake command to run the complete sample
program. The makefile will invoke the ASN1C compiler to generate C# code for
the ASN.1 definition and then compile the generated C# code. 
Execute writer.exe to encode a binary message and write it
to a file. 
Execute reader.exe to read the file containing encoded
binary message and decode it. 
		 
Reporting Problems 
	  
Report problems you encounter by sending E-mail to
support@obj-sys.com. The preferred
format of example programs is the same as the sample programs. Please provide a
writer and reader and indicate where in the code the problem occurs.  
If you have any further questions or comments on what you would like
to see in the product or what is difficult to use or understand, please
communicate them to us. Your feedback is important to us. Please let us know
how it works out for you - either good or bad.  

