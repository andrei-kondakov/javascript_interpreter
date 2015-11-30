using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using ES;
using AST;

namespace JavaScriptInterpreter
{
    public struct ExecutionContext
    {
        public ES.LexicalEnvironment Environment;
        public object ThisBinding;
        public ExecutionContext(ES.LexicalEnvironment env, object thisBinding)
        {
            this.Environment = env;
            this.ThisBinding = thisBinding;
        }
    }
    public static class JSInterpreter
    {
        private static bool debug = true;
        public static Stack<ExecutionContext> ExecutionContexts = new Stack<ExecutionContext>();
        public static void ShowErrorAndStop(Position pos, string text)
        {
            throw new Exception(string.Format("Error: {0}", text));
        }
        public static void Start(string sourceCode)
        {
            Token token;
            Queue<Token> lexems = new Queue<Token>();
            ES.Object globalObject = new ES.Object();
            globalObject.InternalProperties["prototype"] = Undefined.Value;
            globalObject.InternalProperties["class"] = "global_object";
            globalObject.DefineOwnProperty("global", new PropertyDescriptor(globalObject, false, false, false), true);
            ES.LexicalEnvironment globalEnvironment = LexicalEnvironment.NewObjectEnvironment(globalObject, null);
            ExecutionContext globalExceutionContext = new ExecutionContext(globalEnvironment, globalObject);
            ExecutionContexts.Push(globalExceutionContext);
            //ThisBinding = globalObject;
            //http://es5.javascript.ru/x10.html#outer-environment-reference
            try
            {   
                string pathToParseTreeFile = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())), "parseTree.txt");
                File.WriteAllText(pathToParseTreeFile, string.Empty);
                if (sourceCode != null)
                {
                    string program = File.ReadAllText(sourceCode);
                    Console.WriteLine(program);
                    Lexer lexer = new Lexer(program);
                    while ((token = lexer.NextToken()).Tag != DomainTag.END_OF_PROGRAM)
                    {
                        lexems.Enqueue(token);
                    }
                    if (debug)
                    {
                        using (FileStream fs = new FileStream(pathToParseTreeFile, FileMode.Append, FileAccess.Write))
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine("################################# INPUT #######################################");
                            sw.WriteLine(program);
                            //sw.WriteLine("################################## END ########################################");
                            sw.WriteLine("############################## PARSE TREE #####################################");
                        }
                        Console.WriteLine("-----Lexer result:");
                        foreach (Token tkn in lexems)
                        {
                            Console.WriteLine(tkn.ToString());
                        }
                    }
                    Parser parser = new Parser(lexems);
                    parser.Start();
                }
                else
                {
                    int indent = 0;
                    do
                    {
                        if (indent == 0)
                        {
                            Console.Write("> ");
                        }
                        else
                        {
                            Console.Write(".");
                            for (int i = 0; i < indent; i++)
                            {
                                Console.Write("..");
                            }
                            Console.Write(" ");
                        }
                        string input = Console.ReadLine();
                        Lexer lexer = new Lexer(input);
                        lexer.indent = indent;
                        try
                        {
                            while ((token = lexer.NextToken()).Tag != DomainTag.END_OF_PROGRAM)
                            {
                                lexems.Enqueue(token);
                            }
                            indent = lexer.indent;
                            if (indent == 0)
                            {
                                if (debug)
                                {
                                    Console.WriteLine("-------------------------------------------------------------------------------");
                                    Console.WriteLine("--- Lexer result:");
                                    foreach (Token tkn in lexems)
                                    {
                                        Console.WriteLine(tkn.ToString());
                                    }
                                  //  Console.WriteLine("-------------------------------------------------------------------------------");
                                }
                                /*
                                using (FileStream fs = new FileStream(pathToParseTreeFile, FileMode.Append, FileAccess.Write))
                                using (StreamWriter sw = new StreamWriter(fs))
                                {
                                    sw.WriteLine("################################# INPUT #######################################");
                                    sw.WriteLine(input);
                                    //sw.WriteLine("################################## END ########################################");
                                    sw.WriteLine("############################## PARSE TREE #####################################");
                                }
                                */
                                Parser parser = new Parser(lexems);
                                AST.Node ast = parser.Start();
                                if (debug)
                                {
                                    Console.WriteLine("-------------------------------------------------------------------------------");
                                    Console.WriteLine("--- Parser result:");
                                    Console.WriteLine(ast.ToString());
                                 //   Console.WriteLine("-------------------------------------------------------------------------------");
                                }
                                lexems.Clear();
                                ast.Execute();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                            lexems.Clear();
                        }
                    }
                    while (true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                //if (sourceCode != null)
               // {
                    Console.WriteLine("Press any key to continue");
                    Console.ReadKey();
                //}
            }
        }
        public static object GetValue(object v)
        {
            if (!(v is Reference)) return v;
            Reference val = (Reference)v;
            var baseValue = val.GetBase();
            if (val.IsUnresolvableReference()) throw new Exception("ReferenceError");
            if (val.IsPropertyReference())
            {
                throw new NotImplementedException();
                if (val.HasPrimitiveBase() == false)
                {
                    // TODO http://es5.javascript.ru/x8.html#x8.7.1

                }
                else
                {

                }
            }
            else
            {
                ((EnvironmentRecord)baseValue).GetBindingValue(val.GetReferenceName(), val.IsStrictReference());
            }
            return null;
        }
    }
}
