ConsoleApplication.sln is a Visual Studio .Net 2003 solution. 
It is able to encode/decode the message for employee ASN.1 definition.

This solution also displays how to use the C# code with VB.Net
application.

First User will require to execute the writer project, which will
create a binary message from text data/structure. Than execute the
reader project, which will decode the binary message to text 
data/structure.

========================================================================
Employee Project Overview
========================================================================

This project calls ASN1C compiler to generate the C# code 
employee ASN.1 definition. Look up project property to change the ASN1C option.
The ASN1C compiler will produce the PersonnelRecord.cs,
Name.cs, EmployeeNumber.cs, Date.cs, ChildInforamation.cs,
_EmployeeValues.cs, _SeqOfChildInformation.cs files.
That can be used to encode/decode/print the ASN.1 definition strcuture.

The build.bat file can also be used to genearte C# code for ASN.1
definition.

Than creates the runtime library of this generated code. That can
be used by writer project to encode the binary message & reader project
to decode the binary message.

Employee.vcproj
    It contains information about the version of Visual C# that generated the file, and 
    information about the platforms, configurations, and project features selected with the
    Application Wizard.
    
   
PersonnelRecord.cs,
Name.cs, 
EmployeeNumber.cs
Date.cs, 
ChildInforamation.cs,
_EmployeeValues.cs,
_SeqOfChildInformation.cs
    This files are produced by ASN1C compiler for employee.asn definition.
    These files are used to encode/decode the message for employee ASN.1
    definition.
   
========================================================================
Writer Project Overview
========================================================================

This project can be used to encode the employee definition message.  

This file contains a summary of what you will find in each of the files that
make up your Writer application.

Writer.vbproj
    It contains information about the version of Visual Basic.Net that generated the file, and 
    information about the platforms, configurations, and project features selected with the
    Application Wizard.

Writer.vb
    This is the main application source file to encode 
    employee definition binary message. User will require to 
    write this file.

========================================================================
    APPLICATION : Reader Project Overview
========================================================================

This project can be used to decode the employee definition message.  

This file contains a summary of what you will find in each of the files that
make up your Reader application.

Reader.vbproj
    It contains information about the version of Visual Basic.Net that generated the file, and 
    information about the platforms, configurations, and project features selected with the
    Application Wizard.

Reader.vb
    This is the main application source file to decode employee 
    definition binary message. User will require to 
    write this file.



/////////////////////////////////////////////////////////////////////////////
