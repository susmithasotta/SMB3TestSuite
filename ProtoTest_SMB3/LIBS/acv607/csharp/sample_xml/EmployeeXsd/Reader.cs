using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_xml.EmployeeXsd
{
   
   public class Reader
   {
      [STAThread]
      public static void  Main(System.String[] args)
      {
         System.String filename = "message.xml";
         bool trace = true;
         
         // Process command line arguments
         
         if (args.Length > 0)
         {
            for (int i = 0; i < args.Length; i++)
            {
               if (args[i].Equals("-v"))
                  Diag.Instance().SetEnabled(true);
               else if (args[i].Equals("-i"))
                  filename = 
                     new System.Text.StringBuilder(args[++i]).ToString();
               else if (args[i].Equals("-notrace"))
                  trace = false;
               else {
                  Usage();
                  return ;
               }
            }
         }
         
         try
         {
            // Create an XML reader object
            
            XmlSaxParser reader = XmlSaxParser.NewInstance();
            
            // Read and decode the message
            
            PersonnelRecord personnelRecord = new PersonnelRecord();
            personnelRecord.Decode(reader, filename);
            if (trace)
            {
               System.Console.Out.WriteLine("Decode was successful");
               personnelRecord.Print("personnelRecord");
            }
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
            ("usage: Reader [ -v ] [ -i <filename> ]" + "[ -notrace ]");
         System.Console.Out.WriteLine
            ("   -v  verbose mode: print trace info");
         System.Console.Out.WriteLine
            ("   -i <filename>  " + "read encoded msg from <filename>");
         System.Console.Out.WriteLine
            ("   -notrace  do not display trace info");
      }
   }
}
