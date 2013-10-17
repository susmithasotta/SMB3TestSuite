@ECHO OFF
copy ..\..\lib\asn1.xsd .\

if '%1 == ' goto no-params
if %1 == help goto help

set xerces_dir=..\..\xmlParsers\xerces-2_2_0
set ibm_dir=..\..\xmlParsers\ibm_sqc-2_0
set msxml_dir=.
set xsv_dir=.

if %1 == -ibm goto ibm
if %1 == -xsv goto xsv
if %1 == -xerces goto xerces
if %1 == -msxml goto msxml
goto xerces

:xerces
   echo Running the xerces Schema Validator
   if not exist %2 goto no-file
   if not exist %xerces_dir% goto no-xerces

   set xerces_CLASSPATH=.;%xerces_dir%\xercesImpl.jar;%xerces_dir%\xmlParserAPIs.jar;%xerces_dir%\xercesSamples.jar
   java -classpath "%xerces_CLASSPATH%" sax.Counter -v -s -f %2

   echo ..
   echo Done.
   goto eof

:ibm
   echo Running the IBM Schema Validator
   if not exist %2 goto no-file
   if not exist %ibm_dir% goto no-ibm

   set ibm_CLASSPATH=.;%ibm_dir%\xschemaREC.jar;%ibm_dir%\xerces.jar;%ibm_dir%\xml4j.jar;%ibm_dir%\mofrt.jar;%ibm_dir%\regex4j.jar;%ibm_dir%\mail.jar;%ibm_dir%
   java -classpath "%ibm_CLASSPATH%" com.ibm.sketch.util.SchemaQualityChecker -indicateProgress %2

   echo ..
   echo Done.
   goto eof

:xsv
   echo Running the XSV Schema Validator
   if not exist %1 goto no-file
   if not exist %xsv_dir%\xsv.exe goto no-xsv

   %xsv_dir%\xsv %1

   echo ..
   echo Done.
   goto eof

:msxml
   echo Running the MSXML4.0 Schema Validator
   if not exist %msxml_dir%\xsdms.js goto no-msxml
   if not exist %2 goto no-file
   if not exist %3 goto no-xsd

   %msxml_dir%\xsdms.js %2 %3 %4

   echo ..
   echo Done.
   goto eof

:no-params
    echo Error! You have to provide following arguments to run the validation.
    goto help

:help
    echo ...
    echo Usage: "validate -xerces <xml filename>" to invoke the Xerces (Apache) Schema Validator
    echo Usage: "validate -ibm <xsd filename>" to invoke the IBM Schema Quality Checker 
    echo Usage: "validate -msxml <xml filename> <xsd filename>" to invoke the MSXML4.0 Schema Validator
    echo Usage: "validate -xsv <xml filename> <xsd filename>" to invoke the XSV Schema Validator
    echo ...
    goto eof

:no-file
   echo Error! Unable to open file %2 
   goto help

:no-java
   echo Error! Can not find java runtime environment
   echo Please check java is installed on your machine 
   goto eof

:no-xerces
   echo Error! Can not find xerces validator at %xerces_dir%
   echo Check xerces is installed or install directory name is correct
   goto eof

:no-xsv
   echo Error! Can not find xsv validator at %xsv_dir%\xsv.exe
   echo Check xerces is installed or install directory name is correct
   goto eof

:no-ibm
   echo Error! Can not find IBM Schema Validator at %ibm_dir%
   echo Check IBM Schema Validator is installed or install directory name is correct
   goto eof

:no-msxml
   echo Error! Can not find MSXML javascript file: %msxml%\xsdms.js
   echo This javascript file is needed to invoke the MSXML4.0 schema validator
   goto eof

:no-xsd
   echo Error! You must supply a xsd(xml schema) file to run Schema Validator
   goto help


:eof
   echo ...
   echo Exiting ...
