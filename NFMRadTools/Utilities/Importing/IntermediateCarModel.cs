using NFMRadTools.Editing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Utilities.Importing
{
    public class IntermediateCarModel
    {
        public List<IntermediateMesh> Meshes { get; } = new List<IntermediateMesh>();

        public NFMCar ConvertToNFMCar(VertexMergingRule vertexMergingRule)
        {
            NFMCar car = new NFMCar();
            MergeWithNFMCar(car, vertexMergingRule);
            car.SetDefaultCarPhysicProperties();
            return car;
        }

        public void MergeWithNFMCar(NFMCar other, VertexMergingRule vertexMergingRule)
        {
            if(other is null) return;
            if(!Meshes.Any()) return;
            int currentPhyIndex = -1;
            DragShotWheelDefinition dsDefinition = null;
            Dictionary<IntermediateMesh, DragShotWheelDefinition> dsMeshMap = new Dictionary<IntermediateMesh, DragShotWheelDefinition>();
            foreach (IntermediateMesh mesh in Meshes)
            {
                dsDefinition = null;
                Cylinder cylinder = new Cylinder();
                if(mesh.Mode != IntermediateMeshMode.Normal)
                {
                    cylinder = mesh.GetBoundingCylinder();
                }
                switch(mesh.Mode)
                {
                    case IntermediateMeshMode.Normal: break;
                    case IntermediateMeshMode.DragShotWheel:
                        {
                            Wheel dsWheel = GetWheel(cylinder, mesh);
                            other.Wheels.Add(dsWheel);
                            int currentWheelIndex = other.Wheels.Count - 1;
                            int indexOfSelf = -1;
                            foreach (var dsMeshEntry in Meshes.Where(x => x.Mode == IntermediateMeshMode.DragShotWheel).Index())
                            {
                                if (dsMeshEntry.Item == mesh)
                                {
                                    indexOfSelf = dsMeshEntry.Index;
                                    break;
                                }
                            }
                            if (indexOfSelf > 0)
                            {
                                foreach (IntermediateMesh prevDsMesh in Meshes.Where(x => x.Mode == IntermediateMeshMode.DragShotWheel))
                                {
                                    if (prevDsMesh == mesh) break;
                                    if (mesh.IsMeshIdenticalTo(prevDsMesh, 0.5, true))
                                    {
                                        dsDefinition = dsMeshMap[prevDsMesh];
                                        break;
                                    }
                                }
                            }
                            if (dsDefinition is not null)
                            {
                                dsDefinition.Targets.Add(currentWheelIndex);
                                continue;
                            }
                            dsDefinition = new DragShotWheelDefinition();
                            dsDefinition.Radius = cylinder.Radius.RoundToInt();
                            dsDefinition.Depth = int.Abs(dsWheel.Width);
                            dsDefinition.Targets.Add(currentWheelIndex);
                            dsMeshMap.Add(mesh, dsDefinition);
                            break;
                        }
                    case IntermediateMeshMode.PhyrexianWheel:
                        currentPhyIndex++;
                        other.Wheels.Add(GetWheel(cylinder, mesh));
                        break;
                    case IntermediateMeshMode.G6Wheel:
                        {
                            Wheel g6Wheel = GetWheel(cylinder, mesh);
                            other.Wheels.Add(g6Wheel);
                            if(mesh.G6WheelIndex.HasValue)
                            {
                                if(other.PolyGroups.Any(x => x.Mode == PolyGroupMode.G6Wheel && x.CustomWheelIndex == mesh.G6WheelIndex))
                                    continue;
                                break;
                            }
                            int indexOfSelf = -1;
                            foreach(var g6MeshEntry in Meshes.Where(x => x.Mode == IntermediateMeshMode.G6Wheel).Index())
                            {
                                if (g6MeshEntry.Item == mesh)
                                {
                                    indexOfSelf = g6MeshEntry.Index;
                                    break;
                                }
                            }
                            if(indexOfSelf > 0)
                            {
                                bool found = false;
                                foreach(IntermediateMesh prevG6Mesh in Meshes.Where(x => x.Mode == IntermediateMeshMode.G6Wheel))
                                {
                                    if (prevG6Mesh == mesh) break;
                                    if(mesh.IsMeshIdenticalTo(prevG6Mesh, 0.5, true))
                                    {
                                        g6Wheel.WheelModel = prevG6Mesh.G6WheelIndex;
                                        mesh.G6WheelIndex = prevG6Mesh.G6WheelIndex;
                                        found = true;
                                        break;
                                    }
                                }
                                if (found) continue;
                            }
                            int highestIndex = -1;
                            foreach (PolyGroup g in other.PolyGroups.Where(x => x.Mode == PolyGroupMode.G6Wheel))
                            {
                                highestIndex = int.Max(g.CustomWheelIndex, highestIndex);
                            }
                            mesh.G6WheelIndex = highestIndex + 1;
                            g6Wheel.WheelModel = mesh.G6WheelIndex;
                            break;
                        }
                    case IntermediateMeshMode.VanillaWheel:
                        other.Wheels.Add(GetWheel(cylinder, mesh));
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
                                bool invertX = cylinder.Location.X < 0;
                                v = v - cylinder.Location;
                                if (invertX)
                                    v = v * new Vector3D(-1.0, 1.0, 1.0);
                                v += new Vector3D(cylinder.Width / 2, 0, 0);
                                break;
                            case IntermediateMeshMode.G6Wheel:
                            case IntermediateMeshMode.PhyrexianWheel:
                                v = v - cylinder.Location;
                                break;
                            case IntermediateMeshMode.VanillaWheel: break;
                        }
                        vertex = (Vertex)v;
                        p.Vertices.Add(vertex);
                    }
                    bool mergeVerts = false;
                    switch(vertexMergingRule)
                    {
                        case VertexMergingRule.None: break;
                        case VertexMergingRule.All:
                            mergeVerts = true;
                            break;
                        case VertexMergingRule.Wheels:
                            mergeVerts = mesh.Mode == IntermediateMeshMode.DragShotWheel || mesh.Mode == IntermediateMeshMode.PhyrexianWheel || mesh.Mode == IntermediateMeshMode.G6Wheel;
                            break;
                    }
                    if(mergeVerts)
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
                    if(!Matches(currentPolyGroup, face.Material, currentPhyIndex, mesh, dsDefinition))
                    {
                        currentPolyGroup = other.PolyGroups.FirstOrDefault(x => Matches(currentPolyGroup, face.Material, currentPhyIndex, mesh, dsDefinition));
                        if(currentPolyGroup is null)
                        {
                            currentPolyGroup = new PolyGroup();
                            currentPolyGroup.Name = face.Material.Name;
                            currentPolyGroup.Mode = (PolyGroupMode)mesh.Mode;
                            if (currentPolyGroup.Mode == PolyGroupMode.PhyrexianWheel)
                                currentPolyGroup.CustomWheelIndex = currentPhyIndex;
                            else if (currentPolyGroup.Mode == PolyGroupMode.G6Wheel)
                                currentPolyGroup.CustomWheelIndex = (int)mesh.G6WheelIndex;
                            else if(currentPolyGroup.Mode == PolyGroupMode.DragShotWheel)
                                currentPolyGroup.DragShotWheelDefinition = dsDefinition;
                            other.PolyGroups.Add(currentPolyGroup);
                        }
                    }
                    currentPolyGroup.AddPolygon(p);
                }
                dsDefinition = null;
            }
            return;
        }

        private static bool Matches(PolyGroup group, IntermediateMaterial m, int phyIndex, IntermediateMesh mesh, DragShotWheelDefinition currentDsDef)
        {
            if(group is null) return false;
            if((int)group.Mode != (int)mesh.Mode) return false;
            if(!m.Name.Equals(group.Name, StringComparison.OrdinalIgnoreCase)) return false;
            if(group.Mode == PolyGroupMode.PhyrexianWheel) return group.CustomWheelIndex == phyIndex;
            if (group.Mode == PolyGroupMode.DragShotWheel) return group.DragShotWheelDefinition == currentDsDef;
            return true;
        }

        private static Wheel GetWheel(Cylinder c, IntermediateMesh mesh)
        {
            Wheel wheel = c.ConvertToNFMWheel(mesh.Mode);
            switch(mesh.WheelDefinition)
            {
                case IntermediateMeshWheelDefinition.Auto: break;
                case IntermediateMeshWheelDefinition.BackWheel:
                    wheel.CanSteer = false;
                    break;
                case IntermediateMeshWheelDefinition.FrontWheel:
                    wheel.CanSteer = true;
                    break;
            }
            if(mesh.Mode == IntermediateMeshMode.G6Wheel)
            {
                wheel.WheelModel = mesh.G6WheelIndex;
            }
            return wheel;
        }
    }
}
