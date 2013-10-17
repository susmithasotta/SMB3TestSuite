del *.xsd

..\..\..\bin\asn1c employee.asn -xsd -pdu "PersonnelRecord"

..\..\lib\validate.bat -ibm *.xsd 
