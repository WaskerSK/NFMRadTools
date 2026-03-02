using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities.Importing
{
    public static class ImportRegistry
    {
        public static readonly List<Importer> Importers = InitImporters();

        private static List<Importer> InitImporters()
        {
            List<Importer> importers = new List<Importer>();
            foreach(Type t in Assembly.GetExecutingAssembly().GetTypes())
            {
                if(t.IsSubclassOf(typeof(Importer)))
                {
                    Importer imp = (Importer)Activator.CreateInstance(t);
                    importers.Add(imp);
                }
            }
            return importers;
        }

        public static Importer GetImporter(string FileName)
        {
            if(string.IsNullOrWhiteSpace(FileName)) return null;
            if (!Path.HasExtension(FileName)) return null;
            string extension = Path.GetExtension(FileName);
            Importer imp = Importers.FirstOrDefault(x => x.SupportsExtension(extension));
            return imp;
        }
    }
}
