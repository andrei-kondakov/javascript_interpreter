using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.IO;

namespace JavaScriptInterpreter
{
    class Node
    {
        private object data;
        private List<Node> children;
        public Node(object data)
        {
            this.data = data;
            this.children = new List<Node>();
        }
        public object Data
        {
            get { return data; }
            set { data = value; }
        }
        public void AddChild(Node child)
        {
            this.children.Add(child);
        }
        public void AddChildren(List<Node> children)
        {
            this.children.AddRange(children);
        }
        public void print(string filePath)
        {
            //if (filePath != null) File.WriteAllText(filePath, string.Empty);
            print("", true, filePath);
        }

        private void print(string prefix, bool isTail, string filePath)
        {
            if (filePath != null)
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Append, FileAccess.Write))
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    //sw.WriteLine(prefix + (isTail ? "└── " : "├── ") + "[" + data.ToString() + "]");
                    sw.WriteLine(prefix + (isTail ? "└── " : "├── ") + data.ToString());
                }
            }
            else
            {
                Console.WriteLine(prefix + (isTail ? "└── " : "├── ") + data.ToString() );
            }

            //     Console.WriteLine(prefix + (isTail ? "└── " : "├── ") + "[" + data.ToString() + "]");


            for (int i = 0; i < children.Count - 1; i++)
            {
                children[i].print(prefix + (isTail ? "    " : "│   "), false, filePath);
            }
            if (children.Count > 0)
            {
                children[children.Count - 1].print(prefix + (isTail ? "    " : "│   "), true, filePath);
            }
        }
    }
}

