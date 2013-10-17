using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Protocols.TestTools.StackSdk.Messages.Marshaling
{
    public abstract class BaseNode : INode
    {
        private IList<INode> children = new List<INode>();

        protected BaseNode()
        {
        }

        public virtual int ChildCount
        {
            get
            {
                if (children == null)
                {
                    return 0;
                }
                return children.Count;
            }
        }

        public virtual INode GetChild(int childIndex)
        {
            if (children == null || childIndex >= children.Count)
            {
                return null;
            }

            return children[childIndex];
        }

        public abstract TokenType Type
        {
            get;
        }

        public abstract string Text
        {
            get;
        }

        public virtual void AddChild(INode child)
        {
            if (child == null)
            {
                return;
            }

            children.Add(child);
        }

        public override abstract string ToString();

        public virtual string DumpTree()
        {
            if (children == null || children.Count == 0)
            {
                return this.ToString();
            }

            StringBuilder buf = new StringBuilder();
            buf.Append("(");
            buf.Append(this.ToString());
            buf.Append(' ');

            for (int i = 0; children != null && i < children.Count; i++)
            {
                BaseNode node = (BaseNode)children[i];
                if (i > 0)
                {
                    buf.Append(' ');
                }
                buf.Append(node.DumpTree());
            }

            buf.Append(")");

            return buf.ToString();
        }
    }
}
