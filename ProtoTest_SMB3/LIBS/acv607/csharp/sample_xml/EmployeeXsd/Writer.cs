using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_xml.EmployeeXsd
{
   public class Writer
   {
      [STAThread]
      public static void  Main(System.String[] args)
      {
         string filename = "message.xml";
         bool trace = true;
         
         // Process command line arguments
         
         if (args.Length > 0)
         {
            for (int i = 0; i < args.Length; i++)
            {
               if (args[i].Equals("-v"))
                  Diag.Instance().SetEnabled(true);
               else if (args[i].Equals("-o"))
                  filename = args[++i].ToString();
               else if (args[i].Equals("-notrace"))
                  trace = false;
               else {
                  Usage();
                  return ;
               }
            }
         }
         
         // Create a data object and populate it with the data to be encoded
         
         PersonnelRecord personnelRecord = new PersonnelRecord();
         personnelRecord.name = new Name ("John", "P", "Smith");
         personnelRecord.title = new Asn1UTF8String ("Director");
         personnelRecord.number = new EmployeeNumber (51);
         personnelRecord.dateOfHire = new Date ("19710917");
         personnelRecord.nameOfSpouse = new Name ("Mary", "T", "Smith");
         personnelRecord.children = new ChildInformation[2];
         personnelRecord.children[0] = 
            new ChildInformation (new Name("Ralph", "T", "Smith"), "19571111");
         personnelRecord.children[1] = 
            new ChildInformation (new Name("Susan", "B", "Jones"), "19590717");
         
         // Create a message buffer object and encode the record
         
         Asn1XmlEncodeBuffer encodeBuffer = new Asn1XmlEncodeBuffer();
         
         try
         {
            personnelRecord.Encode(encodeBuffer);
            
            if (trace)
            {
               System.Console.Out.WriteLine("Encoding was successful");
               encodeBuffer.Write(System.Console.OpenStandardOutput());
            }
            
            // Write the encoded record to a file
            
            encodeBuffer.Write
               (new System.IO.FileStream(filename, System.IO.FileMode.Create));
         }
         catch (System.Exception e)
         {
            System.Console.Out.WriteLine(e.Message);
            Asn1Util.WriteStackTrace(e, Console.Error);
            return ;
         }
      }

      static private void Usage ()
      {
         System.Console.Out.WriteLine
            ("usage: Writer [-v] " + "[-o <filename>]");
         System.Console.Out.WriteLine
            ("   -v  verbose mode: print trace info");
         System.Console.Out.WriteLine
            ("   -o <filename>  " + "write encoded msg to <filename>");
         System.Console.Out.WriteLine
            ("   -notrace  do not display trace info");
      }
   }
}
