// This is an implementation of a simple print handler class.  It outputs 
// the fields of an encoded BER message to stdout in a structured output 
// format..
using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_ber.EventHandler
{
	
	class PrintHandler : Asn1NamedEventHandler
	{
		protected internal System.String mVarName;
		protected internal int mIndentSpaces = 0;
		
		public PrintHandler(System.String varName)
		{
			mVarName = varName;
			System.Console.Out.WriteLine(mVarName + " = {");
			mIndentSpaces += 3;
		}
		
		public virtual void  StartElement(System.String name, int index)
		{
			Indent();
			System.Console.Out.Write(name);
			if (index >= 0)
				System.Console.Out.Write("[" + index + "]");
			System.Console.Out.WriteLine(" = {");
			mIndentSpaces += 3;
		}
		
		public virtual void  EndElement(System.String name, int index)
		{
			mIndentSpaces -= 3;
			Indent();
			System.Console.Out.WriteLine("}");
		}
		
		public virtual void  Characters(System.String svalue, short typeCode)
		{
			Indent();
			System.Console.Out.WriteLine(svalue);
		}
		
		private void  Indent()
		{
			for (int i = 0; i < mIndentSpaces; i++)
				System.Console.Out.Write(" ");
		}
	}
}