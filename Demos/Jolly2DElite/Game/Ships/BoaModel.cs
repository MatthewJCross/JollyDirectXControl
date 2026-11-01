using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class BoaModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new(   0.0f,   0.0f,   93.0f),
            new(   0.0f,  40.0f,  -87.0f),
            new(  38.0f, -25.0f,  -99.0f),
            new( -38.0f, -25.0f,  -99.0f),
            new( -38.0f,  40.0f,  -59.0f),
            new(  38.0f,  40.0f,  -59.0f),
            new(  62.0f,   0.0f,  -67.0f),
            new(  24.0f, -65.0f,  -79.0f),
            new( -24.0f, -65.0f,  -79.0f),
            new( -62.0f,   0.0f,  -67.0f),
            new(   0.0f,   7.0f, -107.0f),
            new(  13.0f,  -9.0f, -107.0f),
            new( -13.0f,  -9.0f, -107.0f)
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            1, 11, 10,
            1, 2, 11,
            1, 6, 2,
            6, 1, 5,
            2, 12, 11,
            2, 3, 12,
            2, 8, 3,
            8, 2, 7,
            3, 10, 12,
            3, 1, 10,
            3, 4, 1,
            4, 3, 9,
            1, 4, 5,
            2, 6, 7,
            9, 3, 8,
            4, 0, 5,
            0, 9, 8,
            6, 0, 7,
            0, 4, 9,
            0, 8, 7,
            5, 0, 6,
            10, 11, 12
        ];

        public override ushort[] StructureIndices { get; } =
        [
            0, 5,
            0, 7,
            0, 9,
            0, 4,
            0, 6,
            0, 8,
            4, 5,
            5, 6,
            6, 7,
            7, 8,
            8, 9,
            4, 9,
            1, 4,
            1, 5,
            3, 9,
            3, 8,
            2, 6,
            2, 7,
            1, 10,
            2, 11,
            3, 12,
            10, 11,
            11, 12,
            10, 12
        ];

        public override ushort[] DetailIndices { get; } = [];
    }
}
