using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_per.ToXML
{
	
	public class Writer
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = new System.Text.StringBuilder("message.dat").ToString();
			bool aligned = true, trace = true;
			
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
			personnelRecord.name = new Name("John", "P", "Smith");
			personnelRecord.title = new Asn1VisibleString("Director");
			personnelRecord.number = new EmployeeNumber(51);
			personnelRecord.dateOfHire = new Date("19710917");
			personnelRecord.nameOfSpouse = new Name("Mary", "T", "Smith");
			personnelRecord.children = new PersonnelRecord_children(2);
			
			personnelRecord.children.elements[0] = new ChildInformation(new Name("Ralph", "T", "Smith"), "19571111");
			
			personnelRecord.children.elements[1] = new ChildInformation(new Name("Susan", "B", "Jones"), "19590717", ChildInformation_sex.female);
			
			// Create a message buffer object and encode the record
			
			Asn1PerEncodeBuffer encodeBuffer = new Asn1PerEncodeBuffer(aligned);
			
			try
			{
				personnelRecord.Encode(encodeBuffer);
				
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
				
				System.IO.StreamWriter messagedmp = new System.IO.StreamWriter(new System.IO.FileStream("message.dmp", System.IO.FileMode.Create));
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