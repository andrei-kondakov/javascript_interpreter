using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaScriptInterpreter
{
    public struct Position : IComparable<Position>
    {
        private string text;
        private int line, pos, index;

        public Position(string text)
        {
            this.text = text;
            line = pos = 1;
            index = 0;
        }
        public int Line
        {
            get
            {
                return line;
            }
        }
        public int Pos {
            get
            {
                return pos;
            }
        }
        public int Index
        {
            get
            {
                return index;
            }
        }
        public int Cp
        {
            get
            {
                if (index == text.Length) return -1;
                else return Char.ConvertToUtf32(text, index);
            }
        }
        public int CompareTo(Position other)
        {
            return index.CompareTo(other.index);
        }
        public override string ToString()
        {
            return String.Format("({0},{1}", line, pos);
        }
        public bool IsWhiteSpace
        {
            get
            {
                return index != text.Length && Char.IsWhiteSpace(text, index);
            }
        }
        public bool IsLetter
        {
            get
            {
                return index != text.Length && Char.IsLetter(text, index);
            }
        }
        public bool IsLetterOrDigit
        {
            get
            {
                return index != text.Length && Char.IsLetterOrDigit(text, index);
            }
        }
        public bool IsDecimalDigit
        {
            get
            {
                return index != text.Length && Char.IsDigit(text, index);
            }
            
        }
        public bool IsNewLine
        {
            get
            {
                if (index == text.Length)
                    return true;
                if (text[index] == '\r' && index + 1 < text.Length)
                {
                    if (text[index + 1] == '\n')
                        return true;
                }
                return text[index] == '\n';
            }
        }
        public static Position operator ++(Position p)
        {
            if (p.index < p.text.Length)
            {
                if (p.IsNewLine)
                {
                    if (p.text[p.index] == '\r')
                    {
                        p.index++;
                    }
                    p.line++;
                    p.pos = 1;
                }
                else
                {
                    if (Char.IsHighSurrogate(p.text[p.index]))
                    {
                        p.index++;
                    }
                    p.pos++;
                }
                p.index++;
            }
            return p;
        }
    }
    public struct Fragment
    {
        public readonly Position Starting, Following;

        public Fragment(Position starting, Position following)
        {
            this.Starting = starting;
            this.Following = following;
        }
        public override string ToString()
        {
            return Starting.ToString() + "-" + Following.ToString();
        }
    }
}
