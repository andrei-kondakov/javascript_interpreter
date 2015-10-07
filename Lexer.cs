using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaScriptInterpreter
{
    public class Lexer
    {
        private static HashSet<string> reservedWords;
        public readonly string Program;
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
                "with"
            };
            
        }
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
                            return new StringToken(Program.Substring(start.Index, cur.Index - start.Index), start, ++cur);
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
                            return new StringToken(Program.Substring(start.Index, cur.Index - start.Index), start, ++cur);
                        }
                    default:
                        {
                            if (cur.IsLetter || ((++cur).IsLetter && (cur.Cp == '$' || cur.Cp == '_')))
                            {
                                do
                                {
                                    cur++;
                                } while (cur.IsLetterOrDigit || cur.Cp == '$' || cur.Cp == '_');
                                string name = Program.Substring(start.Index, cur.Index - start.Index);
                                if (reservedWords.Contains(name))
                                {
                                    return new ReservedWordToken(name, start, cur);
                                }
                                return new IdentToken(interpreter.AddName(name), start, cur);
                            }
                            else if (cur.IsDecimalDigit)
                            {
                                do
                                {
                                    cur++;
                                } while (cur.IsDecimalDigit);
                                Int64 number = Convert.ToInt64(Program.Substring(start.Index, cur.Index - start.Index));
                                return new NumberToken(number, start, cur);
                            }
                            break;
                        }
                    
                        
                }
                
            }
            return new SpecToken(DomainTag.END_OF_PROGRAM, cur, cur);
        }
    }
}
