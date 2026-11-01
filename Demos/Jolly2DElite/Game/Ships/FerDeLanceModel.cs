using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class FerDeLanceModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new(  0.0f, -14.0f, 108.0f),
            new(-40.0f, -14.0f,  -4.0f),
            new(-12.0f, -14.0f, -52.0f),
            new( 12.0f, -14.0f, -52.0f),
            new( 40.0f, -14.0f,  -4.0f),
            new(-40.0f,  14.0f,  -4.0f),
            new(-12.0f,   2.0f, -52.0f),
            new( 12.0f,   2.0f, -52.0f),
            new( 40.0f,  14.0f,  -4.0f),
            new(  0.0f,  18.0f, -20.0f),
            new( -3.0f, -11.235294f, 96.941177f),
            new(-26.0f,   8.470589f, 18.117647f),
            new(-16.0f,  14.0f,  -4.0f),
            new(  3.0f, -11.235294f, 96.941177f),
            new( 26.0f,   8.470589f, 18.117647f),
            new( 16.0f,  14.0f,  -4.0f),
            new(  0.0f, -14.0f, -20.0f),
            new(-14.0f, -14.0f,  44.0f),
            new( 14.0f, -14.0f,  44.0f)
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            0, 9, 5,
            9, 0, 8,
            0, 5, 1,
            5, 2, 1,
            2, 5, 6,
            6, 3, 2,
            3, 6, 7,
            3, 8, 4,
            8, 3, 7,
            0, 4, 8,
            6, 5, 9,
            7, 6, 9,
            8, 7, 9,
            0, 3, 4,
            0, 2, 3,
            2, 0, 1
        ];

        public override ushort[] StructureIndices { get; } =
        [
            0, 1,
            1, 2,
            2, 3,
            3, 4,
            0, 4,
            0, 5,
            5, 6,
            6, 7,
            7, 8,
            0, 8,
            5, 9,
            6, 9,
            7, 9,
            8, 9,
            1, 5,
            2, 6,
            3, 7,
            4, 8
        ];

        public override ushort[] DetailIndices { get; } =
        [
            10, 11,
            11, 12,
            10, 12,
            13, 14,
            14, 15,
            13, 15,
            16, 17,
            16, 18,
            17, 18
        ];
    }
}
