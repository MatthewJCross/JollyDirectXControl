using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using System.Numerics;

namespace Jolly2DElite.Game.Graphics
{
    internal sealed class WorldRenderer : Renderer
    {
        public void DrawUniverse(List<JollyLineSnapshot> lines, List<JollyTriangleSnapshot> triangles, IReadOnlyList<WorldObject> objects, Matrix4x4 viewMatrix, Vector2 screenCenter, float focalLength = 256.0f, float nearPlane = 2.0f)
        {
            List<(float Depth, JollyTriangleSnapshot Triangle)> surfaceTriangles = [];

            foreach (WorldObject worldObject in objects)
            {
                Matrix4x4 modelToWorld = Matrix4x4.CreateRotationX(worldObject.Rotation.X) * Matrix4x4.CreateRotationY(worldObject.Rotation.Y) * Matrix4x4.CreateRotationZ(worldObject.Rotation.Z) * Matrix4x4.CreateScale(worldObject.Scale) * Matrix4x4.CreateTranslation(worldObject.Position);
                CollectModel(lines, worldObject.Model, modelToWorld, viewMatrix, screenCenter, focalLength, nearPlane, surfaceTriangles);
            }

            foreach ((float Depth, JollyTriangleSnapshot Triangle) surface in surfaceTriangles.OrderByDescending(static surface => surface.Depth))
            {
                triangles.Add(surface.Triangle);
            }
        }

        public void DrawWorld(List<JollyLineSnapshot> lines, List<JollyTriangleSnapshot> triangles, ShipModel model, Matrix4x4 modelToWorld, Matrix4x4 viewMatrix, Vector2 screenCenter, float focalLength = 256.0f, float nearPlane = 2.0f)
        {
            List<(float Depth, JollyTriangleSnapshot Triangle)> surfaceTriangles = [];
            CollectModel(lines, model, modelToWorld, viewMatrix, screenCenter, focalLength, nearPlane, surfaceTriangles);

            foreach ((float Depth, JollyTriangleSnapshot Triangle) surface in surfaceTriangles.OrderByDescending(static surface => surface.Depth))
            {
                triangles.Add(surface.Triangle);
            }
        }

        public void DrawShip(List<JollyLineSnapshot> lines, List<JollyTriangleSnapshot> triangles, ShipModel model, Vector3 position, Vector3 rotation, Matrix4x4 viewMatrix, Vector2 screenCenter, float scale = 1.0f, float focalLength = 256.0f, float nearPlane = 2.0f)
        {
            Matrix4x4 modelToWorld = Matrix4x4.CreateRotationX(rotation.X) * Matrix4x4.CreateRotationY(rotation.Y) * Matrix4x4.CreateRotationZ(rotation.Z) * Matrix4x4.CreateScale(scale) * Matrix4x4.CreateTranslation(position);
            DrawWorld(lines, triangles, model, modelToWorld, viewMatrix, screenCenter, focalLength, nearPlane);
        }

        public Matrix4x4 CreateViewMatrix(PlayerShip playerShip)
        {
            Vector3 right = playerShip.Right;
            Vector3 up = playerShip.Up;
            Vector3 forward = playerShip.Forward;
            Vector3 position = playerShip.Position;

            return new Matrix4x4(right.X, up.X, forward.X, 0.0f, right.Y, up.Y, forward.Y, 0.0f, right.Z, up.Z, forward.Z, 0.0f, -Vector3.Dot(position, right), -Vector3.Dot(position, up), -Vector3.Dot(position, forward), 1.0f);
        }


        private void CollectModel(List<JollyLineSnapshot> lines, ShipModel model, Matrix4x4 modelToWorld, Matrix4x4 viewMatrix, Vector2 screenCenter, float focalLength, float nearPlane, List<(float Depth, JollyTriangleSnapshot Triangle)> surfaceTriangles)
        {
            Vector3[] viewVertices = new Vector3[model.Vertices.Length];

            for (int index = 0; index < model.Vertices.Length; index++)
            {
                Vector3 worldPosition = Vector3.Transform(model.Vertices[index], modelToWorld);
                viewVertices[index] = Vector3.Transform(worldPosition, viewMatrix);
            }

            for (int index = 0; index < model.SurfaceIndices.Length; index += 3)
            {
                Vector3 a = viewVertices[model.SurfaceIndices[index]];
                Vector3 b = viewVertices[model.SurfaceIndices[index + 1]];
                Vector3 c = viewVertices[model.SurfaceIndices[index + 2]];

                List<Vector3> polygon = [a, b, c];
                polygon = ClipPolygonAgainstNearPlane(polygon, nearPlane);

                if (polygon.Count < 3)
                {
                    continue;
                }

                Vector2 first = Project(polygon[0], screenCenter, focalLength);

                for (int vertex = 1; vertex < polygon.Count - 1; vertex++)
                {
                    Vector3 secondVertex = polygon[vertex];
                    Vector3 thirdVertex = polygon[vertex + 1];

                    Vector2 second = Project(secondVertex, screenCenter, focalLength);
                    Vector2 third = Project(thirdVertex, screenCenter, focalLength);

                    float averageDepth = (polygon[0].Z + secondVertex.Z + thirdVertex.Z) / 3.0f;

                    JollyTriangleSnapshot triangle = new(first, second, third, _shipColor);
                    surfaceTriangles.Add((averageDepth, triangle));
                }
            }

            DrawClippedEdges(lines, model.StructureIndices, viewVertices, screenCenter, focalLength, nearPlane, 0.8f);
            DrawClippedEdges(lines, model.DetailIndices, viewVertices, screenCenter, focalLength, nearPlane, 0.4f);
        }


        private static List<Vector3> ClipPolygonAgainstNearPlane(List<Vector3> polygon, float nearPlane)
        {
            List<Vector3> clipped = [];

            if (polygon.Count == 0)
            {
                return clipped;
            }

            Vector3 previous = polygon[^1];
            bool previousInside = previous.Z >= nearPlane;

            foreach (Vector3 current in polygon)
            {
                bool currentInside = current.Z >= nearPlane;

                if (currentInside != previousInside)
                {
                    float amount = (nearPlane - previous.Z) / (current.Z - previous.Z);
                    clipped.Add(Vector3.Lerp(previous, current, amount));
                }

                if (currentInside)
                {
                    clipped.Add(current);
                }

                previous = current;
                previousInside = currentInside;
            }

            return clipped;
        }

        private static Vector2 Project(Vector3 position, Vector2 screenCenter, float focalLength)
        {
            return new Vector2(screenCenter.X + position.X * focalLength / position.Z, screenCenter.Y - position.Y * focalLength / position.Z);
        }

        private static void DrawClippedEdges(List<JollyLineSnapshot> lines, ushort[] indices, Vector3[] viewVertices, Vector2 screenCenter, float focalLength, float nearPlane, float thickness)
        {
            for (int index = 0; index < indices.Length; index += 2)
            {
                Vector3 start = viewVertices[indices[index]];
                Vector3 end = viewVertices[indices[index + 1]];

                bool startInside = start.Z >= nearPlane;
                bool endInside = end.Z >= nearPlane;

                if (!startInside && !endInside)
                {
                    continue;
                }

                if (startInside != endInside)
                {
                    float amount = (nearPlane - start.Z) / (end.Z - start.Z);
                    Vector3 intersection = Vector3.Lerp(start, end, amount);

                    if (!startInside)
                    {
                        start = intersection;
                    }
                    else
                    {
                        end = intersection;
                    }
                }

                lines.Add(new JollyLineSnapshot(Project(start, screenCenter, focalLength), Project(end, screenCenter, focalLength), JollyColor.Gray, thickness)
                {
                    GlowEnabled = false
                });
            }
        }
    }
}
