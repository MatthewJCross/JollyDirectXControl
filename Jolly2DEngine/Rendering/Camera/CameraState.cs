using System.Numerics;

namespace Jolly2DEngine.Rendering.Camera
{
    public readonly struct CameraState
    {
        public Vector2 Position { get; }
        public float Zoom { get; }
        public float Rotation { get; }
        public Vector2 ViewportSize { get; }
        public float ViewportScale { get; }
        public Vector2 ViewportOffset { get; }

        public CameraState(Vector2 position, float zoom, float rotation, Vector2 viewportSize, float viewportScale = 1.0f, Vector2 viewportOffset = default)
        {
            Position = position;
            Zoom = zoom;
            Rotation = rotation;
            ViewportSize = viewportSize;
            ViewportScale = viewportScale;
            ViewportOffset = viewportOffset;
        }

        public Matrix3x2 GetWorldToScreenMatrix()
        {
            Matrix3x2 matrix = Matrix3x2.CreateTranslation(-Position);

            matrix *= Matrix3x2.CreateRotation(-Rotation);
            matrix *= Matrix3x2.CreateScale(Zoom * ViewportScale);

            matrix *= Matrix3x2.CreateTranslation(ViewportOffset.X + ViewportSize.X * 0.5f, ViewportOffset.Y + ViewportSize.Y * 0.5f);

            return matrix;
        }

        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, GetWorldToScreenMatrix());
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            Matrix3x2 matrix = GetWorldToScreenMatrix();

            if (!Matrix3x2.Invert(matrix, out Matrix3x2 inverse))
            {
                return Vector2.Zero;
            }

            return Vector2.Transform(screenPosition, inverse);
        }
    }
}
