using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace AST
{
    public abstract class AstElement
    {
        protected object data;
        public AstElement(object data)
        {
            this.data = data;
        }
        public override string ToString()
        {
            return this.ToString(string.Empty, true);
        }
        public virtual string ToString(string prefix, bool isTail)
        {
            return prefix + (isTail ? "└── " : "├── ") + data.ToString() + Environment.NewLine;
        }
        public virtual void Execute()
        {
            throw new NotImplementedException();
        }
    }
    class Node : AstElement
    {
        private List<Node> children;
        public Node(object data)
            : base(data)
        {
            this.children = new List<Node>();
        }
        public void AddChild(Node child)
        {
            this.children.Add(child);
        }
        public void AddChildren(List<Node> children)
        {
            this.children.AddRange(children);
        }
        public override string ToString(string prefix, bool isTail)
        {
            string result;
            result = base.ToString(prefix, isTail);
            for (int i = 0; i < children.Count - 1; i++)
            {
                result += children[i].ToString(prefix + (isTail ? "    " : "│   "), false);
            }
            if (children.Count > 0)
            {
                result += children[children.Count - 1].ToString(prefix + (isTail ? "    " : "│   "), true);
            }
            return result;
        }
        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }

    public class Expression : AstElement
    {
        public Expression()
            : base("Expression")
        { }
    }

    public class Number : Expression
    {
        private double value;
        public Number(double value)
            : base()
        {
            this.value = value;
        }
    }
    public class String : Expression
    {
        private string value;
        public String(string value)
            : base()
        {
            this.value = value;
        }
    }

    abstract class BinaryNode : AstElement
    {
        protected AstElement left;
        protected AstElement right;
        public BinaryNode(string operation, AstElement left, AstElement right)
            : base(operation)
        {
            this.left = left;
            this.right = right;
        }
        public override string ToString(string prefix, bool isTail)
        {
            string result;
            result = base.ToString(prefix, isTail);
            result += left.ToString(prefix + (isTail ? "    " : "│   "), false);
            result += right.ToString(prefix + (isTail ? "    " : "│   "), true);
            return result;
        }
    }
    #region Additive Operations
    class Plus : BinaryNode
    {
        public Plus(AstElement left, AstElement right)
            : base("+", left, right)
        { }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
    #endregion
}
    

