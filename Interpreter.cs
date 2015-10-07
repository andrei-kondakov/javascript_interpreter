using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaScriptInterpreter
{
    public class Interpreter
    {
        //private SortedList<Position, Message> messages;
        private Dictionary<string, int> nameCodes;      // имена в кода
        private List<string> names;                     // коды в имена
        private Lexer lexer;
        public Interpreter(string sourceCode)
        {
          //  this.messages = new SortedList<Position, Message>();
            this.nameCodes = new Dictionary<string, int>();
            this.names = new List<string>();
            lexer = new Lexer(sourceCode,this);
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
        //public void AddMessage(bool isError, Position pos, string text)
        //{
        //    messages[pos] = new Message(isError, text);
        //}
        //public void OutputMessages()
        //{
        //    foreach (KeyValuePair<Position, Message> p in messages)
        //    {
        //        if (p.Value.IsError)
        //        {
        //            Console.Write("Error");
        //        }
        //        else
        //        {
        //            Console.Write("Warning");
        //        }
        //        Console.Write(" " + p.Key + ": ");
        //        Console.WriteLine(p.Value.Text);
        //    }
        //}
        public void Start()
        {
            while (true)
            {
            }
        }
    }
}
