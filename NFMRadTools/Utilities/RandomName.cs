using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities
{
    public static class RandomName
    {
        public static string Get()
        {
            Span<char> s = stackalloc char[8];
            for (int i = 0; i < s.Length; i++)
            {
                int random = Random.Shared.Next('A', 'Z' + 1);
                s[i] = (char)random;
            }
            return s.ToString();
        }
    }
}
