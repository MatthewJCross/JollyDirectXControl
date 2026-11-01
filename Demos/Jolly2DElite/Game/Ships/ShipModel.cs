using System.Numerics;

namespace Jolly2DElite.Game
{
    internal abstract class ShipModel
    {
        public abstract Vector3[] Vertices { get; }
        public abstract ushort[] StructureIndices { get; }
        public abstract ushort[] DetailIndices { get; }
        public abstract ushort[] SurfaceIndices { get; }
    }
}
