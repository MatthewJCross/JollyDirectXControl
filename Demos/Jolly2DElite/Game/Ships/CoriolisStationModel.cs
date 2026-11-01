using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class CoriolisStationModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new( 160.0f,    0.0f,  160.0f),
            new(   0.0f,  160.0f,  160.0f),
            new(-160.0f,    0.0f,  160.0f),
            new(   0.0f, -160.0f,  160.0f),
            new( 160.0f, -160.0f,    0.0f),
            new( 160.0f,  160.0f,    0.0f),
            new(-160.0f,  160.0f,    0.0f),
            new(-160.0f, -160.0f,    0.0f),
            new( 160.0f,    0.0f, -160.0f),
            new(   0.0f,  160.0f, -160.0f),
            new(-160.0f,    0.0f, -160.0f),
            new(   0.0f, -160.0f, -160.0f),
            new(  10.0f,  -30.0f,  160.0f),
            new(  10.0f,   30.0f,  160.0f),
            new( -10.0f,   30.0f,  160.0f),
            new( -10.0f,  -30.0f,  160.0f)
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            3, 0, 1,
            1, 2, 3,
            0, 3, 4,
            5, 1, 0,
            1, 6, 2,
            7, 3, 2,
            4, 3, 7,
            7, 11, 4,
            0, 4, 8,
            8, 5, 0,
            7, 2, 6,
            6, 10, 7,
            1, 5, 9,
            9, 6, 1,
            7, 10, 11,
            11, 8, 4,
            8, 9, 5,
            9, 10, 6,
            9, 8, 11,
            11, 10, 9
        ];

        public override ushort[] StructureIndices { get; } =
        [
            0, 3,
            0, 1,
            1, 2,
            2, 3,
            3, 4,
            0, 4,
            0, 5,
            1, 5,
            1, 6,
            2, 6,
            2, 7,
            3, 7,
            8, 11,
            8, 9,
            9, 10,
            10, 11,
            4, 11,
            4, 8,
            5, 8,
            5, 9,
            6, 9,
            6, 10,
            7, 10,
            7, 11
        ];

        public override ushort[] DetailIndices { get; } =
        [
            12, 13,
            13, 14,
            14, 15,
            12, 15
        ];
    }
}
