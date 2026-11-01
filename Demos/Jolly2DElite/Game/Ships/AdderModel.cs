using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class AdderModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new(-18.0f,  0.0f,  40.0f),
            new( 18.0f,  0.0f,  40.0f),
            new( 30.0f,  0.0f, -24.0f),
            new( 30.0f,  0.0f, -40.0f),
            new( 18.0f, -7.0f, -40.0f),
            new(-18.0f, -7.0f, -40.0f),
            new(-30.0f,  0.0f, -40.0f),
            new(-30.0f,  0.0f, -24.0f),
            new(-18.0f,  7.0f, -40.0f),
            new( 18.0f,  7.0f, -40.0f),
            new(-18.0f,  7.0f,  13.0f),
            new( 18.0f,  7.0f,  13.0f),
            new(-18.0f, -7.0f,  13.0f),
            new( 18.0f, -7.0f,  13.0f),
            new(-11.0f,  2.86118245f, 28.96401024f),
            new( 11.0f,  2.86118245f, 28.96401024f),
            new( 11.0f,  4.13881731f, 24.03598976f),
            new(-11.0f,  4.13881731f, 24.03598976f)
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            10, 0, 1,
            1, 11, 10,
            0, 12, 13,
            13, 1, 0,
            2, 11, 1,
            13, 2, 1,
            11, 2, 3,
            3, 9, 11,
            2, 13, 4,
            4, 3, 2,
            8, 6, 7,
            7, 10, 8,
            6, 5, 12,
            12, 7, 6,
            10, 7, 0,
            7, 12, 0,
            9, 8, 10,
            10, 11, 9,
            5, 4, 13,
            13, 12, 5,
            3, 4, 5,
            3, 5, 6,
            3, 6, 8,
            8, 9, 3
        ];

        public override ushort[] StructureIndices { get; } =
        [
            0, 1,
            1, 2,
            2, 3,
            3, 4,
            4, 5,
            5, 6,
            6, 7,
            0, 7,
            3, 9,
            8, 9,
            6, 8,
            0, 10,
            7, 10,
            1, 11,
            2, 11,
            0, 12,
            7, 12,
            1, 13,
            2, 13,
            10, 11,
            12, 13,
            8, 10,
            9, 11,
            5, 12,
            4, 13
        ];

        public override ushort[] DetailIndices { get; } =
        [
            14, 15,
            15, 16,
            16, 17,
            14, 17
        ];
    }
}
