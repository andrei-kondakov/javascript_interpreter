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
        private List<AstElement> children;
        public Node(object data)
            : base(data)
        {
            this.children = new List<AstElement>();
        }
        public void AddChild(AstElement child)
        {
            this.children.Add(child);
        }
        public void AddChildren(List<AstElement> children)
        {
            this.children.AddRange(children);
        }
        public void AddChildren(List<Expression> children)
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

    class ArgumentList : AstElement
    {
        private List<Expression> args;
        public ArgumentList(List<Expression> arguments)
            : base("args")
        {
            this.args = arguments;
        }
        public override string ToString(string prefix, bool isTail)
        {
            string result;
            result = base.ToString(prefix, isTail);
            for (int i = 0; i < args.Count - 1; i++)
            {
                result += args[i].ToString(prefix + (isTail ? "    " : "│   "), false);
            }
            if (args.Count > 0)
            {
                result += args[args.Count - 1].ToString(prefix + (isTail ? "    " : "│   "), true);
            }
            return result;
        }
        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
    #region Statements
    public class Statement : AstElement
    {
        public Statement(string label)
            : base(label)
        { }
    }
    public class Var : Statement
    {
        private string identifier;
        private Expression value;

        public Var(string identifier, Expression val):base("var")
        {
            this.identifier = identifier;
            this.value = val;
        }
    }
    #endregion

    #region Expressions
    public class Expression : AstElement
    {
        public Expression(object data)
            : base(data)
        { }
    }
    #region Primary Expression
    public class This : Expression
    {
        public This() : base("this") { }
    }
    public class Identifier : Expression
    {
        string name;
        public Identifier(string name)
            : base(name)
        {
            this.name = name;
        }
    }
    #region Literals
    public class Number : Expression
    {
        private double value;
        public Number(double value)
            : base(value)
        {
            this.value = value;
        }
    }
    public class String : Expression
    {
        private string value;
        public String(string value)
            : base(value)
        {
            this.value = value;
        }
    }
    public class Null : Expression
    {
        public Null() : base("null") { }
    }
    public class Boolean : Expression
    {
        private bool value;
        public Boolean(bool value)
            : base(value)
        {
            this.value = value;
        }
    }
    public class Object : Expression
    {
        List<ObjectProperty> properties;
        public Object()
            : base("object")
        { }
        public Object(List<ObjectProperty> properties)
            : base("object")
        {
            this.properties = properties;
        }
    }
    public class ObjectProperty : AstElement
    {
        string name;
        Expression value;
        public ObjectProperty(string name, Expression value)
            : base("obj_property")
        {
            this.name = name;
            this.value = value;
        }
    }
    #endregion
    #endregion
    public class Condition : Expression
    {
        private Expression condition;
        private Expression doIfTrue;
        private Expression doIfFalse;
        public Condition(Expression condition, Expression doIfTrue, Expression doIfFalse)
            : base("condition?doIfTrue:doIfFalse")
        {
            this.condition = condition;
            this.doIfTrue = doIfTrue;
            this.doIfFalse = doIfFalse;
        }
        public override string ToString(string prefix, bool isTail)
        {
            string result;
            result = base.ToString(prefix, isTail);
            result += condition.ToString(prefix + (isTail ? "    " : "│   "), false);
            result += doIfTrue.ToString(prefix + (isTail ? "    " : "│   "), false);
            result += doIfFalse.ToString(prefix + (isTail ? "    " : "│   "), true);
            return result;
        }
    }

    #region Unary Expressions
    public abstract class UnaryNode : Expression
    {
        protected Expression node;
        public UnaryNode(string operation, Expression node)
            : base(operation)
        {
            this.node = node;
        }
        public override string ToString(string prefix, bool isTail)
        {
            string result;
            result = base.ToString(prefix, isTail);
            result += node.ToString(prefix + (isTail ? "    " : "│   "), true);
            return result;
        }
    }
    public class Increment : UnaryNode
    {
        public Increment(Expression node)
            : base("++", node)
        { }
    }
    public class Decrement : UnaryNode
    {
        public Decrement(Expression node)
            : base("--", node)
        { }
    }
    public class NewExpr : UnaryNode
    {
        public NewExpr(Expression node)
            : base("new", node)
        { }
    }
    public class DeleteExpr : UnaryNode
    {
        public DeleteExpr(Expression node)
            : base("delete", node)
        { }
    }
    #endregion

    #region Binary Expressions
    public abstract class BinaryNode : Expression
    {
        protected Expression left;
        protected Expression right;
        public BinaryNode(string operation, Expression left, Expression right)
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

    #region Assignment Expressions
    public class Assignment : BinaryNode
    {
        public Assignment(string operation, Expression left, Expression right)
            : base(operation, left, right)
        { }
    }
    #endregion

    #region Logical Operations
    public class LogicalOR : BinaryNode
    {
        public LogicalOR(Expression left, Expression right)
            : base("||", left, right)
        { }
    }
    public class LogicalAND : BinaryNode
    {
        public LogicalAND(Expression left, Expression right)
            : base("&&", left, right)
        { }
    }
    public class LogicalEqual : BinaryNode
    {
        public LogicalEqual(Expression left, Expression right)
            : base("==", left, right)
        { }
    }
    public class LogicalNotEqual : BinaryNode
    {
        public LogicalNotEqual(Expression left, Expression right)
            : base("!=", left, right)
        { }
    }
    public class LogicalLess : BinaryNode
    {
        public LogicalLess(Expression left, Expression right)
            : base("<", left, right)
        { }
    }
    public class LogicalLarger : BinaryNode
    {
        public LogicalLarger(Expression left, Expression right)
            : base(">", left, right)
        { }
    }
    public class LogicalLessOrEqual : BinaryNode
    {
        public LogicalLessOrEqual(Expression left, Expression right)
            : base("<=", left, right)
        { }
    }
    public class LogicalLargerOrEqual : BinaryNode
    {
        public LogicalLargerOrEqual(Expression left, Expression right)
            : base(">=", left, right)
        { }
    }
    #endregion

    #region Bitwise Operations
    public class BitwiseOR : BinaryNode
    {
        public BitwiseOR(Expression left, Expression right)
            : base("|", left, right)
        { }
    }
    public class BitwiseXOR : BinaryNode
    {
        public BitwiseXOR(Expression left, Expression right)
            : base("^", left, right)
        { }
    }
    public class BitwiseAND : BinaryNode
    {
        public BitwiseAND(Expression left, Expression right)
            : base("&", left, right)
        { }
    }
    #endregion

    #region Shift Operations
    public class Lshift : BinaryNode
    {
        public Lshift(Expression left, Expression right)
            : base("<<", left, right)
        { }
    }
    public class Rshift : BinaryNode
    {
        public Rshift(Expression left, Expression right)
            : base(">>", left, right)
        { }
    }
    #endregion

    #region Additive Operations
    class Plus : BinaryNode
    {
        public Plus(Expression left, Expression right)
            : base("+", left, right)
        { }
    }
    class Minus : BinaryNode
    {
        public Minus(Expression left, Expression right)
            : base("-", left, right)
        { }
    }

    #endregion

    #region Multiplicative Operations
    public class Mul : BinaryNode
    {
        public Mul(Expression left, Expression right)
            : base("*", left, right)
        { }
    }
    public class Div : BinaryNode
    {
        public Div(Expression left, Expression right)
            : base("/", left, right)
        { }
    }
    // отстаток от деления
    public class Remainder : BinaryNode
    {
        public Remainder(Expression left, Expression right)
            : base("%", left, right)
        { }
    }
    #endregion

    #region Member Operations
    class GetProperty : BinaryNode
    {
        public GetProperty(Expression left, Expression right)
            : base(".", left, right)
        { }
    }
    #endregion

    #endregion

    #endregion
}
    

