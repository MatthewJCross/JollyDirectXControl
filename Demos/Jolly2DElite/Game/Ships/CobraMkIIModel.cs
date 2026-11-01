using Jolly2DElite.Game;
using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class CobraMkIIIModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new(  32.0f,   0.0f,  76.0f), //  0
            new( -32.0f,   0.0f,  76.0f), //  1
            new(   0.0f,  26.0f,  24.0f), //  2
            new(-120.0f,  -3.0f,  -8.0f), //  3
            new( 120.0f,  -3.0f,  -8.0f), //  4
            new( -88.0f,  16.0f, -40.0f), //  5
            new(  88.0f,  16.0f, -40.0f), //  6
            new( 128.0f,  -8.0f, -40.0f), //  7
            new(-128.0f,  -8.0f, -40.0f), //  8
            new(   0.0f,  26.0f, -40.0f), //  9
            new( -32.0f, -24.0f, -40.0f), // 10
            new(  32.0f, -24.0f, -40.0f), // 11
            new( -36.0f,   8.0f, -40.0f), // 12
            new(  -8.0f,  12.0f, -40.0f), // 13
            new(   8.0f,  12.0f, -40.0f), // 14
            new(  36.0f,   8.0f, -40.0f), // 15
            new(  36.0f, -12.0f, -40.0f), // 16
            new(   8.0f, -16.0f, -40.0f), // 17
            new(  -8.0f, -16.0f, -40.0f), // 18
            new( -36.0f, -12.0f, -40.0f), // 19
            new(   0.0f,   0.0f,  76.0f), // 20
            new(   0.0f,   0.0f,  90.0f), // 21 Gun vertex
            new( -80.0f,  -6.0f, -40.0f), // 22
            new( -80.0f,   6.0f, -40.0f), // 23
            new( -88.0f,   0.0f, -40.0f), // 24
            new(  80.0f,   6.0f, -40.0f), // 25
            new(  88.0f,   0.0f, -40.0f), // 26
            new(  80.0f,  -6.0f, -40.0f)  // 27
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            0, 2, 1,
            5, 1, 2,
            0, 6, 2,
            1, 5, 3,
            0, 4, 6,
            5, 2, 9,
            2, 6, 9,
            3, 5, 8,
            6, 4, 7,
            6, 5, 9,
            6, 8, 5,
            6, 10, 8,
            6, 11, 10,
            11, 6, 7,
            8, 1, 3,
            1, 8, 10,
            0, 10, 11,
            10, 0, 1,
            4, 11, 7,
            11, 4, 0
        ];

        public override ushort[] StructureIndices { get; } =
        [
            0, 1,
            0, 4,
            1, 3,
            3, 8,
            4, 7,
            6, 7,
            6, 9,
            5, 9,
            5, 8,
            2, 5,
            2, 6,
            3, 5,
            4, 6,
            1, 2,
            0, 2,
            8, 10,
            10, 11,
            7, 11,
            1, 10,
            0, 11,
            1, 5,
            0, 6,
            20, 21,
            2, 9
        ];

        public override ushort[] DetailIndices { get; } =
        [
            12, 13,
            18, 19,
            14, 15,
            16, 17,
            15, 16,
            14, 17,
            13, 18,
            12, 19,
            22, 24,
            23, 24,
            22, 23,
            25, 26,
            26, 27,
            25, 27
        ];

        public int GunVertex => 21;

        public int ExplosionVertexCount => 9;
    }
}
