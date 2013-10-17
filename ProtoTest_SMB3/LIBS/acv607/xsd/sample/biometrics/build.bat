del *.xsd

..\..\..\bin\asn1c biometrics.asn -xsd -pdu "BiometricSyntax" -tags

..\..\lib\validate.bat -ibm *.xsd 
