
using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class CobraMkIModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new(-18.0f,  -1.0f,  50.0f),
            new( 18.0f,  -1.0f,  50.0f),
            new(-66.0f,   0.0f,   7.0f),
            new( 66.0f,   0.0f,   7.0f),
            new(-32.0f,  12.0f, -38.0f),
            new( 32.0f,  12.0f, -38.0f),
            new(-54.0f, -12.0f, -38.0f),
            new( 54.0f, -12.0f, -38.0f),
            new(  0.0f,  12.0f,  -6.0f),
            new(  0.0f,  -1.0f,  50.0f),
            new(  0.0f,  -1.0f,  60.0f)
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            0, 1, 8,
            0, 6, 7,
            7, 1, 0,
            0, 8, 4,
            4, 2, 0,
            0, 2, 6,
            1, 3, 5,
            5, 8, 1,
            1, 7, 3,
            8, 5, 4,
            6, 4, 5,
            5, 7, 6,
            2, 4, 6,
            3, 7, 5
        ];

        public override ushort[] StructureIndices { get; } =
        [
            0, 1,
            0, 2,
            2, 6,
            6, 7,
            3, 7,
            1, 3,
            2, 4,
            4, 5,
            3, 5,
            0, 8,
            1, 8,
            4, 8,
            5, 8,
            4, 6,
            5, 7,
            0, 6,
            1, 7,
            9, 10
        ];

        public override ushort[] DetailIndices { get; } = [];
    }
}
