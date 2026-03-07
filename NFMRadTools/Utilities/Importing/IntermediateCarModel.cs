using NFMRadTools.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities.Importing
{
    public class IntermediateCarModel
    {
        public List<IntermediateMesh> Meshes { get; } = new List<IntermediateMesh>();

        public NFMCar ConvertToNFMCar()
        {
            NFMCar car = new NFMCar();
            MergeWithNFMCar(car);
            return car;
        }

        public void MergeWithNFMCar(NFMCar other)
        {
            if(other is null) return;
            if(!Meshes.Any()) return;
            bool hasSeenDragShotWheel = other.PolyGroups.Any(x => x.Mode == PolyGroupMode.DragShotWheel);
            int currentPhyIndex = -1;
            foreach(IntermediateMesh mesh in Meshes)
            {
                Cylinder cylinder = new Cylinder();
                if(mesh.Mode != IntermediateMeshMode.Normal)
                {
                    cylinder = mesh.GetBoundingCylinder();
                }
                switch(mesh.Mode)
                {
                    case IntermediateMeshMode.Normal: break;
                    case IntermediateMeshMode.DragShotWheel:
                        if (hasSeenDragShotWheel)
                        {
                            other.Wheels.Add(GetWheel(cylinder, mesh.WheelDefinition));
                            continue;
                        }
                        hasSeenDragShotWheel = true;
                        Wheel dsWheelVanillaDef = GetWheel(cylinder, mesh.WheelDefinition);
                        other.DragShotWheelDefinition.Radius = (int)cylinder.NFMCorrectedRadius;
                        other.DragShotWheelDefinition.Depth = int.Abs(dsWheelVanillaDef.Width);
                        other.Wheels.Add(dsWheelVanillaDef);
                        break;
                    case IntermediateMeshMode.PhyrexianWheel:
                        currentPhyIndex++;
                        other.Wheels.Add(GetWheel(cylinder, mesh.WheelDefinition));
                        break;
                    case IntermediateMeshMode.VanillaWheel:
                        other.Wheels.Add(GetWheel(cylinder, mesh.WheelDefinition));
                        continue;
                }
                PolyGroup currentPolyGroup = null;
                foreach(IntermediateFace face in mesh.Faces)
                {
                    if(face.Indexes.Count < 3) continue;
                    Polygon p = new Polygon();
                    foreach(int vIndex in face.Indexes)
                    {
                        Vertex vertex = new Vertex();
                        Vector3D v = mesh.Vertices[vIndex];
                        switch(mesh.Mode)
                        {
                            case IntermediateMeshMode.Normal: break;
                            case IntermediateMeshMode.DragShotWheel:
                                bool invertX = cylinder.Location.X >= 0;
                                v = v - cylinder.Location + new Vector3D(cylinder.NFMCorrectedWidth/4,0, 0);
                                if(invertX)
                                    v = v * new Vector3D(-1.0, 1.0, 1.0);
                                break;
                            case IntermediateMeshMode.PhyrexianWheel:
                                v = v - cylinder.Location;
                                break;
                            case IntermediateMeshMode.VanillaWheel: break;
                        }
                        vertex.X = (int)v.X;
                        vertex.Y = (int)v.Y;
                        vertex.Z = (int)v.Z;
                        p.Vertices.Add(vertex);
                    }
                    if(mesh.Mode == IntermediateMeshMode.DragShotWheel)
                    {
                        for(int i = p.Vertices.Count - 1; i >= 0; i--)
                        {
                            Vertex current = p.Vertices[i];
                            int j = i - 1;
                            Vertex prev = p.Vertices[(j < 0 ? p.Vertices.Count - 1 : j)];
                            if(current == prev)
                            {
                                p.Vertices.RemoveAt(i);
                            }
                        }
                    }
                    p.Color = face.Material.Color;
                    if(!Matches(currentPolyGroup, face.Material, currentPhyIndex, mesh))
                    {
                        currentPolyGroup = other.PolyGroups.FirstOrDefault(x => Matches(currentPolyGroup, face.Material, currentPhyIndex, mesh));
                        if(currentPolyGroup is null)
                        {
                            currentPolyGroup = new PolyGroup();
                            currentPolyGroup.Name = face.Material.Name;
                            currentPolyGroup.Mode = (PolyGroupMode)mesh.Mode;
                            if (currentPolyGroup.Mode == PolyGroupMode.PhyrexianWheel)
                                currentPolyGroup.PhyrexianWheelIndex = currentPhyIndex;
                            other.PolyGroups.Add(currentPolyGroup);
                        }
                    }
                    currentPolyGroup.AddPolygon(p);
                }
            }
            return;
        }

        private static bool Matches(PolyGroup group, IntermediateMaterial m, int phyIndex, IntermediateMesh mesh)
        {
            if(group is null) return false;
            if((int)group.Mode != (int)mesh.Mode) return false;
            if(!m.Name.Equals(group.Name, StringComparison.OrdinalIgnoreCase)) return false;
            if(group.Mode == PolyGroupMode.PhyrexianWheel) return group.PhyrexianWheelIndex == phyIndex;
            return true;
        }

        private static Wheel GetWheel(Cylinder c, IntermediateMeshWheelDefinition wheelDef)
        {
            Wheel wheel = c.ConvertToNFMWheel();
            switch(wheelDef)
            {
                case IntermediateMeshWheelDefinition.Auto: break;
                case IntermediateMeshWheelDefinition.BackWheel:
                    wheel.CanSteer = false;
                    break;
                case IntermediateMeshWheelDefinition.FrontWheel:
                    wheel.CanSteer = true;
                    break;
            }
            return wheel;
        }
    }
}
