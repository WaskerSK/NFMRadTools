using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Commanding
{
    public ref struct ArgumentEnumerator : IEnumerator<Argument>
    {
        public string Source { get; private set; }
        private int currentStart;
        private int currentLength;
        private int ptr;
        private char separator;
        private bool anyWhitespace;

        public Argument Current
        {
            get
            {
                if(Source is null) return Argument.Empty;
                if((uint)currentStart >= Source.Length) return Argument.Empty;
                return new Argument(Source, currentStart, currentLength);
            }
        }

        object IEnumerator.Current => Current.ToString();

        public ArgumentEnumerator(string argsString)
        {
            Source = argsString;
            currentStart = 0;
            currentLength = 0;
            ptr = 0;
            separator = ' ';
            anyWhitespace = true;
        }

        public ArgumentEnumerator(string argsString, char argSeparator, bool anyWhitespace)
        {
            Source = argsString;
            currentStart = 0;
            currentLength = 0;
            ptr = 0;
            separator = argSeparator;
            this.anyWhitespace = anyWhitespace;
        }

        public void Reset()
        {
            currentStart = 0;
            currentLength = 0;
            ptr = 0;
        }

        public void Dispose()
        {
            Source = null;
        }

        public bool MoveNext()
        {
            if(Source is null) return false;
            if(ptr >= Source.Length)
            {
                currentStart = 0;
                currentLength = 0;
                return false;
            }
            if(anyWhitespace)
            {
                while (ptr < Source.Length)
                {
                    if (!char.IsWhiteSpace(Source[ptr])) break;
                    ptr++;
                }
            }
            else
            {
                while(ptr < Source.Length)
                {
                    if (Source[ptr] != separator) break;
                    ptr++;
                }
            }

            if (ptr >= Source.Length)
            {
                currentStart = 0;
                currentLength = 0;
                return false;
            }
            if(Source[ptr] == '\"') //arg with whitespace
            {
                ptr++;
                currentStart = ptr;
                while(ptr < Source.Length)
                {
                    if (Source[ptr] == '\"') break;
                    ptr++;
                }
                if (ptr >= Source.Length)
                {
                    currentStart = 0;
                    currentLength = 0;
                    return false;
                }
                currentLength = ptr - currentStart;
                ptr++;
                return true;
            }
            else //no whitespace arg
            {
                currentStart = ptr;
                if(anyWhitespace)
                {
                    while (ptr < Source.Length)
                    {
                        if (char.IsWhiteSpace(Source[ptr])) break;
                        ptr++;
                    }
                }
                else
                {
                    while (ptr < Source.Length)
                    {
                        if (Source[ptr] == separator) break;
                        ptr++;
                    }
                }

                if (ptr >= Source.Length) ptr = Source.Length;
                currentLength = ptr - currentStart;
                return true;
            }
        }

        public int ArgCount()
        {
            int count = 0;
            ArgumentEnumerator copy = new ArgumentEnumerator(Source, separator, anyWhitespace);
            while(copy.MoveNext())
                count++;
            return count;
        }
    }
}
