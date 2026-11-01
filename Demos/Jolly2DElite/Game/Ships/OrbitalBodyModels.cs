
using System.Numerics;

namespace Jolly2DElite.Game
{
    internal abstract class OrbitalBodyModel : ShipModel
    {
        public override Vector3[] Vertices { get; } =
        [
            new( 0.0f,  96.0f,   0.0f),
            new( 0.0f, -96.0f,   0.0f),
            new( 96.0f,  0.0f,   0.0f),
            new(-96.0f,  0.0f,   0.0f),
            new( 0.0f,  0.0f,  96.0f),
            new( 0.0f,  0.0f, -96.0f)
        ];

        public override ushort[] SurfaceIndices { get; } =
        [
            0, 2, 4,
            0, 4, 3,
            0, 3, 5,
            0, 5, 2,
            1, 4, 2,
            1, 3, 4,
            1, 5, 3,
            1, 2, 5
        ];

        public override ushort[] StructureIndices { get; } =
        [
            0, 2, 0, 3, 0, 4, 0, 5,
            1, 2, 1, 3, 1, 4, 1, 5,
            2, 4, 4, 3, 3, 5, 5, 2
        ];

        public override ushort[] DetailIndices { get; } = [];
    }

    internal sealed class PlanetModel : OrbitalBodyModel
    {
    }

    internal sealed class SunModel : OrbitalBodyModel
    {
    }
}
