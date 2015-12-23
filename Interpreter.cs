using System;
using System.Collections.Generic;
using System.IO;
using ES;

namespace JavaScriptInterpreter
{
    public struct ExecutionContext
    {
        public ES.EnvironmentRecord Environment;
        public object ThisBinding;
        public ExecutionContext(ES.EnvironmentRecord env, object thisBinding)
        {
            this.Environment = env;
            this.ThisBinding = thisBinding;
        }
    }
    public static class JSInterpreter
    {
        private static bool debug = false;
        public static Stack<ExecutionContext> ExecutionContexts = new Stack<ExecutionContext>();
        private static ES.Object globalObject;
        public static ES.Object prototypeObject;
        public static void ShowErrorAndStop(Position pos, string text)
        {
            throw new Exception(string.Format("Error: {0}", text));
        }
        public static void Start(string sourceCode)
        {
            Token token;
            Queue<Token> lexems = new Queue<Token>();
            globalObject = new ES.Object();
            globalObject.InternalProperties["prototype"] = ES.Null.Value;
            globalObject.InternalProperties["class"] = "GlobalObject";
            globalObject.Put("_proto_", ES.Null.Value, false);
            PropertyDescriptor globalDesc = new PropertyDescriptor();
            globalDesc.Attributes["value"] = globalObject;
            globalDesc.Attributes["writable"] = false;
            globalDesc.Attributes["enumerable"] = false;
            globalDesc.Attributes["configurable"] = false;
            globalObject.DefineOwnProperty("global", globalDesc, true);

            ES.EnvironmentRecord globalEnvironment = new ObjectEnviroment(globalObject, null);
            ExecutionContext globalExceutionContext = new ExecutionContext(globalEnvironment, globalObject);
            ExecutionContexts.Push(globalExceutionContext);

            //Значение внутреннего свойства [[Prototype]] объекта-прототипа Object равно null
            //Значение внутреннего свойства [[Class]] равно "Object"
            //Tачальное значение внутреннего свойства [[Extensible]] равно true.

            prototypeObject = new ES.Object();
            prototypeObject.Put("_proto_", ES.Null.Value, false);
            prototypeObject.InternalProperties["prototype"] = ES.Null.Value;
            prototypeObject.InternalProperties["class"] = "PrototypeOfObjects";

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
                                //if (debug)
                                //{
                                //    Console.WriteLine("-------------------------------------------------------------------------------");
                                //    Console.WriteLine("--- Lexer result:");
                                //    foreach (Token tkn in lexems)
                                //    {
                                //        Console.WriteLine(tkn.ToString());
                                //    }
                                //    //  Console.WriteLine("-------------------------------------------------------------------------------");
                                //}
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
                                //if (debug)
                                //{
                                //    Console.WriteLine("-------------------------------------------------------------------------------");
                                //    Console.WriteLine("--- Global object:");
                                //    Console.WriteLine(globalObject.ToString());
                                //}
                            }
                        }
                        catch (Exception ex)
                        {
                            if (debug)
                            {
                                Console.WriteLine(ex.ToString());
                            }
                            else
                            {
                                Console.WriteLine(ex.Message);
                            }
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
        public static LanguageType GetValue(ES.Type v)
        {
            if (!(v is Reference)) return (LanguageType)v;
            Reference val = (Reference)v;
            var baseValue = val.GetBase();
            if (val.IsUnresolvableReference()) throw new Exception("ReferenceError");
            if (val.IsPropertyReference())
            {
                //if (val.HasPrimitiveBase() == false)
                //{
                //    // TODO http://es5.javascript.ru/x8.html#x8.7.1
                //    throw new NotImplementedException();
                //}
                //else
                //{
                    return ((ES.Object)baseValue).Get(val.GetReferenceName());
                //}
            }
            else
            {
                return ((EnvironmentRecord)baseValue).GetBindingValue(val.GetReferenceName(), val.IsStrictReference());
            }
        }
        public static bool PutValue(ES.Type v, LanguageType w)
        {
            if (!(v is Reference)) throw new Exception("ReferenceError");
            Reference val = (Reference)v;
            var baseValue = val.GetBase();
            if (val.IsUnresolvableReference())
            {
                if (val.IsStrictReference())
                {
                    throw new Exception("ReferenceError");
                }
                else
                {
                    return globalObject.Put(val.GetReferenceName(), w, val.IsStrictReference());
                }
            }
            else if (val.IsPropertyReference())
            {
                //if (!val.HasPrimitiveBase())
                //{
                //    throw new NotImplementedException();
                //}
                //else
                //{
                    ES.Object obj = (ES.Object)baseValue;
                    string propertyName = val.GetReferenceName();
                    bool needThrow = val.IsStrictReference();
                    return obj.Put(propertyName, w, needThrow);
                //    if (obj.CanPut(propertyName) == false)
                //    {
                //        if (needThrow)
                //        {
                //            throw new Exception("TypeError");
                //        }
                //        else
                //        {
                //            return false;
                //        }
                //    }
                //    PropertyDescriptor ownDesc = (PropertyDescriptor)obj.GetOwnProperty(propertyName);
                //    if (PropertyDescriptor.IsDataDescriptor(ownDesc))
                //    {
                //        if (needThrow)
                //        {
                //            throw new Exception("TypeError");
                //        }
                //        else
                //        {
                //            return false;
                //        }
                //    }
                //    PropertyDescriptor desc = (PropertyDescriptor)obj.GetProperty(propertyName);
                //    if (PropertyDescriptor.IsAcessorDescriptor(desc))
                //    {
                //        throw new NotImplementedException();
                //    }
                //    else
                //    {
                //        if (needThrow) throw new Exception("TypeError");
                //    }
                //    return false;
                //}
            }
            else
            {
                ((EnvironmentRecord)baseValue).SetMutableBinding(val.GetReferenceName(), w, val.IsStrictReference());
                return true;
            }
        }
        // ------------------------ Работа с лексическим окружением --------------------------------//
        public static object GetIdentifierReference(EnvironmentRecord lex, string name, bool strict)
        {
            if (lex == null)
            {
                return new Reference(Undefined.Value, name, strict);
            }
            bool exists = lex.HasBinding(name);
            if (exists)
            {
                return new Reference(lex, name, strict);
            }
            else
            {
                return GetIdentifierReference(lex.Outer, name, strict);
            }
        }
    }
}
