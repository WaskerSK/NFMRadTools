using NFMRadTools.Editing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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

        public override IntermediateCarModel ImportCar(string filename, double importScale = 1.0)
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
            Dictionary<string, IntermediateMesh> meshes = new Dictionary<string, IntermediateMesh>();
            OptimizedStringReader sr = new OptimizedStringReader(objData);
            IntermediateMesh currentMesh = null;
            Dictionary<string, IntermediateMaterial> materials = new Dictionary<string, IntermediateMaterial>();
            IntermediateMaterial currentMat = null;
            int indexesEncountered = 0;
            while (!sr.EndOfString())
            {
                ReadOnlySpan<char> line = sr.ReadLine();
                line = line.TrimStart();
                if (line.IsEmpty || line.IsWhiteSpace()) continue;
                if (line.StartsWith("#")) continue;
                if (line.StartsWith("v "))
                {
                    if (currentMesh is null) throw new FormatException();
                    line = line.Slice(1).Trim();
                    int indexOfFirstSpace = line.IndexOf(' ');
                    double x = double.Parse(line.Slice(0, indexOfFirstSpace + 1), CultureInfo.InvariantCulture);
                    line = line.Slice(indexOfFirstSpace + 1).TrimStart();
                    int indexOfSecondSpace = line.IndexOf(" ");
                    double y = double.Parse(line.Slice(0, indexOfSecondSpace + 1), CultureInfo.InvariantCulture);
                    line = line.Slice(indexOfSecondSpace + 1).TrimStart();
                    double z = double.Parse(line, CultureInfo.InvariantCulture);
                    x = x * ImportScaleConversionConstant * importScale;
                    y = y * ImportScaleConversionConstant * importScale;
                    z = z * ImportScaleConversionConstant * importScale;
                    currentMesh.Vertices.Add(new Vector3D(x,y,z));
                    continue;
                }
                if(line.StartsWith("f "))
                {
                    if(currentMesh is null) throw new FormatException();
                    line = line.Slice(1).TrimStart();
                    if (line.IsEmpty || line.IsWhiteSpace()) throw new FormatException();
                    IntermediateFace face = new IntermediateFace();
                    while(true)
                    {
                        ReadOnlySpan<char> indexChars = line.Slice(0, line.GetLengthOfNumericCharactersFromIndex(0));
                        if(!int.TryParse(indexChars, out int index)) throw new FormatException();
                        index -= 1; // for some reasons in obj indexes are not 0 based??
                        index -= indexesEncountered; //for some reason obj does not reset vertex index on new object definition?? peak stupidity
                        if((uint)index >= (uint)currentMesh.Vertices.Count)
                            throw new FormatException();
                        face.Indexes.Add(index);
                        int indexOfSpace = line.IndexOf(' ');
                        if (indexOfSpace < 0) break;
                        line = line.Slice(indexOfSpace).TrimStart();
                    }
                    if (!face.Indexes.Any()) throw new FormatException();
                    if(currentMat is null)
                    {
                        currentMat = new IntermediateMaterial();
                        currentMat.Name = RandomName.Get();
                    }
                    face.Material = currentMat;
                    currentMesh.Faces.Add(face);
                    continue;
                }
                if (line.StartsWith("o "))
                {
                    line = line.Slice(1).Trim();
                    string mesh = line.ToString();
                    if (meshes.ContainsKey(mesh)) throw new FormatException("File contains identical mesh names.");
                    if (currentMesh is not null)
                        indexesEncountered += currentMesh.Vertices.Count;
                    currentMesh = new IntermediateMesh();
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
                            IntermediateMaterial mt = null;
                            OptimizedStringReader mr = new OptimizedStringReader(mtlData);
                            while(!mr.EndOfString())
                            {
                                ReadOnlySpan<char> mLine = mr.ReadLine();
                                mLine = mLine.TrimStart();
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
                                    mt = new IntermediateMaterial();
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
                                    double dR = double.Parse(r, CultureInfo.InvariantCulture);
                                    double dG = double.Parse(g, CultureInfo.InvariantCulture);
                                    double dB = double.Parse(b, CultureInfo.InvariantCulture);
                                    dR = ColorEditing.LinearsRGBTosRGB(dR);
                                    dG = ColorEditing.LinearsRGBTosRGB(dG);
                                    dB = ColorEditing.LinearsRGBTosRGB(dB);
                                    byte R = (byte)double.Round(byte.MaxValue * dR, MidpointRounding.AwayFromZero);
                                    byte G = (byte)double.Round(byte.MaxValue * dG, MidpointRounding.AwayFromZero);
                                    byte B = (byte)double.Round(byte.MaxValue * dB, MidpointRounding.AwayFromZero);
                                    mt.Color = new Color(R, G, B);
                                    continue;
                                }
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
                    if(!materials.TryGetValue(line.ToString(), out IntermediateMaterial m)) throw new FormatException();
                    currentMat = m;
                    continue;
                }
            }
            IntermediateCarModel model = new IntermediateCarModel();
            model.Meshes.EnsureCapacity(meshes.Count);
            foreach(var entry in meshes)
            {
                model.Meshes.Add(entry.Value);
            }
            return FinalizeImport(model);
        }

        public override bool SupportsExtension(string extension)
        {
            return string.Equals(extension, "obj", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".obj", StringComparison.OrdinalIgnoreCase);
        }
    }
}
