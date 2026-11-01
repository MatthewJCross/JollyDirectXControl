
using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class ConstrictorModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new( 20.0f, -7.0f,  80.0f),
            new(-20.0f, -7.0f,  80.0f),
            new(-54.0f, -7.0f,  40.0f),
            new(-54.0f, -7.0f, -40.0f),
            new(-20.0f, 13.0f, -40.0f),
            new( 20.0f, 13.0f, -40.0f),
            new( 54.0f, -7.0f, -40.0f),
            new( 54.0f, -7.0f,  40.0f),
            new( 20.0f, 13.0f,   5.0f),
            new(-20.0f, 13.0f,   5.0f),
            new( 20.0f, -7.0f,  62.0f),
            new(-20.0f, -7.0f,  62.0f),
            new( 25.0f, -7.0f, -25.0f),
            new(-25.0f, -7.0f, -25.0f),
            new( 15.0f, -7.0f, -15.0f),
            new(-15.0f, -7.0f, -15.0f),
            new(  0.0f, -7.0f,   0.0f)
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            8, 1, 0,
            1, 8, 9,
            2, 1, 9,
            8, 0, 7,
            6, 8, 7,
            2, 9, 3,
            4, 3, 9,
            4, 8, 5,
            8, 4, 9,
            6, 5, 8,
            4, 6, 3,
            6, 4, 5,
            0, 6, 7,
            0, 3, 6,
            0, 2, 3,
            2, 0, 1
        ];

        public override ushort[] StructureIndices { get; } =
        [
            0, 1,
            1, 2,
            1, 9,
            0, 8,
            0, 7,
            7, 8,
            2, 9,
            2, 3,
            6, 7,
            6, 8,
            5, 8,
            4, 9,
            3, 9,
            3, 4,
            4, 5,
            5, 6,
            3, 6,
            8, 9
        ];

        public override ushort[] DetailIndices { get; } =
        [
            10, 12,
            12, 14,
            10, 14,
            11, 15,
            13, 15,
            11, 13
        ];
    }
}
