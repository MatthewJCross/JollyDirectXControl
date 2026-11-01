using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class BoulderModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new(-18.0f,  37.0f, -11.0f),
            new( 30.0f,   7.0f,  12.0f),
            new( 28.0f,  -7.0f, -12.0f),
            new(  2.0f,   0.0f, -39.0f),
            new(-28.0f,  34.0f, -30.0f),
            new(  5.0f, -10.0f,  13.0f),
            new( 20.0f,  17.0f, -30.0f)
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            4, 5, 0,
            5, 1, 0,
            5, 2, 1,
            5, 3, 2,
            3, 5, 4,
            0, 1, 6,
            2, 6, 1,
            2, 3, 6,
            4, 6, 3,
            0, 6, 4
        ];

        public override ushort[] StructureIndices { get; } =
        [
            0, 1,
            1, 2,
            2, 3,
            3, 4,
            0, 4,
            0, 5,
            1, 5,
            2, 5,
            3, 5,
            4, 5,
            0, 6,
            1, 6,
            2, 6,
            3, 6,
            4, 6
        ];

        public override ushort[] DetailIndices { get; } = [];
    }
}
