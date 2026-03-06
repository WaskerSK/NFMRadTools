using NFMRadTools.Editing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities.Importing
{
    public abstract class Importer
    {
        public abstract bool SupportsExtension(string extension);
        public abstract IntermediateCarModel ImportCar(string filename, double importScale);
        protected IntermediateCarModel FinalizeImport(IntermediateCarModel model)
        {
            if(model is not null)
            {
                foreach(IntermediateMesh mesh in model.Meshes)
                {
                    for(int i = 0; i < mesh.Vertices.Count; i++)
                    {
                        mesh.Vertices[i] = mesh.Vertices[i] * new Vector3D(1.0, -1.0, 1.0);
                    }
                    if(string.IsNullOrWhiteSpace(mesh.Name))
                    {
                        mesh.Name = RandomName.Get();
                        mesh.Mode = IntermediateMeshMode.Normal;
                        mesh.WheelDefinition = IntermediateMeshWheelDefinition.Auto;
                        continue;
                    }
                    InterMeshDefinitions defs = GetIntermediateMeshDefinitions(mesh);
                    mesh.Mode = defs.Mode;
                    mesh.WheelDefinition = defs.WheelDefinition;
                }
            }
            return model;
        }
        private InterMeshDefinitions GetIntermediateMeshDefinitions(IntermediateMesh mesh)
        {
            InterMeshDefinitions defs = new InterMeshDefinitions();
            defs.Mode = IntermediateMeshMode.Normal;
            defs.WheelDefinition = IntermediateMeshWheelDefinition.Auto;
            if (!mesh.Name.StartsWith("wheel", StringComparison.OrdinalIgnoreCase))
                return defs;
            defs.Mode = IntermediateMeshMode.VanillaWheel;
            if (mesh.Name.Length <= "wheel".Length)
                return defs;
            ReadOnlySpan<char> name = mesh.Name.AsSpan().Slice("wheel".Length);
            if (!(name[0] == '-' || name[0] == '_'))
                return defs;
            name = name.Slice(1);
            if (name.IsEmpty || name.IsWhiteSpace())
                return defs;
            int reqLength = 0;
            if(name.StartsWith("ds", StringComparison.OrdinalIgnoreCase))
            {
                defs.Mode = IntermediateMeshMode.DragShotWheel;
                reqLength = "ds".Length;
            }
            else if(name.StartsWith("phy", StringComparison.OrdinalIgnoreCase))
            {
                defs.Mode = IntermediateMeshMode.PhyrexianWheel;
                reqLength = "phy".Length;
            }
            else
            {
                reqLength = 0;
            }
            if (name.Length <= reqLength)
                return defs;
            name = name.Slice(reqLength);
            if(name.IsEmpty || name.IsWhiteSpace())
                return defs;
            if (!(name[0] == '-' || name[0] == '_'))
                return defs;
            name = name.Slice(1);
            if (name.IsEmpty || name.IsWhiteSpace())
                return defs;
            if(name.StartsWith("b", StringComparison.OrdinalIgnoreCase) || name.StartsWith("r", StringComparison.OrdinalIgnoreCase))
            {
                defs.WheelDefinition = IntermediateMeshWheelDefinition.BackWheel;
            }
            else if(name.StartsWith("f", StringComparison.OrdinalIgnoreCase))
            {
                defs.WheelDefinition = IntermediateMeshWheelDefinition.FrontWheel;
            }
            return defs;
        }

        private struct InterMeshDefinitions
        {
            public IntermediateMeshMode Mode { get; set; }
            public IntermediateMeshWheelDefinition WheelDefinition { get; set; }
        }
    }
}
