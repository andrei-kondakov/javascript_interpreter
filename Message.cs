using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JavaScriptInterpreter
{
    class Message
    {
        public readonly bool IsError;
        public readonly string Text;

        public Message(bool isError, string text)
        {
            this.IsError = isError;
            this.Text = text;
        }
    }
}
