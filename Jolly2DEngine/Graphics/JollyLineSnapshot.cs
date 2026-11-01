using Jolly2DEngine.Math;
using System.Numerics;

namespace Jolly2DEngine.Graphics
{
    public readonly struct JollyLineSnapshot(Vector2 start, Vector2 end, JollyColor color, float thickness, float startThickness = 1.0f, float endThickness = 1.0f)
    {
        public Vector2 Start { get; } = start;
        public Vector2 End { get; } = end;
        public JollyColor Color { get; } = color;
        public float Thickness { get; } = thickness;
        public float StartThickness { get; } = startThickness;
        public float EndThickness { get; } = endThickness;
        public bool GlowEnabled { get; init; } = true;
    }
}


