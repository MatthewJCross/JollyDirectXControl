using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class CougarModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new(  0.0f,  5.0f,  67.0f),
            new(-20.0f,  0.0f,  40.0f),
            new(-40.0f,  0.0f, -40.0f),
            new(  0.0f, 14.0f, -40.0f),
            new(  0.0f,-14.0f, -40.0f),
            new( 20.0f,  0.0f,  40.0f),
            new( 40.0f,  0.0f, -40.0f),
            new(-36.0f,  0.0f,  56.0f),
            new(-60.0f,  0.0f, -20.0f),
            new( 36.0f,  0.0f,  56.0f),
            new( 60.0f,  0.0f, -20.0f),
            new(-0.16948877f, 7.74791527f, 35.06290817f),
            new(-0.13055216f, 8.57609653f, 25.04845619f),
            new(-12.15568066f, 2.42822289f, 45.03601837f),
            new( 12.15568066f, 2.42822289f, 45.03601837f),
            new(-10.0f,  6.0f, -40.0f),
            new(-10.0f, -6.0f, -40.0f),
            new( 10.0f, -6.0f, -40.0f),
            new( 10.0f,  6.0f, -40.0f)
        ];

        // glTF primitive 1: structure lines.
        public override ushort[] StructureIndices { get; } =
        [
            0, 1,
            1, 7,
            7, 8,
            2, 8,
            2, 3,
            3, 6,
            2, 4,
            4, 6,
            6, 10,
            9, 10,
            5, 9,
            0, 5,
            0, 3,
            1, 4,
            4, 5,
            1, 2,
            5, 6
        ];

        // glTF primitive 2: detail lines.
        public override ushort[] DetailIndices { get; } =
        [
            12, 13,
            11, 13,
            11, 14,
            12, 14,
            15, 16,
            16, 18,
            17, 18,
            15, 17
        ];

        // glTF primitive 0: surface triangles.
        public override ushort[] SurfaceIndices { get; } =
        [
            3, 1, 0,
            3, 7, 1,
            3, 8, 7,
            8, 3, 2,
            2, 7, 8,
            2, 1, 7,
            1, 2, 4,
            0, 4, 5,
            4, 0, 1,
            5, 10, 9,
            5, 6, 10,
            6, 5, 4,
            5, 3, 0,
            9, 3, 5,
            9, 6, 3,
            6, 9, 10,
            2, 6, 4,
            6, 2, 3
        ];
    }
}