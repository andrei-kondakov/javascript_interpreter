using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace JavaScriptInterpreter
{
    public class Interpreter
    {
        private Dictionary<string, int> nameCodes;      // имена в кода
        private List<string> names;                     // коды в имена
        private bool debug = true;
        public Interpreter(string sourceCode)
        {
            //  this.messages = new SortedList<Position, Message>();
            this.nameCodes = new Dictionary<string, int>();
            this.names = new List<string>();
        }

        public int AddName(string name)
        {
            if (nameCodes.ContainsKey(name))
            {
                return nameCodes[name];
            }
            else
            {
                int code = names.Count;
                names.Add(name);
                nameCodes[name] = code;
                return code;
            }
        }
        public string GetName(int code)
        {
            return names[code];
        }
        // (?) Интерпретатор должен возвращать предупреждения?
        public void ShowWarning(Position pos, string text)
        {
            Console.WriteLine("Warning {0}: {1}", pos.ToString(), text);
        }
        public void ShowErrorAndStop(Position pos, string text)
        {
            throw new Exception(String.Format("Error {0}: {1}", pos.ToString(), text));
        }
        public Lexer GetLexer(string program)
        {
            return new Lexer(program, this);
        }
        public void Start()
        {
            try
            {
                Token token;
                Queue<Token> lexems = new Queue<Token>();
                bool fromFile = true;
                if (fromFile)
                {

                    string pathToProgram = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())), "program.txt");
                    string program = File.ReadAllText(pathToProgram);
                    //Console.WriteLine("> input:");
                    Console.WriteLine(program);
                    //Console.WriteLine("> end of input");
                    Lexer lexer = this.GetLexer(program);
                    while ((token = lexer.NextToken()).Tag != DomainTag.END_OF_PROGRAM)
                    {
                        lexems.Enqueue(token);
                    }
                    if (debug)
                    {
                        Console.WriteLine("-----Lexer result:");
                        foreach (Token tkn in lexems)
                        {
                            Console.WriteLine(tkn.ToString());
                        }
                    }
                    //Node parseTree = new Node("program");
                    //parseTree.AddChild(new Node(lexems.Dequeue()));
                    //parseTree.print();

                    Parser parser = new Parser(lexems, this);
                    parser.Start();
                    

                }
                else
                {

                    while (true)
                    {
                        Console.Write("> ");
                        Lexer lexer = this.GetLexer(Console.ReadLine());
                        while ((token = lexer.NextToken()).Tag != DomainTag.END_OF_PROGRAM)
                        {
                            lexems.Enqueue(token);
                        }
                        if (debug)
                        {
                            Console.WriteLine("-----Lexer result:");
                            foreach (Token tkn in lexems)
                            {
                                Console.WriteLine(tkn.ToString());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
