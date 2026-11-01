using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class AlloyPlateModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new(-15.0f, -22.0f, -9.0f),
            new(-15.0f,  38.0f, -9.0f),
            new( 19.0f,  32.0f, 11.0f),
            new( 10.0f, -46.0f,  6.0f)
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            3, 0, 1,
            1, 2, 3,
            3, 1, 0,
            1, 3, 2
        ];

        public override ushort[] StructureIndices { get; } =
        [
        ];

        public override ushort[] DetailIndices { get; } =
        [
            0, 1,
            1, 2,
            2, 3,
            0, 3
        ];
    }
}
