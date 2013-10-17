// This is an implementation of a simple print handler class.  It outputs 
// the fields of an encoded BER message to stdout in a structured output 
// format..
using System;
using Com.Objsys.Asn1.Runtime;
namespace Sample_ber.ToXML
{
	
	class XMLHandler : Asn1NamedEventHandler
	{
		protected internal System.String mVarName;
		protected internal int mIndentSpaces = 0;
		
		public XMLHandler(System.String varName)
		{
			mVarName = varName;
			System.Console.Out.WriteLine("<" + mVarName + ">");
			mIndentSpaces += 3;
		}
		
		~XMLHandler()
		{
			System.Console.Out.WriteLine("</" + mVarName + ">");
		}

      public virtual void  StartElement(System.String name, int index)
		{
			Indent();
			System.Console.Out.WriteLine("<" + name + ">");
			mIndentSpaces += 3;
		}
		
		public virtual void  EndElement(System.String name, int index)
		{
			mIndentSpaces -= 3;
			Indent();
			System.Console.Out.WriteLine("</" + name + ">");
		}
		
		public virtual void  Characters(System.String svalue, short typeCode)
		{
			Indent();
			System.String typeName = 
            new System.Text.StringBuilder(Asn1Type.GetTypeName(typeCode)).ToString();
			typeName.Replace(' ', '_');
			System.Console.Out.Write("<" + typeName + ">");
			System.Console.Out.Write(svalue);
			System.Console.Out.WriteLine("</" + typeName + ">");
		}
		
		public virtual void  Finished()
		{
			System.Console.Out.WriteLine("</" + mVarName + ">");
		}
		
		private void  Indent()
		{
			for (int i = 0; i < mIndentSpaces; i++)
				System.Console.Out.Write(" ");
		}
	}
}