using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace JavaScriptInterpreter
{
    class Program
    {
        static void Main(string[] args)
        {
            string pathToProgram = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(System.IO.Directory.GetCurrentDirectory())), "program.txt");
            //JSInterpreter.Start(pathToProgram);
            JSInterpreter.Start(null);
        }
    }
}
