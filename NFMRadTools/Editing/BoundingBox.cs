using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Editing
{
    public readonly struct BoundingBox
    {
        public int MinX { get; }
        public int MinY { get; }
        public int MinZ { get; }
        public int MaxX { get; }
        public int MaxY { get; }
        public int MaxZ { get; }

        public ulong Volume
        {
            get
            {
                ulong dX = (ulong)(MaxX - MinX);
                ulong dY = (ulong)(MaxY - MinY);
                ulong dZ = (ulong)(MaxZ - MinZ);
                return dX * dX + dY;
            }
        }

        private BoundingBox(int minX, int minY, int minZ, int maxX, int maxY, int maxZ)
        {
            MinX = minX;
            MinY = minY;
            MinZ = minZ;
            MaxX = maxX;
            MaxY = maxY;
            MaxZ = maxZ;
        }

        public ulong GetIntersectingVolume(BoundingBox other)
        {
            long overlapX = (long)int.Min(MaxX, other.MaxX) - (long)int.Max(MinX, other.MinX);
            long overlapY = (long)int.Min(MaxY, other.MaxY) - (long)int.Max(MinY, other.MinY);
            long overlapZ = (long)int.Min(MaxZ, other.MaxZ) - (long)int.Max(MinZ, other.MinZ);

            if (overlapX <= 0 || overlapY <= 0 || overlapZ <= 0) return 0;
            return (ulong)overlapX * (ulong)overlapY * (ulong)overlapZ;
        }


        public static BoundingBox Make(Polygon poly)
        {
            if(poly is null) return new BoundingBox();
            if(!poly.Vertices.Any()) return new BoundingBox();
            Vertex vert1 = poly.Vertices[0];
            int minX = vert1.X;
            int minY = vert1.Y;
            int minZ = vert1.Z;
            int maxX = vert1.X;
            int maxY = vert1.Y;
            int maxZ = vert1.Z;
            for(int i = 1;  i < poly.Vertices.Count; i++)
            {
                Vertex v = poly.Vertices[i];
                minX = int.Min(minX, v.X);
                minY = int.Min(minY, v.Y);
                minZ = int.Min(minZ, v.Z);
                maxX = int.Max(maxX, v.X);
                maxY = int.Max(maxY, v.Y);
                maxZ = int.Max(maxZ, v.Z);
            }
            return new BoundingBox(minX, minY, minZ, maxX, maxY, maxZ);
        }
    }
}
