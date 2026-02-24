using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Commanding
{
    public readonly struct Argument
    {
        public static Argument Empty => new Argument(null, 0, 0);
        public string SourceString { get; }
        public int StartIndex { get; }
        public int Length { get; }

        public Argument(string source, int startIndex, int length)
        {
            SourceString = source;
            StartIndex = startIndex;
            Length = length;
        }

        public override string ToString()
        {
            if(string.IsNullOrWhiteSpace(SourceString)) return string.Empty;
            if(Length == 0) return string.Empty;
            if((uint)StartIndex + (uint)Length > (uint)SourceString.Length) return string.Empty;
            if (Length == SourceString.Length) return SourceString;
            return SourceString.Substring(StartIndex, Length);
        }

        public ReadOnlySpan<char> ToSpan()
        {
            if(string.IsNullOrWhiteSpace(SourceString)) return ReadOnlySpan<char>.Empty;
            if(Length == 0) return ReadOnlySpan<char>.Empty;
            if ((uint)StartIndex + (uint)Length > (uint)SourceString.Length) return ReadOnlySpan<char>.Empty;
            if (Length == SourceString.Length) return SourceString.AsSpan();
            return SourceString.AsSpan(StartIndex, Length);
        }
    }
}
