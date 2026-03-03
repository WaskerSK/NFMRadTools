using NFMRadTools.Editing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities.Importing
{
    public sealed class ObjImporter : Importer
    {
        private const double ImportScaleConversionConstant = 10.0;
        public ObjImporter() { }

        public override NFMCar ImportCar(string filename, double importScale = 1.0)
        {
            Debug.Assert(filename is not null);
            Debug.Assert(filename.EndsWith(".obj", StringComparison.OrdinalIgnoreCase));
            string objData = null;
            try
            {
                objData = File.ReadAllText(filename);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return null;
            }
            if (string.IsNullOrWhiteSpace(objData))
            {
                Logger.Error("Tried to load empty file.");
                return null;
            }
            Dictionary<string, ObjMesh> meshes = new Dictionary<string, ObjMesh>();
            OptimizedStringReader sr = new OptimizedStringReader(objData);
            ObjMesh currentMesh = null;
            Dictionary<string, ObjMaterial> materials = new Dictionary<string, ObjMaterial>();
            ObjMaterial currentMat = null;
            while (!sr.EndOfString())
            {
                ReadOnlySpan<char> line = sr.ReadLine();
                line = line.TrimStart();
                if (line.IsEmpty || line.IsWhiteSpace()) continue;
                if (line.StartsWith("#")) continue;
                if (line.StartsWith("v"))
                {
                    if (currentMesh is null) throw new FormatException();
                    line = line.Slice(1).Trim();
                    int indexOfFirstSpace = line.IndexOf(' ');
                    double x = double.Parse(line.Slice(0, indexOfFirstSpace + 1));
                    line = line.Slice(indexOfFirstSpace + 1).TrimStart();
                    int indexOfSecondSpace = line.IndexOf(" ");
                    double y = double.Parse(line.Slice(0, indexOfSecondSpace + 1));
                    line = line.Slice(indexOfSecondSpace + 1).TrimStart();
                    double z = double.Parse(line);
                    x = x * ImportScaleConversionConstant * importScale;
                    y = y * ImportScaleConversionConstant * importScale;
                    z = z * ImportScaleConversionConstant * importScale;
                    currentMesh.Vertices.Add(new Vector3D(x,y,z));
                    continue;
                }
                if(line.StartsWith("f"))
                {
                    if(currentMesh is null) throw new FormatException();
                    line = line.Slice(1).TrimStart();
                    if (line.IsEmpty || line.IsWhiteSpace()) throw new FormatException();
                    ObjFace face = new ObjFace();
                    while(true)
                    {
                        ReadOnlySpan<char> indexChars = line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0));
                        if(!int.TryParse(indexChars, out int index)) throw new FormatException();
                        if((uint)index >= (uint)currentMesh.Vertices.Count) throw new FormatException();
                        face.Indexes.Add(index);
                        int indexOfSpace = line.IndexOf(' ');
                        if (indexOfSpace < 0) break;
                        line = line.Slice(indexOfSpace).TrimStart();
                    }
                    if (!face.Indexes.Any()) throw new FormatException();
                    currentMesh.Faces.Add(face);
                    continue;
                }
                if (line.StartsWith("o"))
                {
                    line = line.Slice(1).Trim();
                    string mesh = line.ToString();
                    if (meshes.ContainsKey(mesh)) throw new FormatException("File contains identical mesh names.");
                    currentMesh = new ObjMesh();
                    currentMesh.Name = mesh;
                    meshes.Add(mesh, currentMesh);
                    currentMat = null;
                    continue;
                }
                if (line.StartsWith("mtllib"))
                {
                    currentMat = null;
                    line = line.Slice("mtllib".Length).Trim();
                    string mtl = Path.Combine(Path.GetDirectoryName(filename), line.ToString());
                    if(!File.Exists(mtl))
                    {
                        Logger.Warning($"{Path.GetFileName(filename)} has a missing material dependency: {Path.GetFileName(mtl)}");
                    }
                    else
                    {
                        try
                        {
                            string mtlData = null;
                            try
                            {
                                mtlData = System.IO.File.ReadAllText(mtl);
                            }
                            catch (Exception ex)
                            {
                                Logger.Warning(ex.ToString());
                                continue;
                            }
                            if (string.IsNullOrWhiteSpace(mtlData))
                            {
                                Logger.Warning("Tried to read empty material file.");
                                continue;
                            }
                            OptimizedStringReader mr = new OptimizedStringReader(mtlData);
                            ReadOnlySpan<char> mLine = sr.ReadLine();
                            mLine = mLine.TrimStart();
                            ObjMaterial mt = null;
                            if (mLine.IsEmpty || mLine.IsWhiteSpace()) continue;
                            if (mLine.StartsWith("newmtl"))
                            {
                                mLine = mLine.Slice("newmtl".Length).Trim();
                                string n = null;
                                if (mLine.IsEmpty || mLine.IsWhiteSpace())
                                {
                                    n = RandomName.Get();
                                }
                                else n = mLine.ToString();
                                mt = new ObjMaterial();
                                mt.Name = n;
                                materials.Add(n, mt);
                                continue;
                            }
                            if (mLine.StartsWith("Kd"))
                            {
                                if (mt is null) continue;
                                mLine = mLine.Slice(2).TrimStart();
                                int indexOfSpace = mLine.IndexOf(' ');
                                ReadOnlySpan<char> r = mLine.Slice(0, indexOfSpace);
                                mLine = mLine.Slice(indexOfSpace).TrimStart();
                                indexOfSpace = mLine.IndexOf(' ');
                                ReadOnlySpan<char> g = mLine.Slice(0, indexOfSpace);
                                mLine = mLine.Slice(indexOfSpace).Trim();
                                ReadOnlySpan<char> b = mLine;
                                double dR = double.Parse(r);
                                double dG = double.Parse(g);
                                double dB = double.Parse(b);
                                byte R = (byte)(byte.MaxValue * dR);
                                byte G = (byte)(byte.MaxValue * dG);
                                byte B = (byte)(byte.MaxValue * dB);
                                mt.Color = new Color(R,G,B);
                                continue;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning(ex.ToString());
                            currentMat = null;
                        }
                    }
                    continue;
                }
                if(line.StartsWith("usemtl"))
                {
                    if (currentMesh is null) throw new FormatException();
                    line = line.Slice("usemtl".Length).Trim();
                    if(!materials.TryGetValue(line.ToString(), out ObjMaterial m)) throw new FormatException();
                    currentMat = m;
                    continue;
                }
            }
            /*foreach(var entry in meshes)
            {
                if(entry.Value.Name.StartsWith("dsw-"))
            }*/
            throw new NotImplementedException();
        }

        public override bool SupportsExtension(string extension)
        {
            return string.Equals(extension, "obj", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".obj", StringComparison.OrdinalIgnoreCase);
        }

        private class ObjMesh
        {
            internal string Name;
            internal List<Vector3D> Vertices = new List<Vector3D>();
            internal List<ObjFace> Faces = new List<ObjFace>();
        }

        private class ObjFace
        {
            internal List<int> Indexes;
            internal ObjMaterial material;

            internal ObjFace()
            {
                Indexes = new List<int>();
            }
        }

        private class ObjMaterial
        {
            internal string Name;
            internal Color Color;
        }
    }
}
