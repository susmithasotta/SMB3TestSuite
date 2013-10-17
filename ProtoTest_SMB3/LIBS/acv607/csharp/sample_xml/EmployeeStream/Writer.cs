using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_xml.EmployeeStream
{
	
	public class Writer
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			System.String filename = new System.Text.StringBuilder("message.xml").ToString();
			bool trace = true;
			
			// Process command line arguments
			
			if (args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					if (args[i].Equals("-v"))
						Diag.Instance().SetEnabled(true);
					else if (args[i].Equals("-o"))
						filename = new System.Text.StringBuilder(args[++i]).ToString();
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
			
			// Create a data object and populate it with the data to be encoded
			
			PersonnelRecord personnelRecord = new PersonnelRecord();
			personnelRecord.name = new Name("John", "P", "Smith");
			personnelRecord.title = new Asn1IA5String("Director");
			personnelRecord.number = new EmployeeNumber(51);
			personnelRecord.dateOfHire = new Date("19710917");
			personnelRecord.nameOfSpouse = new Name("Mary", "T", "Smith");
			personnelRecord.children = new _SeqOfChildInformation(2);
			personnelRecord.children.elements[0] = new ChildInformation(new Name("Ralph", "T", "Smith"), "19571111");
			personnelRecord.children.elements[1] = new ChildInformation(new Name("Susan", "B", "Jones"), "19590717");
			
			// Create a message buffer object and encode the record
			
			Asn1XmlOutputStream outs = null;
			try
			{
				// Create a message buffer object and encode the record
				
				outs = new Asn1XmlOutputStream(new System.IO.FileStream(filename, System.IO.FileMode.Create));
				
				personnelRecord.Encode(outs);
				
				if (trace)
				{
					System.Console.Out.WriteLine("Encoding was successful");
				}
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(e.Message);
				Asn1Util.WriteStackTrace(e, Console.Error);
				return ;
			}
			finally
			{
				try
				{
					if (outs != null)
						outs.Close();
				}
				catch (System.Exception)
				{
				}
			}
		}
	}
}