using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_ber.ToXML
{
	
	public class Reader
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = new System.Text.StringBuilder("message.dat").ToString();
			
			// Process command line arguments
			
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].Equals("-v"))
						Diag.Instance().SetEnabled(true);
					else if (args[i].Equals("-i"))
						filename = new System.Text.StringBuilder(args[++i]).ToString();
					else if (args[i].Equals("-notrace")) { continue; }
					else
					{
						System.Console.Out.WriteLine("usage: Reader [ -v ] [ -i <filename> ]");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -i <filename>  " + "read encoded msg from <filename>");
						return ;
					}
				}
			}
			
			try
			{
				// Create an input file stream object
				
				System.IO.FileStream ins = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				
				// Create a Decode buffer object
				
				Asn1BerDecodeBuffer decodeBuffer = new Asn1BerDecodeBuffer(ins);
				
				// Register event handler object
				
				XMLHandler xmlHandler = new XMLHandler("PersonnelRecord");
				// XMlHandler constrcutor will add the start tag 
            // (i.e. "<PersonnelRecord>")

            decodeBuffer.AddNamedEventHandler(xmlHandler);
				
				// Read and Decode the message
				
				PersonnelRecord personnelRecord = new PersonnelRecord();
				
				personnelRecord.Decode(decodeBuffer);
				
				// XMlHandler destrcutor will add the end tag 
            // (i.e. "</PersonnelRecord>")
            // Or can be added through Finish() method
				
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(e.Message);
				Asn1Util.WriteStackTrace(e, Console.Error);
				return ;
			}
		}
	}
}