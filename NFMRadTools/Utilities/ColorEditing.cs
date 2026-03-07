using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities
{
    public static class ColorEditing
    {
        public static double LinearsRGBTosRGB(double linear)
        {
            return linear <= 0.0031308 ? (12.92 * linear) : (1.055 * double.Pow(linear, 1.0 / 2.4) - 0.055); 
        }
    }
}
