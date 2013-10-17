using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public interface INode
    {
        int ChildCount { get; }
        TokenType Type { get; }
        string Text { get; }
        void AddChild(INode child);
        INode GetChild(int childIndex);
        string DumpTree();
        string ToString();
    }
}
