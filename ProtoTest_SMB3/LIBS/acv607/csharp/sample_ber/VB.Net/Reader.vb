Imports System
Imports Com.Objsys.Asn1.Runtime


Namespace Sample_ber.Employee
   
   Public Class Reader
           
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
                  If args(i).Equals("-i") Then
                     i = i + 1
                     filename = New System.Text.StringBuilder(args(i)).ToString()
                  Else
                     If args(i).Equals("-notrace") Then
                        trace = False
                     Else
                        System.Console.Out.WriteLine("usage: Reader [ -v ] [ -i <filename> ]")
                        System.Console.Out.WriteLine("   -v  verbose mode: print trace info")
                        System.Console.Out.WriteLine(("   -i <filename>  " + "read Encoded msg from <filename>"))
                        System.Console.Out.WriteLine("   -notrace  do not display trace info")
                        Return
                     End If
                  End If
               End If
            Next i 
         End If
         Try

            ' Create an input file stream object
            Dim ins As New System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read)
            
            ' Create a Decode buffer object
            Dim decodeBuffer As New Asn1BerDecodeBuffer(ins)
            
            ' Read and Decode the message
            Dim personnelRecord As New PersonnelRecord()
            personnelRecord.Decode(decodeBuffer)
            If trace Then
               System.Console.Out.WriteLine("Decode was successful")
               personnelRecord.Print("personnelRecord")
            End If
         Catch e As System.Exception
            System.Console.Out.WriteLine(e.Message)
            Asn1Util.WriteStackTrace(e, Console.Error)
            Return
         End Try
      End Sub 'Main
   End Class 'Reader
End Namespace 'Sample_ber.Employee
