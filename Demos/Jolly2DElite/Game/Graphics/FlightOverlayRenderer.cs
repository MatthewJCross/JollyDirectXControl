using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using System.Numerics;

namespace Jolly2DElite.Game.Graphics
{
    internal sealed class FlightOverlayRenderer
    {
        private readonly JollyColor _crosshairColor = new(0, 255, 80, 255);
        private readonly JollyColor _laserColor = new(255, 255, 0, 0);
        private readonly JollyColor _laserCoreColor = JollyColor.White;

        public void DrawCrosshair(List<JollyLineSnapshot> lines, Vector2 center)
        {
            const float innerRadius = 5.0f;
            const float outerRadius = 13.0f;
            const float tickLength = 5.0f;

            AddLine(lines, new Vector2(center.X - outerRadius, center.Y), new Vector2(center.X - innerRadius, center.Y), _crosshairColor, 1.0f);
            AddLine(lines, new Vector2(center.X + innerRadius, center.Y), new Vector2(center.X + outerRadius, center.Y), _crosshairColor, 1.0f);
            AddLine(lines, new Vector2(center.X, center.Y - outerRadius), new Vector2(center.X, center.Y - innerRadius), _crosshairColor, 1.0f);
            AddLine(lines, new Vector2(center.X, center.Y + innerRadius), new Vector2(center.X, center.Y + outerRadius), _crosshairColor, 1.0f);

            AddLine(lines, new Vector2(center.X - outerRadius, center.Y - tickLength), new Vector2(center.X - outerRadius, center.Y - 2.0f), _crosshairColor, 1.0f);
            AddLine(lines, new Vector2(center.X + outerRadius, center.Y - tickLength), new Vector2(center.X + outerRadius, center.Y - 2.0f), _crosshairColor, 1.0f);
            AddLine(lines, new Vector2(center.X - outerRadius, center.Y + 2.0f), new Vector2(center.X - outerRadius, center.Y + tickLength), _crosshairColor, 1.0f);
            AddLine(lines, new Vector2(center.X + outerRadius, center.Y + 2.0f), new Vector2(center.X + outerRadius, center.Y + tickLength), _crosshairColor, 1.0f);
        }

        public void DrawLaser(List<JollyLineSnapshot> lines, Vector2 center)
        {
            AddLine(lines, new Vector2(100.0f, 500.0f), new Vector2(center.X, center.Y), JollyColor.Red, 6.0f, 1.0f, 0.30f);
            AddLine(lines, new Vector2(580.0f, 500.0f), new Vector2(center.X, center.Y), JollyColor.Red, 6.0f, 1.0f, 0.30f);
        }

        private static void AddLine(List<JollyLineSnapshot> lines, Vector2 start, Vector2 end, JollyColor color, float thickness, float startThickness = 1.0f, float endThickness = 1.0f)
        {
            lines.Add(new JollyLineSnapshot(start, end, color, thickness, startThickness, endThickness)
            {
                GlowEnabled = false
            });
        }

        public void DrawShieldBars(List<JollyLineSnapshot> lines, float cockpitTop, float forwardShield, float aftShield)
        {
            const float originalWidth = 256.0f;
            const int originalX = 16;
            const int originalBarHeight = 3;
            const int originalRowSpacing = 8;
            const int maximumBarLength = 15;

            float scale = 680.0f / originalWidth;
            float barX = originalX * scale;
            float barWidth = 2.0f * scale;
            float barHeight = originalBarHeight * scale;

            JollyColor magenta = new(255, 0, 255, 255);
            JollyColor red = new(255, 0, 0, 255);

            DrawShieldBar(lines, barX, cockpitTop + 2.0f * scale, forwardShield, scale, barWidth, barHeight, magenta, red);
            DrawShieldBar(lines, barX, cockpitTop + (originalRowSpacing + 2.0f) * scale, aftShield, scale, barWidth, barHeight, magenta, red);
        }

        private static void DrawShieldBar(List<JollyLineSnapshot> lines, float x, float y, float shield, float scale, float segmentWidth, float thickness, JollyColor magenta, JollyColor red)
        {
            int length = Math.Clamp((int)(shield / 100.0f * 15.0f), 0, 15);

            if (length == 0)
            {
                return;
            }

            if (length < 3)
            {
                AddLine(lines, new Vector2(x, y), new Vector2(x + length * segmentWidth, y), red, thickness);
                return;
            }

            for (int index = 0; index < length; index++)
            {
                JollyColor colour = index % 4 < 2 ? magenta : red;

                float startX = x + index * segmentWidth;
                float endX = startX + segmentWidth + 0.25f;

                AddLine(lines, new Vector2(startX, y), new Vector2(endX, y), colour, thickness);
            }
        }
    }
}
