using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace JavaScriptInterpreter
{
    public static class JSInterpreter
    {
        private static bool debug = true;
        public static void ShowErrorAndStop(Position pos, string text)
        {
            throw new Exception(String.Format("Error: {0}", text));
        }
        public static void Start(string sourceCode)
        {
            Token token;
            Queue<Token> lexems = new Queue<Token>();
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
                                    Console.WriteLine("--- Lexer result:");
                                    foreach (Token tkn in lexems)
                                    {
                                        Console.WriteLine(tkn.ToString());
                                    }
                                }
                                using (FileStream fs = new FileStream(pathToParseTreeFile, FileMode.Append, FileAccess.Write))
                                using (StreamWriter sw = new StreamWriter(fs))
                                {
                                    sw.WriteLine("################################# INPUT #######################################");
                                    sw.WriteLine(input);
                                    //sw.WriteLine("################################## END ########################################");
                                    sw.WriteLine("############################## PARSE TREE #####################################");
                                }
                                Parser parser = new Parser(lexems);
                                parser.Start();
                                lexems.Clear();
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
                Console.WriteLine(ex.Message);
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
    }
}
