using NFMRadTools.Editing;
using NFMRadTools.Utilities.CodeGen;
using NFMRadTools.Utilities.Importing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Commanding
{
    [Command(DevCommand = true)]
    public static class DevCommands
    {
        [Command(CommandName = "dev.presetgen", DevCommand = true)]
        public static void GeneratePresetsCodeFromFile(string file)
        {
            PresetCodeConverter.RunPresetCreatorScript(file);
        }

        [Command(CommandName = "dev.processobj", DevCommand = true)]
        public static void ProcessOBJ(string file)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(file);
            ObjImporter imp = ImportRegistry.GetImporter(file) as ObjImporter;
            ArgumentNullException.ThrowIfNull(imp);
            IntermediateCarModel car = imp.ImportCar(file, 1.0, null);
            ArgumentNullException.ThrowIfNull(car);

            foreach(IntermediateMesh mesh in car.Meshes.Where(x => x.Mode == IntermediateMeshMode.G6Wheel))
            {
                Cylinder c = mesh.GetBoundingCylinder();
                for(int i = 0; i < mesh.Vertices.Count; i++)
                {
                    mesh.Vertices[i] = (mesh.Vertices[i] - c.Location) * (c.Location.X < 0 ? new Vector3D(-1.0, 1.0, 1.0) : new Vector3D(1.0));
                }
            }

            StringBuilder sb = new StringBuilder();
            int encounteredVerts = 0;
            int meshCount = 0;
            foreach(IntermediateMesh mesh in car.Meshes)
            {
                sb.Append("o ").AppendLine(mesh.Name);
                foreach(Vector3D v in mesh.Vertices)
                {
                    sb.Append("v ").Append(v.X).Append(" ").Append(v.Y).Append(" ").Append(v.Z).AppendLine();
                }

                sb.AppendLine("vn 0.0 1.0 0.0");
                sb.AppendLine("vt 0.0 0.0");
                sb.AppendLine("s 0");

                foreach(IntermediateFace f in mesh.Faces)
                {
                    sb.Append("f");
                    foreach(int index in f.Indexes)
                    {
                        sb.Append(" ");
                        sb.Append(encounteredVerts + index + 1)
                            .Append("/").Append(1 + meshCount)
                            .Append("/").Append(1 + meshCount);
                    }
                    sb.AppendLine();
                }
                encounteredVerts += mesh.Vertices.Count;
                meshCount++;
            }

            File.WriteAllText(Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file) + "-processed.obj"), sb.ToString());
        }

        [Command(CommandName = "dev.g6rawcompareinfo", DevCommand = true)]
        public static void G6CompareModels(string file)
        {
            ArgumentNullException.ThrowIfNullOrWhiteSpace(file);
            ObjImporter imp = ImportRegistry.GetImporter(file) as ObjImporter;
            ArgumentNullException.ThrowIfNull(imp);
            IntermediateCarModel car = imp.ImportCar(file, 1.0, null);
            ArgumentNullException.ThrowIfNull(car);
            IEnumerable<IntermediateMesh> g6Meshes = car.Meshes.Where(x => x.Mode == IntermediateMeshMode.G6Wheel);
            foreach (IntermediateMesh mesh in g6Meshes)
            {
                Console.WriteLine($"{mesh.Name} - {mesh.Vertices.Count} vertices | Cylinder: Loc: {mesh.GetBoundingCylinder().Location} Radius: {mesh.GetBoundingCylinder().Radius} Width: {mesh.GetBoundingCylinder().Width}");
            }
            Console.WriteLine();
            PropertyInfo meshInfoProp = typeof(IntermediateMesh).GetProperty("MeshInfo", BindingFlags.Instance | BindingFlags.NonPublic);
            PropertyInfo olovProp = meshInfoProp.GetValue(g6Meshes.First()).GetType().GetProperty("OrderedLocationOffsetVertices", BindingFlags.Instance | BindingFlags.Public);
            List<Vertex> baseVerts = (List<Vertex>)olovProp.GetValue(meshInfoProp.GetValue(g6Meshes.First()));
            foreach(IntermediateMesh mesh in g6Meshes.Skip(1))
            {
                IntermediateMesh baseMesh = g6Meshes.First();
                int difference = 0;
                foreach(Vertex v in (List<Vertex>)olovProp.GetValue(meshInfoProp.GetValue(mesh)))
                {
                    if(!baseVerts.Any(x => x == v)) difference++;
                }
                Console.WriteLine($"{mesh.Name} has {difference} different vertices compared to base {baseMesh.Name}, FuncCompareResult: {mesh.IsMeshIdenticalTo(baseMesh, 0.5, true)}");
            }
        }
    }
}
