using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace JavaScriptInterpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            Interpreter js_interpreter = new Interpreter("");
            js_interpreter.Start();
            Console.WriteLine("Press any key to continue"); 
            Console.ReadKey();
        }
    }
}
