using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaScriptInterpreter
{
    public abstract class Token
    {
        public readonly DomainTag Tag;
        public readonly Fragment Coords;

        protected Token(DomainTag tag, Position starting, Position following)
        {
            this.Tag = tag;
            this.Coords = new Fragment(starting, following);
        }
        public override string ToString()
        {
            //return Coords.ToString() + " " + Tag.ToString();
            return Tag.ToString();
        }
    }
    public class IdentToken : Token
    {
        public readonly string Name;

        public IdentToken(string name, Position starting, Position following) : base(DomainTag.IDENT, starting, following)
        {
            this.Name = name;
        }
        public override string ToString()
        {
            return base.ToString() + " " + Name;
        }
    }
    public class NumberToken : Token
    {
        public readonly Int64 Value;

        public NumberToken(Int64 value, Position starting, Position following)
            : base(DomainTag.NUMBER, starting, following)
        {
            this.Value = value;
        }
        public override string ToString()
        {
            return base.ToString() + " " + Value;
        }
    }
    public class StringToken : Token
    {
        public readonly string Value;

        public StringToken(string value, Position starting, Position following)
            : base(DomainTag.STRING, starting, following)
        {
            this.Value = value;
        }
        public override string ToString()
        {
            return base.ToString() + " " + Value;
        }
    }
    public class ReservedWordToken : Token
    {
        public readonly string ReservedWord;

        public ReservedWordToken(string reservedWord, Position starting, Position following)
            : base(DomainTag.RESERVED_WORD, starting, following)
        {
            this.ReservedWord = reservedWord;
        }
        public override string ToString()
        {
            return base.ToString() + " " + ReservedWord;
        }
    }
    public class SpecToken : Token
    {
        public SpecToken(DomainTag tag, Position starting, Position following)
            : base(tag, starting, following)
        {

        }
    }
}
