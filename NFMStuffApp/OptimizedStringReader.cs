using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMStuffApp
{
    public ref struct OptimizedStringReader
    {
        public string Source { get; init; }
        public int Current { get; private set; }
        public OptimizedStringReader()
        {
            Current = 0;
        }
        public OptimizedStringReader(string source)
        {
            Source = source;
            Current = 0;
        }

        public bool EndOfString()
        {
            if (Source is null) return true;
            if(Current >= Source.Length) return true;
            return false;
        }

        public ReadOnlySpan<char> ReadLine()
        {
            if (EndOfString()) return ReadOnlySpan<char>.Empty;
            int i = Current;
            for(; i < Source.Length; i++)
            {
                if(Source[i] == '\n')
                {
                    break;
                }
            }
            ReadOnlySpan<char> span = Source.AsSpan(Current, i - Current);
            Current = i + 1;
            return span;
        }
    }
}
