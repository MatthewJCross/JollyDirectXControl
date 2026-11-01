using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class CargoCanisterModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new( 24.0f,  16.0f,   0.0f),
            new( 24.0f,   5.0f,  15.0f),
            new( 24.0f, -13.0f,   9.0f),
            new( 24.0f, -13.0f,  -9.0f),
            new( 24.0f,   5.0f, -15.0f),
            new(-24.0f,  16.0f,   0.0f),
            new(-24.0f,   5.0f,  15.0f),
            new(-24.0f, -13.0f,   9.0f),
            new(-24.0f, -13.0f,  -9.0f),
            new(-24.0f,   5.0f, -15.0f)
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            4, 0, 1,
            4, 1, 2,
            2, 3, 4,
            0, 5, 6,
            6, 1, 0,
            2, 1, 6,
            6, 7, 2,
            2, 7, 8,
            8, 3, 2,
            4, 3, 8,
            8, 9, 4,
            0, 4, 9,
            9, 5, 0,
            6, 5, 9,
            6, 9, 8,
            8, 7, 6
        ];

        public override ushort[] StructureIndices { get; } =
        [
            0, 1,
            1, 2,
            2, 3,
            3, 4,
            0, 4,
            0, 5,
            1, 6,
            2, 7,
            3, 8,
            4, 9,
            5, 6,
            6, 7,
            7, 8,
            8, 9,
            5, 9
        ];

        public override ushort[] DetailIndices { get; } = [];
    }
}