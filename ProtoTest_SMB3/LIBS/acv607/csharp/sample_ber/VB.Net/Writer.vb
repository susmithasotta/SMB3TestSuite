Imports System
Imports Com.Objsys.Asn1.Runtime

Namespace Sample_ber.Employee
   
   Public Class Writer
            
      Overloads Public Shared Sub Main(args() As System.String)
         Dim filename As System.String = New System.Text.StringBuilder("message.dat").ToString()
         Dim trace As Boolean = True
         
         ' Process command line arguments
         If args.Length > 0 Then
            Dim i As Integer
            For i = 0 To args.Length - 1
               If args(i).Equals("-v") Then
                  Diag.Instance().SetEnabled(True)
               Else
                  If args(i).Equals("-o") Then
                     i = i + 1
                     filename = New System.Text.StringBuilder(args(i)).ToString()
                  Else
                     If args(i).Equals("-notrace") Then
                        trace = False
                     Else
                        System.Console.Out.WriteLine("usage: Writer [ -v ] [ -o <filename>")
                        System.Console.Out.WriteLine("   -v  verbose mode: print trace info")
                        System.Console.Out.WriteLine(("   -o <filename>  " + "write Encoded msg to <filename>"))
                        System.Console.Out.WriteLine("   -notrace  do not display trace info")
                        Return
                     End If
                  End If
               End If
            Next i
         End If

         ' Create a data object and populate it with the data to be Encoded
         Dim personnelRecord As New personnelRecord
         personnelRecord.name = New Name("John", "P", "Smith")
         personnelRecord.title = New Asn1IA5String("Director")
         personnelRecord.number = New EmployeeNumber(51)
         personnelRecord.dateOfHire = New [Date]("19710917")
         personnelRecord.nameOfSpouse = New Name("Mary", "T", "Smith")
         personnelRecord.children = New _SeqOfChildInformation(2)
         personnelRecord.children.elements(0) = New ChildInformation(New Name("Ralph", "T", "Smith"), "19571111")
         personnelRecord.children.elements(1) = New ChildInformation(New Name("Susan", "B", "Jones"), "19590717")

         ' Create a message buffer object and Encode the record
         Dim encodeBuffer As New Asn1BerEncodeBuffer

         Try
            personnelRecord.Encode(encodeBuffer, True)

            If trace Then
               System.Console.Out.WriteLine("Encoding was successful")
               System.Console.Out.WriteLine("Hex dump of Encoded record:")
               encodeBuffer.HexDump()
               System.Console.Out.WriteLine("Binary dump:")
               encodeBuffer.BinDump()
            End If

            ' Write the Encoded record to a file
            encodeBuffer.Write(New System.IO.FileStream(filename, System.IO.FileMode.Create))

            ' Generate a dump file for comparisons
            Dim messagedmp As New System.IO.StreamWriter(New System.IO.FileStream("message.dmp", System.IO.FileMode.Create))
            messagedmp.AutoFlush = True
            encodeBuffer.HexDump(messagedmp)
         Catch e As System.Exception
            System.Console.Out.WriteLine(e.Message)
            Asn1Util.WriteStackTrace(e, Console.Error)
            Return
         End Try
      End Sub 'Main
   End Class 'Writer
End Namespace 'Sample_ber.Employee
