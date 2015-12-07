using System;
using System.Collections.Generic;
using JavaScriptInterpreter;
using ES;

namespace AST
{
    public abstract class Element
    {
        protected object data;
        public Element(object data)
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
        public virtual object Execute()
        {
            throw new NotImplementedException();
        }
    }
    class Node : Element
    {
        private List<Element> children;
        public Node(object data)
            : base(data)
        {
            this.children = new List<Element>();
        }
        public void AddChild(Element child)
        {
            this.children.Add(child);
        }
        public void AddChildren(List<Element> children)
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
        public override object Execute()
        {
            foreach (Element child in children)
            {
                child.Execute();
            }
            return null;
        }
    }

    class ArgumentList : Element
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
        public override object Execute()
        {
            throw new NotImplementedException();
        }
    }
    #region Statements
    public class Statement : Element
    {
        public Statement(string label)
            : base(label)
        { }
    }
    public class Var : Statement
    {
        private List<VarDeclaration> varDeclarations;   
        public Var(List<VarDeclaration> varDeclarations)
            : base("variable statement")
        {
            this.varDeclarations = varDeclarations;
        }
        public override string ToString(string prefix, bool isTail)
        {
            string result;
            result = base.ToString(prefix, isTail);
            for (int i = 0; i < varDeclarations.Count - 1; i++)
            {
                result += varDeclarations[i].ToString(prefix + (isTail ? "    " : "│   "), false);
            }
            if (varDeclarations.Count > 0)
            {
                result += varDeclarations[varDeclarations.Count - 1].ToString(prefix + (isTail ? "    " : "│   "), true);
            }
            return result;
        }
        public override object Execute()
        {
            ExecutionContext activeContext = JSInterpreter.ExecutionContexts.Peek(); // берем активный контекст исполнения
            EnvironmentRecord env = activeContext.Environment;
            // TODO IN FUTURE: обрабатывать директиву Use strict 
            // в данной реализации код все время не строгий
            // также тут должна быть реализация на проверку eval код или нет
            // на данный момент выполнения кода в eval() не реализовано
            bool configurableBindings = false;
            //bool strict = false;
            foreach (VarDeclaration varDeclaration in varDeclarations)
            {

                //string identName = varDeclaration.IdentifierName;
                //bool varAlreadyDeclared = env.HasBinding(identName);
                //if (varAlreadyDeclared == false)
                //{
                //    env.CreateMutableBinding(identName, configurableBindings);
                //    env.SetMutableBinding(identName, Undefined.Value, strict);
                //}

                ES.Type lhs = (ES.Type)varDeclaration.lhs.Execute();
                if (varDeclaration.rhs != null)
                {
                    ES.Type rhs = (ES.Type)varDeclaration.rhs.Execute();
                    LanguageType value = JSInterpreter.GetValue(rhs);
                    JSInterpreter.PutValue(lhs, value);
                }
                else
                {
                    string identName = varDeclaration.lhs.Name;
                    bool varAlreadyDeclared = env.HasBinding(identName);
                    if (varAlreadyDeclared == false)
                    {
                        env.CreateMutableBinding(identName, configurableBindings);
                    }
                }
            }
            return null;
        }
    }
    public class VarDeclaration : Element
    {
        public Identifier lhs;
        public Expression rhs;
        public VarDeclaration(Identifier lhs, Expression rhs)
            : base("variable declaration")
        {
            this.lhs = lhs;
            this.rhs = rhs;
        }
        public override string ToString(string prefix, bool isTail)
        {
            //return prefix + (isTail ? "└── " : "├── ") + data.ToString() + Environment.NewLine;
            string result;
            result = base.ToString(prefix, isTail);
            if (rhs != null)
            {

                result += lhs.ToString(prefix + (isTail ? "    " : "│   "), false);
                result += rhs.ToString(prefix + (isTail ? "    " : "│   "), true);

            }
            else
            {
                result += lhs.ToString(prefix + (isTail ? "    " : "│   "), true);
            }
            return result;
        }
        public string IdentifierName
        {
            get
            {
                return lhs.Name;
            }
        }
    }
    public class ExpressionStatment : Statement
    {
        List<Expression> expressions;
        public ExpressionStatment(List<Expression> expressions) : base("expression statement")
        {
            this.expressions = expressions;
        }
        public override string ToString(string prefix, bool isTail)
        {
            string result;
            result = base.ToString(prefix, isTail);
            for (int i = 0; i < expressions.Count - 1; i++)
            {
                result += expressions[i].ToString(prefix + (isTail ? "    " : "│   "), false);
            }
            if (expressions.Count > 0)
            {
                result += expressions[expressions.Count - 1].ToString(prefix + (isTail ? "    " : "│   "), true);
            }
            return result;
        }
        public override object Execute()
        {
            for (int i = 0; i < expressions.Count - 1; i++)
            {
                expressions[i].Execute();
            }
            if (expressions.Count > 0)
            {
                object result = expressions[expressions.Count - 1].Execute();
                Console.WriteLine(result);
            }
            return null;
        }

    }
    
    #endregion

    #region Expressions
    public class Expression : Element
    {
        public Expression(object data)
            : base(data)
        { }
    }
    #region Primary Expression
    public class This : Expression
    {
        public This() : base("this") { }

        public override object Execute()
        {
            ExecutionContext activeContext = JSInterpreter.ExecutionContexts.Peek();
            return activeContext.ThisBinding;
        }
    }
    public class Identifier : Expression
    {
        string name;
        public Identifier(string name)
            : base(name)
        {
            this.name = name;
        }
        public string Name
        {
            get
            {
                return name;
            }
        }
        public override object Execute()
        {
            ExecutionContext activeContext = JSInterpreter.ExecutionContexts.Peek(); // берем активный контекст исполнения
            EnvironmentRecord env = activeContext.Environment;
            // TODO IN FUTURE: обрабатывать директиву Use strict 
            bool strict = false;
            return JSInterpreter.GetIdentifierReference(env, name, strict);
        }
    }
    #region Literals
    public class Number : Expression
    {
        private ES.Number number;
        public Number(double value)
            : base(value)
        {
            this.number = new ES.Number(value);
        }
        public override object Execute()
        {
            return number;
        }
    }
    public class String : Expression
    {
        private ES.String value;
        public String(string value)
            : base(value)
        {
            this.value = new ES.String(value);
        }
        public override object Execute()
        {
            return value;
        }

    }
    public class Null : Expression
    {
        public Null() : base("null") { }
        public override object Execute()
        {
            return null;
        }
    }
    public class Boolean : Expression
    {
        private ES.Boolean value;
        public Boolean(bool value)
            : base(value)
        {
            this.value = new ES.Boolean(value);
        }
        public override object Execute()
        {
            return value;
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
    public class ObjectProperty : Element
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
        public override object Execute()
        {
            var lref = left.Execute();
            var lval = JSInterpreter.GetValue((ES.Type)lref);
            var rref = right.Execute();
            var rval = JSInterpreter.GetValue((ES.Type)rref);
            var lprim = ES.Convert.ToPrimitive(lval, null);
            var rprim = ES.Convert.ToPrimitive(rval, null);
            if (lprim is ES.String && rprim is ES.String) 
            {
                string val1, val2;
                val1 = ((ES.String)lprim).Value;
                val2 = ((ES.String)rprim).Value;
                return new ES.String(val1 + val2);
            }
            else
            {
                var lhs = (ES.Convert.ToNumber(lprim)).Value;
                var rhs = (ES.Convert.ToNumber(rprim)).Value;
                if (double.IsNaN(lhs) || double.IsNaN(rhs))
                {
                    return new ES.Number(double.NaN);
                }
                else
                {
                    return new ES.Number(lhs + rhs);
                }
            }
        }
    }
    class Minus : BinaryNode
    {
        public Minus(Expression left, Expression right)
            : base("-", left, right)
        { }
        public override object Execute()
        {
            var lref = left.Execute();
            var lval = JSInterpreter.GetValue((ES.Type)lref);
            var rref = right.Execute();
            var rval = JSInterpreter.GetValue((ES.Type)rref);
            var lprim = ES.Convert.ToPrimitive(lval, null);
            var rprim = ES.Convert.ToPrimitive(rval, null);
            var lhs = (ES.Convert.ToNumber(lprim)).Value;
            var rhs = (ES.Convert.ToNumber(rprim)).Value;
            if (double.IsNaN(lhs) || double.IsNaN(rhs))
            {
                return new ES.Number(double.NaN);
            }
            else
            {
                return new ES.Number(lhs - rhs);
            }
        }
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
    

