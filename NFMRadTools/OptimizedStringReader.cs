using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools
{
    public ref struct OptimizedStringReader
    {
        public string Source { get; init; }
        public int Current { get; private set; }
        public int Line { get; private set; }

        private int start;
        private int len;

        public ReadOnlySpan<char> LastReadLine
        {
            get
            {
                if (len <= 0) return ReadOnlySpan<char>.Empty;
                if (Source is null) return ReadOnlySpan<char>.Empty;
                return Source.AsSpan(start, len);
            }
        }

        public OptimizedStringReader()
        {
            Current = 0;
            Line = 0;
            start = 0;
            len = 0;
        }
        public OptimizedStringReader(string source)
        {
            Source = source;
            Current = 0;
            Line = 0;
            start = 0;
            len = 0;
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
            start = i;
            len = span.Length;
            Current = i + 1;
            Line++;
            return span;
        }
    }
}
