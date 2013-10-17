Imports System
Imports Com.Objsys.Asn1.Runtime

Namespace Sample_per.Employee
   
   Public Class Writer
      
      Overloads Public Shared Sub Main(args() As System.String)
         Dim filename As System.String = New System.Text.StringBuilder("message.dat").ToString()
         Dim aligned As Boolean = True
         Dim trace As Boolean = True
         
         ' Process command line arguments
         If args.Length > 0 Then
            Dim i As Integer
            For i = 0 To args.Length - 1
               If args(i).Equals("-v") Then
                  Diag.Instance().SetEnabled(True)
               Else
                  If args(i).Equals("-o") Then
                     i = i +1
		     filename = New System.Text.StringBuilder(args(i)).ToString()
                  Else
                     If args(i).Equals("-a") Then
                        aligned = True
                     Else
                        If args(i).Equals("-u") Then
                           aligned = False
                        Else
                           If args(i).Equals("-notrace") Then
                              trace = False
                           Else
                              System.Console.Out.WriteLine(("usage: Writer [ -a | -u ] [ -v ] " + "[ -o <filename>"))
                              System.Console.Out.WriteLine("   -a  PER aligned encoding (default)")
                              System.Console.Out.WriteLine("   -u  PER unaligned encoding")
                              System.Console.Out.WriteLine("   -v  verbose mode: print trace info")
                              System.Console.Out.WriteLine(("   -o <filename>  " + "write encoded msg to <filename>"))
                              System.Console.Out.WriteLine("   -notrace  do not display trace info")
                              Return
                           End If
                        End If
                     End If
                  End If 
               End If ' Create a data object and populate it with the data to be encoded
            Next i
         End If
         Dim personnelRecord As New PersonnelRecord()
         personnelRecord.name = New Name("John", "P", "Smith")
         personnelRecord.title = New Asn1IA5String("Director")
         personnelRecord.number = New EmployeeNumber(51)
         personnelRecord.dateOfHire = New [Date]("19710917")
         personnelRecord.nameOfSpouse = New Name("Mary", "T", "Smith")
         personnelRecord.children = New _SeqOfChildInformation(2)
         personnelRecord.children.elements(0) = New ChildInformation(New Name("Ralph", "T", "Smith"), "19571111")
         personnelRecord.children.elements(1) = New ChildInformation(New Name("Susan", "B", "Jones"), "19590717")
         
         ' Create a message buffer object 
         Dim encodeBuffer As New Asn1PerEncodeBuffer(aligned)
         
         ' Enable bit field tracing
         If trace Then
            encodeBuffer.TraceHandler.Enable()
         End If
         
         ' Encode
         Try
            personnelRecord.Encode(encodeBuffer)
            
            If trace Then
               System.Console.Out.WriteLine("Encoding was successful")
               System.Console.Out.WriteLine("Hex dump of encoded record:")
               encodeBuffer.HexDump()
               System.Console.Out.WriteLine("Binary dump:")
               encodeBuffer.BinDump("personnelRecord")
            End If
            
            ' Write the encoded record to a file
            encodeBuffer.Write(New System.IO.FileStream(filename, System.IO.FileMode.Create))
            
            ' Generate a dump file for comparisons
            Dim fileSuffix As System.String
            If aligned Then
               fileSuffix = "a"
            Else
               fileSuffix = "u"
            End If
            Dim messagedmp As New System.IO.StreamWriter(New System.IO.FileStream("message" + fileSuffix + ".dmp", System.IO.FileMode.Create))
            messagedmp.AutoFlush = True
            encodeBuffer.HexDump(messagedmp)
         Catch e As System.Exception
            System.Console.Out.WriteLine(e.Message)
            Asn1Util.WriteStackTrace(e, Console.Error)
            Return
         End Try
      End Sub 'Main
   End Class 'Writer
End Namespace 'Sample_per.Employee
