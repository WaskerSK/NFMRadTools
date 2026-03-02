using NFMRadTools.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities.Importing
{
    public abstract class Importer
    {
        public abstract bool SupportsExtension(string extension);
        public abstract NFMCar ImportCar(string filename, double importScale);
    }
}
