del *.xsd

..\..\..\bin\asn1c extChoice.asn -xsd -pdu "AliasAddressList"

..\..\lib\validate.bat -ibm *.xsd 
