del *.xsd

..\..\..\bin\asn1c PKCS15TokenInfo.asn -xsd -pdu "TokenInfo"

..\..\lib\validate.bat -ibm *.xsd 
