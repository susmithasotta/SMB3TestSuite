using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_xml.ExtChoice
{
	
	public class Reader
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
					else if (args[i].Equals("-i"))
						filename = new System.Text.StringBuilder(args[++i]).ToString();
					else if (args[i].Equals("-notrace"))
						trace = false;
					else
					{
						System.Console.Out.WriteLine("usage: Reader [ -v ] [ -i <filename> ]" + "[ -notrace ]");
						System.Console.Out.WriteLine("   -v  verbose mode: print trace info");
						System.Console.Out.WriteLine("   -i <filename>  " + "read encoded msg from <filename>");
						System.Console.Out.WriteLine("   -notrace  do not display trace info");
						return ;
					}
				}
			}
			
			try
			{
				// Create an XML reader object
				
				XmlSaxParser reader = XmlSaxParser.NewInstance();
				
				// Read and decode the message
				
				AliasAddressList aliasAddressList = new AliasAddressList();
				aliasAddressList.Decode(reader, filename);
				if (trace)
				{
					System.Console.Out.WriteLine("Decode was successful");
					aliasAddressList.Print("aliasAddressList");

				}
				
				// To get the selected value, you would do the following:
				
				Asn1BMPString element;
				if (aliasAddressList.elements[1].ChoiceID == AliasAddress._H323_ID)
				{
					element = (Asn1BMPString) aliasAddressList.elements[1].GetElement();
				}
				else
				{
					System.Console.Out.WriteLine("Unexpected value.\n");
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