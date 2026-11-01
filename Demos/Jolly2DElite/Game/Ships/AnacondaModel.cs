using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class AnacondaModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new(   0.0f,   7.0f, -58.0f),
            new( -43.0f, -13.0f, -37.0f),
            new( -26.0f, -47.0f,  -3.0f),
            new(  26.0f, -47.0f,  -3.0f),
            new(  43.0f, -13.0f, -37.0f),
            new(   0.0f,  48.0f, -49.0f),
            new( -69.0f,  15.0f, -15.0f),
            new( -43.0f, -39.0f,  40.0f),
            new(  43.0f, -39.0f,  40.0f),
            new(  69.0f,  15.0f, -15.0f),
            new( -43.0f,  53.0f, -23.0f),
            new( -69.0f,  -1.0f,  32.0f),
            new(   0.0f,   0.0f, 254.0f),
            new(  69.0f,  -1.0f,  32.0f),
            new(  43.0f,  53.0f, -23.0f)
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            4, 1, 0,
            4, 2, 1,
            2, 4, 3,
            0, 10, 5,
            0, 6, 10,
            6, 0, 1,
            1, 11, 6,
            1, 7, 11,
            7, 1, 2,
            2, 12, 7,
            2, 8, 12,
            8, 2, 3,
            3, 13, 8,
            3, 9, 13,
            9, 3, 4,
            5, 4, 0,
            5, 9, 4,
            9, 5, 14,
            5, 10, 14,
            11, 10, 6,
            10, 11, 12,
            11, 7, 12,
            12, 8, 13,
            13, 14, 12,
            14, 13, 9,
            14, 10, 12
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
            5, 10,
            6, 10,
            6, 11,
            7, 11,
            7, 12,
            8, 12,
            8, 13,
            9, 13,
            9, 14,
            5, 14,
            10, 14,
            10, 12,
            11, 12,
            12, 13,
            12, 14
        ];

        public override ushort[] DetailIndices { get; } = [];
    }
}
