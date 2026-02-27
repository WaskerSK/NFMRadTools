using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools
{
    public class ExitException : Exception
    {
        public int ExitCode { get; init; }
        public ExitException() 
        {
            ExitCode = 0;
        }
        public ExitException(int exitCode)
        {
            ExitCode = exitCode;
        }
    }
}
