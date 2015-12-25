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
