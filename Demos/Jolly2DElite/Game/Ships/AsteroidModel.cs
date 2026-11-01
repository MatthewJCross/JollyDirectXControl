using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class AsteroidModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new(  0.0f,  80.0f,   0.0f),
            new(-80.0f, -10.0f,   0.0f),
            new(  0.0f, -80.0f,   0.0f),
            new( 70.0f, -40.0f,   0.0f),
            new( 60.0f,  50.0f,   0.0f),
            new( 50.0f,   0.0f,  60.0f),
            new(-40.0f,   0.0f,  70.0f),
            new(  0.0f,  30.0f, -75.0f),
            new(  0.0f, -50.0f, -60.0f)
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            5, 0, 6,
            6, 2, 5,
            6, 0, 1,
            1, 2, 6,
            5, 2, 3,
            3, 4, 5,
            4, 0, 5,
            0, 7, 1,
            8, 1, 7,
            3, 8, 7,
            2, 1, 8,
            2, 8, 3,
            4, 3, 7,
            0, 4, 7
        ];

        public override ushort[] StructureIndices { get; } =
        [
            0, 1,
            0, 4,
            3, 4,
            2, 3,
            1, 2,
            1, 6,
            2, 6,
            2, 5,
            5, 6,
            0, 5,
            3, 5,
            0, 6,
            4, 5,
            1, 8,
            1, 7,
            0, 7,
            4, 7,
            3, 7,
            3, 8,
            2, 8,
            7, 8
        ];

        public override ushort[] DetailIndices { get; } = [];
    }
}
