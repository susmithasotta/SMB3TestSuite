using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_per.ExtChoice
{
	
	public class Writer
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = new System.Text.StringBuilder("message.dat").ToString();
			bool aligned = true, trace = true;
			int tvalue = 2;
			
			// Process command line arguments
			
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].Equals("-v"))
						Diag.Instance().SetEnabled(true);
					else if (args[i].Equals("-o"))
						filename = new System.Text.StringBuilder(args[++i]).ToString();
					else if (args[i].Equals("-a"))
						aligned = true;
					else if (args[i].Equals("-u"))
						aligned = false;
					else if (args[i].Equals("-t"))
						tvalue = System.Int32.Parse(args[++i]);
					else if (args[i].Equals("-notrace"))
						trace = false;
					else
					{
						System.Console.Out.WriteLine("usage: Writer [ -a | -u ] [ -v ] " + "[ -o <filename>");
						System.Console.Out.WriteLine("   -a  PER aligned encoding (default)");
						System.Console.Out.WriteLine("   -u  PER unaligned encoding");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -o <filename>  " + "write encoded msg to <filename>");
						System.Console.Out.WriteLine("   -t <option>  select option number");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			// Create a data object and populate it with the data to be encoded
			
			AliasAddress aliasAddress = new AliasAddress();
			
			switch (tvalue)
			{
				
				case 1: 
					aliasAddress.Set_e164(new Asn1IA5String("#111,222"));
					break;
				
				case 2: 
					aliasAddress.Set_h323_ID(new Asn1BMPString("TESTSTRING"));
					break;
				
				case 3: 
					aliasAddress.Set_aNull();
					break;
				
				case 4: 
					aliasAddress.Set_url_ID(new Asn1IA5String("http://www.obj-sys.com"));
					break;
				
				case 5: 
					aliasAddress.Set_transportID(new AliasAddress_transportID("Truck", 111));
					break;
				
				case 6: 
					aliasAddress.Set_email_ID(new Asn1IA5String("objective@obj-sys.com"));
					break;
				
				case 7: 
					aliasAddress.Set_partyNumber(new PartyNumber(555));
					break;
				
				default: 
					System.Console.Out.WriteLine("invalid element number " + tvalue);
					return ;
				
			}
			
			if (trace)
			{
				System.Console.Out.WriteLine("The following record will be encoded:");
				aliasAddress.Print("aliasAddress");
				System.Console.Out.WriteLine("");
			}
			
			// Create a message buffer object 
			
			Asn1PerEncodeBuffer encodeBuffer = new Asn1PerEncodeBuffer(aligned);
			
			// Enable bit field tracing
			
			if (trace)
			{
				encodeBuffer.TraceHandler.Enable();
			}
			
			// Encode
			
			try
			{
				aliasAddress.Encode(encodeBuffer);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					encodeBuffer.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					encodeBuffer.BinDump("aliasAddress");
				}
				
				// Write the encoded record to a file
				
				encodeBuffer.Write(new System.IO.FileStream(filename, System.IO.FileMode.Create));
				
				// Generate a dump file for comparisons
				
				System.String fileSuffix = (aligned)?"a":"u";
				System.IO.StreamWriter messagedmp = new System.IO.StreamWriter(new System.IO.FileStream("message"+ fileSuffix +".dmp", System.IO.FileMode.Create));
				messagedmp.AutoFlush = true;
				encodeBuffer.HexDump(messagedmp);
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