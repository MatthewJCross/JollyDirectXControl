using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using System.Numerics;

namespace Jolly2DElite.Game.Graphics
{
    internal class Renderer
    {
        protected readonly JollyColor _shipColor = JollyColor.Cyan;
        protected readonly JollyColor _lineColor = JollyColor.Gray;


        protected void DrawEdges(List<JollyLineSnapshot> lines, ushort[] indices, Vector2[] projected, bool[] visible, float thickness)
        {
            for (int index = 0; index < indices.Length; index += 2)
            {
                int startIndex = indices[index];
                int endIndex = indices[index + 1];

                if (!visible[startIndex] || !visible[endIndex])
                {
                    continue;
                }

                lines.Add(new JollyLineSnapshot(projected[startIndex], projected[endIndex], _lineColor, thickness)
                {
                    GlowEnabled = false
                });
            }
        }
    }
}
