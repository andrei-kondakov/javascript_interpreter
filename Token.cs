using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaScriptInterpreter
{
    enum DomainTag
    {
        IDENT,
        NUMBER
    }
    abstract class Token
    {
        public readonly DomainTag Tag;
        public readonly Fragment Coords;

        protected Token(DomainTag tag, Position starting, Position following)
        {
            this.Tag = tag;
            this.Coords = new Fragment(starting, following);
        }
    }
    class IdentToken : Token
    {
        public readonly int Code;

        public IdentToken(int code, Position starting, Position following) : base(DomainTag.IDENT, starting, following)
        {
            this.Code = code;
        }
    }
    class NumberToken : Token
    {
        public readonly long Value;

        public NumberToken(long value, Position starting, Position following)
            : base(DomainTag.NUMBER, starting, following)
        {
            this.Value = value;
        }
    }
}
