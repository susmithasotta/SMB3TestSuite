using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_per.RealDec
{
	
	public class Writer
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = new System.Text.StringBuilder("message.dat").ToString();
			bool aligned = true, trace = true;
			System.String nr3a = new System.Text.StringBuilder("8991").ToString(); 
         System.String nr3b = new System.Text.StringBuilder("-8991.17").ToString();
         System.String nr3c = new System.Text.StringBuilder("8991,17e-02").ToString();
			
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
					else if (args[i].Equals("-notrace"))
						trace = false;
					else
					{
						System.Console.Out.WriteLine("usage: Writer [ -a | -u ] [ -v ] " + "[ -o <filename>");
						System.Console.Out.WriteLine("   -a  PER aligned encoding (default)");
						System.Console.Out.WriteLine("   -u  PER unaligned encoding");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -o <filename>  " + "write encoded msg to <filename>");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			// Create a data object and populate it with the data to be encoded
			
			PersonnelRecord personnelRecord = new PersonnelRecord();
			
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
				Asn1BitString bits = new Asn1BitString("'101'B");
				encodeBuffer.TraceHandler.AddElemName("bits", - 1);
				bits.Encode(encodeBuffer);
				encodeBuffer.TraceHandler.RemoveLastElemName();
				
				byte b = 3;
				encodeBuffer.EncodeLength(nr3a.Length + 1);
				encodeBuffer.Copy(b);
				encodeBuffer.EncodeBits(Asn1Util.ToByteArray(nr3a), 0, nr3a.Length * 8);
				
				encodeBuffer.EncodeLength(nr3b.Length + 1);
				encodeBuffer.Copy(b);
				encodeBuffer.EncodeBits(Asn1Util.ToByteArray(nr3b), 0, nr3b.Length * 8);
				
				encodeBuffer.EncodeLength(nr3c.Length + 1);
				encodeBuffer.Copy(b);
				encodeBuffer.EncodeBits(Asn1Util.ToByteArray(nr3c), 0, nr3c.Length * 8);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
					System.Console.Out.WriteLine("Hex dump of encoded record:");
					encodeBuffer.HexDump();
					System.Console.Out.WriteLine("Binary dump:");
					encodeBuffer.BinDump("personnelRecord");
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