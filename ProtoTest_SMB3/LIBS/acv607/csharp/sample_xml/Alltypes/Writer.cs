using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_xml.Alltypes
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
			
			TestType testData = new TestType();
			
			testData.utc = new Asn1UTCTime("030922190538-0400");
			testData.utc1 = new Asn1UTCTime("040229190538Z");
			testData.gtc = new Asn1GeneralizedTime("20030922190538.3141+0430");
			testData.gtc1 = new Asn1GeneralizedTime("20040229190538.3141Z");
			
			testData.nullval = new Asn1Null();
			
			testData.bs3 = new BS3();
			testData.bs3.Set(BS3.zero, true);
			testData.bs3.Set(BS3.a, true);
			testData.bs3.Set(BS3.b, true);
			testData.bs3.Set(BS3.c, true);
			
			testData.bs1 = new TestType_bs1();
			testData.bs1.Set(TestType_bs1.zero, true);
			testData.bs1.Set(TestType_bs1.a, true);
			testData.bs1.Set(TestType_bs1.b, true);
			testData.bs1.Set(TestType_bs1.c, true);
			
			byte[] bs = new byte[10];
			for (int i = 0; i < bs.Length; i++)
				bs[i] = (byte) (0xAA * (i + 1));
			
			testData.bs0 = new Asn1BitString(10 * 8 - 2, bs);
			testData.bs2 = new Asn1BitString();
			
			testData.val1 = new TestType_val1(21);
			testData.val2 = new TestType_val2(TestType_val2.value2);
			
			testData.enum_ = new TestType_enum_(TestType_enum_.yes);
			
			testData.real = new Asn1Real(3.14159);
			testData.nan = new Asn1Real(System.Double.NaN);
			testData.boolean = new Asn1Boolean(true);
			
			testData.name = new Name("John", "P", "Smith");
			
			testData.seqofEnum = new _SeqOfEnumType(4);
			testData.seqofEnum.elements[0] = new EnumType(EnumType.married);
			testData.seqofEnum.elements[1] = new EnumType(EnumType.single);
			testData.seqofEnum.elements[2] = new EnumType(EnumType.divorced);
			testData.seqofEnum.elements[3] = new EnumType(EnumType.children);
			
			testData.bsseqof1 = new TestType_bsseqof1(4);
			for (int i = 0; i < testData.bsseqof1.elements.Length; i++)
			{
				byte[] data = new byte[5];
				for (int j = 0; j < data.Length; j++)
				{
					data[j] = (byte) ((i + 1) * (j + 1) * 0xCC);
				}
				testData.bsseqof1.elements[i] = new Asn1BitString(5 * 8 - 1, data);
			}
			
			testData.bsseqof2 = new _SeqOfTestType_bsseqof2_element(4);
			for (int i = 0; i < testData.bsseqof2.elements.Length; i++)
			{
				byte[] data = new byte[1];
				for (int j = 0; j < data.Length; j++)
				{
					data[j] = (byte) ((i + 1) * (j + 1) * 0xCC);
				}
				testData.bsseqof2.elements[i] = new TestType_bsseqof2_element(7, data);
			}
			
			testData.osseqof1 = new TestType_osseqof1(4);
			for (int i = 0; i < testData.osseqof1.elements.Length; i++)
			{
				byte[] data = new byte[5];
				for (int j = 0; j < data.Length; j++)
				{
					data[j] = (byte) ((i + 1) * (j + 1) * 0xCC);
				}
				testData.osseqof1.elements[i] = new Asn1OctetString(data);
			}
			
			testData.csseqof1 = new TestType_csseqof1(3);
			testData.csseqof1.elements[0] = new Asn1IA5String("Objective");
			testData.csseqof1.elements[1] = new Asn1IA5String("Systems");
			testData.csseqof1.elements[2] = new Asn1IA5String("Inc.");
			
			double[] relem = new double[]{1.0, 3.14159, 2e-2, 3, 4, 5, 6, 0};
			relem[3] = -0.0d;
			relem[4] = System.Double.NegativeInfinity;
			relem[5] = System.Double.NaN;
			relem[6] = System.Double.PositiveInfinity;
			testData.rseqof = new TestType_rseqof(relem.Length);
			for (int i = 0; i < relem.Length; i++)
			{
				testData.rseqof.elements[i] = new Asn1Real(relem[i]);
			}
			
			bool[] belem = new bool[]{false, true, true, false, true};
			testData.bseqof = new TestType_bseqof(belem.Length);
			for (int i = 0; i < belem.Length; i++)
			{
				testData.bseqof.elements[i] = new Asn1Boolean(belem[i]);
			}
			
			int[] ielem = new int[]{0, 1, 2, 3, 4};
			testData.iseqof = new _SeqOfTestType_iseqof_element(5);
			for (int i = 0; i < ielem.Length; i++)
			{
				testData.iseqof.elements[i] = new TestType_iseqof_element(ielem[i]);
			}
			
			testData.iseqof1 = new TestType_iseqof1(20);
			for (int i = 0; i < testData.iseqof1.elements.Length; i++)
			{
				testData.iseqof1.elements[i] = new Asn1Integer(i);
			}
			
			testData.iseqof2 = new TestType_iseqof2(5);
			for (int i = 0; i < testData.iseqof2.elements.Length; i++)
			{
				testData.iseqof2.elements[i] = new Asn1Integer(i + 1);
			}
			
			testData.seqofChoice = new _SeqOfChoiceType(3);
			testData.seqofChoice.elements[0] =
            new ChoiceType(ChoiceType._E1, new Asn1Integer(1));
			testData.seqofChoice.elements[1] =
            new ChoiceType(ChoiceType._E2, new Asn1IA5String("String data"));
			testData.seqofChoice.elements[2] =
            new ChoiceType(ChoiceType._E3, new Name("Art", "S", "Bolgar"));
			
			testData.children = new _SeqOfChildInformation(2);
			testData.children.elements[0] = new ChildInformation(new Name("Ralph", "T", "Smith"), "19571111");
			testData.children.elements[1] = new ChildInformation(new Name("Susan", "B", "Jones"), "19590717");
			
			// Create a message buffer object and encode the record
			
			Asn1XmlEncodeBuffer encodeBuffer = new Asn1XmlEncodeBuffer();
			
			try
			{
				testData.Encode(encodeBuffer);
				
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