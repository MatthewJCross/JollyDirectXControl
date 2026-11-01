using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class AspMkIIModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new(  0.0f, -18.0f,   0.0f),
            new(  0.0f,  -9.0f, -45.0f),
            new( 43.0f,   0.0f, -45.0f),
            new( 69.0f,  -3.0f,   0.0f),
            new( 43.0f, -14.0f,  28.0f),
            new(-43.0f,   0.0f, -45.0f),
            new(-69.0f,  -3.0f,   0.0f),
            new(-43.0f, -14.0f,  28.0f),
            new( 26.0f,  -7.0f,  73.0f),
            new(-26.0f,  -7.0f,  73.0f),
            new( 43.0f,  14.0f,  28.0f),
            new(-43.0f,  14.0f,  28.0f),
            new(  0.0f,   9.0f, -45.0f),
            new(-17.0f,   0.0f, -45.0f),
            new( 17.0f,   0.0f, -45.0f),
            new(  0.0f,  -4.0f, -45.0f),
            new(  0.0f,   4.0f, -45.0f),
            new(  0.0f,  -7.0f,  73.0f),
            new(  0.0f,  -7.0f,  83.0f)
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            0, 9, 7,
            0, 8, 9,
            8, 0, 4,
            0, 3, 4,
            0, 2, 3,
            2, 0, 1,
            7, 1, 0,
            7, 5, 1,
            5, 7, 6,
            11, 10, 12,
            10, 9, 8,
            9, 10, 11,
            11, 5, 6,
            5, 11, 12,
            12, 3, 2,
            3, 12, 10,
            4, 3, 8,
            6, 7, 9,
            3, 10, 8,
            6, 9, 11,
            5, 2, 1,
            2, 5, 12
        ];

        public override ushort[] StructureIndices { get; } =
        [
            0, 1,
            0, 4,
            0, 7,
            1, 2,
            2, 3,
            3, 8,
            8, 9,
            6, 9,
            5, 6,
            1, 5,
            3, 4,
            4, 8,
            6, 7,
            7, 9,
            2, 12,
            5, 12,
            10, 12,
            11, 12,
            10, 11,
            6, 11,
            9, 11,
            3, 10,
            8, 10,
            17, 18
        ];

        public override ushort[] DetailIndices { get; } =
        [
            13, 15,
            14, 15,
            14, 16,
            13, 16
        ];
    }
}

