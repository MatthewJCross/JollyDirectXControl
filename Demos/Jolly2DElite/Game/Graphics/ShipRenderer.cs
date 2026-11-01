using Jolly2DElite.Game;
using Jolly2DElite.Game.Graphics;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using System.Numerics;

namespace Jolly2DElite.Graphics
{
    internal sealed class ShipRenderer : Renderer
    {

        public void Draw(List<JollyLineSnapshot> lines, List<JollyTriangleSnapshot> triangles, ShipModel model, Vector2 screenCenter, float scale, float rotationY, float rotationX, float cameraDistance = 220.0f, float focalLength = 500.0f)
        {
            Vector2[] projected = new Vector2[model.Vertices.Length];
            bool[] visible = new bool[model.Vertices.Length];
            float[] depths = new float[model.Vertices.Length];

            float sinY = MathF.Sin(rotationY);
            float cosY = MathF.Cos(rotationY);
            float sinX = MathF.Sin(rotationX);
            float cosX = MathF.Cos(rotationX);

            for (int index = 0; index < model.Vertices.Length; index++)
            {
                Vector3 vertex = model.Vertices[index];

                float rotatedX = vertex.X * cosY + vertex.Z * sinY;
                float rotatedZ = -vertex.X * sinY + vertex.Z * cosY;

                float rotatedY = vertex.Y * cosX - rotatedZ * sinX;
                rotatedZ = vertex.Y * sinX + rotatedZ * cosX;
                
                float depth = cameraDistance - rotatedZ;
                depths[index] = depth;

                if (depth <= 1.0f)
                {
                    continue;
                }

                projected[index] = new Vector2(screenCenter.X + rotatedX * focalLength / depth * scale, screenCenter.Y - rotatedY * focalLength / depth * scale);

                visible[index] = true;
            }

            List<(float Depth, JollyTriangleSnapshot Triangle)> surfaceTriangles = [];

            for (int index = 0; index < model.SurfaceIndices.Length; index += 3)
            {
                int a = model.SurfaceIndices[index];
                int b = model.SurfaceIndices[index + 1];
                int c = model.SurfaceIndices[index + 2];

                if (!visible[a] || !visible[b] || !visible[c])
                {
                    continue;
                }

                float averageDepth = (depths[a] + depths[b] + depths[c]) / 3.0f;
                JollyTriangleSnapshot triangle = new(projected[a], projected[b], projected[c], _shipColor);
                surfaceTriangles.Add((averageDepth, triangle));
            }

            foreach ((float Depth, JollyTriangleSnapshot Triangle) surface in surfaceTriangles.OrderByDescending(static surface => surface.Depth))
            {
                triangles.Add(surface.Triangle);
            }

            DrawEdges(lines, model.StructureIndices, projected, visible, 0.8f);
            DrawEdges(lines, model.DetailIndices, projected, visible, 0.4f);
        }

        //private void DrawEdges(List<JollyLineSnapshot> lines, ushort[] indices, Vector2[] projected, bool[] visible, float thickness)
        //{
        //    for (int index = 0; index < indices.Length; index += 2)
        //    {
        //        int startIndex = indices[index];
        //        int endIndex = indices[index + 1];

        //        if (!visible[startIndex] || !visible[endIndex])
        //        {
        //            continue;
        //        }

        //        lines.Add(new JollyLineSnapshot(projected[startIndex], projected[endIndex], _lineColor, thickness)
        //        {
        //            GlowEnabled = false
        //        });
        //    }
        //}
    }
}

