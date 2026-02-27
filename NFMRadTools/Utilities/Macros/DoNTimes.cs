using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities.Macros
{
    public struct DoNTimes
    {
        public ulong Current { get; private set; }
        public ulong Times { get; private set; }
        public DoNTimes(int times) : this((ulong)times)
        {

        }
        public DoNTimes(uint times) : this((ulong)times)
        {

        }
        public DoNTimes(long times) : this((ulong)times)
        { 

        }
        public DoNTimes(ulong times) 
        {
            Times = times;
            Current = 0;
        }

        public void Reset()
        {
            Current = 0;
        }

        public bool Next()
        {
            if(Current < Times)
            {
                Current++;
                return true;
            }
            return false;
        }
    }
}
