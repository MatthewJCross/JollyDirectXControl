using Jolly2DEngine.Math;
using System.Numerics;

namespace Jolly2DEngine.Graphics
{
    public readonly struct JollyTriangleSnapshot
    {
        public Vector2 A { get; }
        public Vector2 B { get; }
        public Vector2 C { get; }
        public JollyColor Colour { get; }

        public JollyTriangleSnapshot(Vector2 a, Vector2 b, Vector2 c, JollyColor colour)
        {
            A = a;
            B = b;
            C = c;
            Colour = colour;
        }
    }
}