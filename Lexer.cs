using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaScriptInterpreter
{
    public enum DomainTag
    {
        IDENT,
        NUMBER,
        STRING,
        RESERVED_WORD,
        SEMICOLON,  // ;
        COMMA,      // ,
        EQUAL,      // =
        LPARENT,    // (
        RPARENT,    // )
        LBRACE,     // {
        RBRACE,     // }
        LSBRACKET,  // [
        RSBRACKET,  // ]
        COLON,      // :   
        POINT,      // .
        // арифмитические операторы
        INCREMENT,
        DECREMENT,
        PLUS,
        MINUS,
        MUL,
        DIV,
        PERCENT,
        // побитовые операторы
        LSHIFT,
        RSHIFT,
        AND,
        OR,

        // логические операторы
        LOGICAL_AND,
        LOGICAL_OR,
        LOGICAL_EQUAL,
        LOGICAL_NOT_EQUAL,
        LOGICAL_NOT,
        LESS,
        LARGER,
        LESS_OR_EQUAL,
        LARGER_OR_EQUAL,
        XOR,            // ^

        QUESTION,
        END_OF_PROGRAM
    }
    public class Lexer
    {
        private static HashSet<string> reservedWords;
        public string Program;
        private Interpreter interpreter;
        private Position cur;
        public Lexer(string program, Interpreter interpreter)
        {
            this.interpreter = interpreter;
            this.Program = program;
            cur = new Position(program);
            reservedWords = new HashSet<string>
            {
                "break", "case", "catch", "continue", "debugger", "default", "delete", "do", "else",
                "false", "finally", "for", "function", "if", "in", "instanceof", "new", "null",
                "return", "switch", "this", "throw", "true", "try", "typeof", "var", "void", "while",
                "with", "undefined"
            };

        }
        public Position Cur { get; set; }
        public Token NextToken()
        {
            while (cur.Cp != -1)
            {
                while (cur.IsWhiteSpace)
                {
                    cur++;
                }
                Position start = cur;
                switch (cur.Cp)
                {
                    case '\'':
                        {
                            do
                            {
                                cur++;
                                if (cur.IsNewLine)
                                {
                                    interpreter.ShowErrorAndStop(cur, "missing the closing quotation mark");
                                }
                            } while (cur.Cp != '\'');
                            return new StringToken(Program.Substring(start.Index, cur.Index - start.Index + 1), start, ++cur);
                        }
                    case '\"':
                        {
                            do
                            {
                                cur++;
                                if (cur.IsNewLine)
                                {
                                    interpreter.ShowErrorAndStop(cur, "missing the closing quotation mark");
                                }
                            } while (cur.Cp != '\"');
                            return new StringToken(Program.Substring(start.Index, cur.Index - start.Index + 1), start, ++cur);
                        }
                    case '(':
                        return new SpecToken(DomainTag.LPARENT, start, ++cur);
                    case ')':
                        return new SpecToken(DomainTag.RPARENT, start, ++cur);
                    case ';':
                        return new SpecToken(DomainTag.SEMICOLON, start, ++cur);
                    case ',':
                        return new SpecToken(DomainTag.COMMA, start, ++cur);
                    case '{':
                        return new SpecToken(DomainTag.LBRACE, start, ++cur);
                    case '}':
                        return new SpecToken(DomainTag.RBRACE, start, ++cur);
                    case '[':
                        return new SpecToken(DomainTag.LSBRACKET, start, ++cur);
                    case ']':
                        return new SpecToken(DomainTag.RSBRACKET, start, ++cur);
                    case ':':
                        return new SpecToken(DomainTag.COLON, start, ++cur);
                    case '.':
                        // арифмитические операторы
                        return new SpecToken(DomainTag.POINT, start, ++cur);
                    case '+':
                        if ((++cur).Cp == '+')
                        {
                            return new SpecToken(DomainTag.INCREMENT, start, ++cur);
                        }
                        return new SpecToken(DomainTag.PLUS, start, cur);
                    case '-':
                        if ((++cur).Cp == '-')
                        {
                            return new SpecToken(DomainTag.DECREMENT, start, ++cur);
                        }
                        return new SpecToken(DomainTag.MINUS, start, cur);
                    case '*':
                        return new SpecToken(DomainTag.MUL, start, ++cur);
                    case '/':
                        return new SpecToken(DomainTag.DIV, start, ++cur);
                    case '%':
                        return new SpecToken(DomainTag.PERCENT, start, ++cur);
                    case '>':
                        int ch = (++cur).Cp;
                        if (ch == '>')
                        {
                            return new SpecToken(DomainTag.RSHIFT, start, ++cur);
                        }
                        else if (ch == '=')
                        {
                            return new SpecToken(DomainTag.LARGER_OR_EQUAL, start, ++cur);
                        }
                        return new SpecToken(DomainTag.LARGER, start, cur);
                    case '<':
                        ch = (++cur).Cp;
                        if (ch == '<')
                        {
                            return new SpecToken(DomainTag.LSHIFT, start, ++cur);
                        }
                        else if (ch == '=')
                        {
                            return new SpecToken(DomainTag.LESS_OR_EQUAL, start, ++cur);
                        }
                        return new SpecToken(DomainTag.LESS, start, cur);
                    case '&':
                        if ((++cur).Cp == '&')
                        {
                            return new SpecToken(DomainTag.LOGICAL_AND, start, ++cur);
                        }
                        return new SpecToken(DomainTag.AND, start, cur);
                    case '^':
                        return new SpecToken(DomainTag.XOR, start, ++cur);
                    case '?':
                        return new SpecToken(DomainTag.QUESTION, start, ++cur);
                    
                    case '|':
                        if ((++cur).Cp == '|')
                        {
                            return new SpecToken(DomainTag.LOGICAL_OR, start, ++cur);
                        }
                        return new SpecToken(DomainTag.OR, start, cur);
                    case '=':
                        if ((++cur).Cp == '=')
                        {
                            return new SpecToken(DomainTag.LOGICAL_EQUAL, start, ++cur);
                        }
                        return new SpecToken(DomainTag.EQUAL, start, cur);
                    case '!':
                        if ((++cur).Cp == '=')
                        {
                            return new SpecToken(DomainTag.LOGICAL_NOT_EQUAL, start, ++cur);
                        }
                        return new SpecToken(DomainTag.LOGICAL_NOT, start, cur);
                    default:
                        {
                            //Console.WriteLine(cur.IsLetter);
                            //Console.WriteLine((++cur).IsLetter);
                            if (cur.IsLetter || ((cur.Cp == '$' || cur.Cp == '_') && (++cur).IsLetter))
                            {
                                while (cur.IsLetterOrDigit || cur.Cp == '$' || cur.Cp == '_')
                                {
                                    cur++;
                                }
                                string name = Program.Substring(start.Index, cur.Index - start.Index);
                                if (reservedWords.Contains(name))
                                {
                                    return new ReservedWordToken(name, start, cur);
                                }
                                return new IdentToken(interpreter.AddName(name), name, start, cur);
                            }
                            else if (cur.IsDecimalDigit)
                            {
                                do
                                {
                                    cur++;
                                } while (cur.IsDecimalDigit);
                                try
                                {
                                    Int64 number = System.Convert.ToInt64(Program.Substring(start.Index, cur.Index - start.Index));
                                    return new NumberToken(number, start, cur);
                                }
                                catch (System.OverflowException)
                                {
                                    interpreter.ShowErrorAndStop(cur, "int number is too large");
                                }
                            }
                            break;
                        }
                }

            }
            return new SpecToken(DomainTag.END_OF_PROGRAM, cur, cur);
        }
    }
}