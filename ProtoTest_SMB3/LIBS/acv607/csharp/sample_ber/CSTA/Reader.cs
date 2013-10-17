using System;
using Com.Objsys.Asn1.Runtime;
using Sample_ber.SimpleROSE;
namespace Sample_ber.CSTA
{
	
	public class Reader
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = new System.Text.StringBuilder("message.dat").ToString();
			bool trace = true;
			
			// Process command line arguments
			
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].Equals("-v"))
						Diag.Instance().SetEnabled(true);
					else if (args[i].Equals("-i"))
						filename = new System.Text.StringBuilder(args[++i]).ToString();
					else if (args[i].Equals("-notrace"))
						trace = false;
					else
					{
						System.Console.Out.WriteLine("usage: Reader [ -v ] [ -i <filename> ]");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -i <filename>  " + "read encoded msg from <filename>");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			try
			{
				// Create an input file stream object
				
				System.IO.FileStream ins = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				
				// Create a decode buffer object
				
				Asn1BerDecodeBuffer decodeBuffer = new Asn1BerDecodeBuffer(ins);
				
				// Read and decode the ROSE header
				
				RosePDU rosePDU = new RosePDU();
				rosePDU.Decode(decodeBuffer);
				if (trace)
				{
					System.Console.Out.WriteLine("Decode RosePDU was successful");
					rosePDU.Print("rosePDU");
					System.Console.Out.WriteLine("");
				}
				
				// Decode the argument
				
				InvokePDU invokePDU = (InvokePDU) rosePDU.GetElement();
				
				Asn1BerDecodeBuffer decodeBuffer2 = new Asn1BerDecodeBuffer(invokePDU.argument.mValue);
				
				MakeCallArgument makeCallArgument = new MakeCallArgument();
				
				makeCallArgument.Decode(decodeBuffer2);
				if (trace)
				{
					System.Console.Out.WriteLine("Decode MakeCallArgument was successful");
					makeCallArgument.Print("makeCallArgument");
				}
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