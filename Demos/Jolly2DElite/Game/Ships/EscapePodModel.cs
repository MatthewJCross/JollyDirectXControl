using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class EscapePodModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new(-7.0f,   0.0f,  36.0f),
            new(-7.0f, -14.0f, -12.0f),
            new(-7.0f,  14.0f, -12.0f),
            new(21.0f,   0.0f,   0.0f)
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            2, 3, 1,
            0, 3, 2,
            0, 1, 3,
            2, 1, 0
        ];

        public override ushort[] StructureIndices { get; } =
        [
            0, 1,
            1, 2,
            2, 3,
            0, 3,
            0, 2,
            1, 3
        ];

        public override ushort[] DetailIndices { get; } = [];
    }
}
