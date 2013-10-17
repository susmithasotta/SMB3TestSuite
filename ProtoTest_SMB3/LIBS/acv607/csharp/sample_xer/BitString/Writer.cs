using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_xer.BitString
{
	
	public class Writer
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = new System.Text.StringBuilder("message.xml").ToString();
			bool cxer = false, trace = true;
			
			// Process command line arguments
			
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].Equals("-v"))
						Diag.Instance().SetEnabled(true);
					else if (args[i].Equals("-o"))
						filename = new System.Text.StringBuilder(args[++i]).ToString();
					else if (args[i].Equals("-cxer"))
						cxer = true;
					else if (args[i].Equals("-notrace"))
						trace = false;
					else
					{
						System.Console.Out.WriteLine("usage: Writer [ -cxer ] [ -v ] " + "[ -o <filename>");
						System.Console.Out.WriteLine("   -cxer  Use Canonical XER");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -o <filename>  " + "write encoded msg to <filename>");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			try
			{
				// Create a data object and populate it with the data to be encoded
				
				BS3 bs3 = new BS3();
				bs3.Set(BS3.a);
				bs3.Clear(BS3.b);
				bs3.Set(BS3.c);
				
				BS2 bs2 = new BS2("'0000000001'B");
				
				System.Collections.BitArray jbs = new System.Collections.BitArray(612);
				
				/*// in JDK 1.4 you may use this portion of code 
				jbs.set (2, 612);
				jbs.clear (611);
				jbs.flip (2, 611);
				jbs.flip (68);
				*/
				
				for (int i = 2; i < 612; i++) {
               jbs.Set (i, true);
            }
				jbs.Set(610, false);
				
				BS1 bs1 = new BS1(jbs);
				
				BSSeq bsseq = new BSSeq(bs1, bs2, bs3);
				
				// Create a message buffer object and encode the record
				
				Asn1XerEncodeBuffer encodeBuffer = new Asn1XerEncodeBuffer(cxer, 0);
				
				encodeBuffer.EncodeStartDocument();
				
				bsseq.Encode(encodeBuffer, null);
				
				encodeBuffer.EncodeEndDocument();
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					encodeBuffer.Write(System.Console.OpenStandardOutput());
				}
				
				// Write the encoded record to a file
				
				encodeBuffer.Write(new System.IO.FileStream(filename, System.IO.FileMode.Create));
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