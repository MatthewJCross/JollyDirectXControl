using Jolly2DEngine.Core;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using Jolly2DEngine.Physics;
using Jolly2DEngine.World;

namespace Jolly2DEngine.Rendering.DirectX
{
    public sealed class JollyPhysicsDebugRenderer
    {
        private readonly DirectXJollyRenderer _renderer;
        private readonly JollyTexture _greenTexture;
        private readonly JollyTexture _redTexture;
        private readonly JollyTexture _orangeTexture;
        private readonly JollyTexture _yellowTexture;
        private readonly JollyTexture _whiteTexture;

        private readonly List<JollySprite> _sprites = new();

        public bool Enabled { get; set; } = true;
        public bool TileMapEnabled { get; set; } = true;

        public JollyPhysicsDebugRenderer(DirectXJollyRenderer renderer)
        {
            ArgumentNullException.ThrowIfNull(renderer);

            _renderer = renderer;
            _greenTexture = renderer.CreateSolidTexture(0, 255, 0, 255);
            _redTexture = renderer.CreateSolidTexture(255, 0, 0, 255);
            _orangeTexture = renderer.CreateSolidTexture(255, 165, 0, 255);
            _yellowTexture = renderer.CreateSolidTexture(255, 255, 0, 255);
            _whiteTexture = renderer.CreateSolidTexture(255, 255, 255, 255);
        }

        public IReadOnlyList<JollySprite> Sprites => _sprites;

        public void Update(IReadOnlyList<JollyGameObject> objects)
        {
            Update(objects, null, 0.0f, 0.0f);
        }

        public void Update(IReadOnlyList<JollyGameObject> objects, JollyTileMap? tileMap, float tileMapX, float tileMapY)
        {
            ArgumentNullException.ThrowIfNull(objects);

            _sprites.Clear();

            if (!Enabled)
            {
                return;
            }

            foreach (JollyGameObject gameObject in objects)
            {
                if (!gameObject.Visible)
                {
                    continue;
                }

                if (gameObject.Collider == null || !gameObject.Collider.Enabled)
                {
                    continue;
                }

                DrawCollider(gameObject);
            }

            if (TileMapEnabled && tileMap != null)
            {
                DrawTileMap(tileMap, tileMapX, tileMapY);
            }
        }

        private void DrawCollider(JollyGameObject gameObject)
        {
            JollyCollider collider = gameObject.Collider!;

            JollyTexture outlineTexture = GetColliderTexture(gameObject);

            if (collider.Shape == JollyColliderShape.Circle)
            {
                DrawCircle(gameObject, collider, outlineTexture);
            }
            else
            {
                DrawRectangle(gameObject, collider, outlineTexture);
            }

            DrawCentrePoint(gameObject);

            if (gameObject.UsePhysics)
            {
                DrawVelocity(gameObject);
            }
        }

        private JollyTexture GetColliderTexture(JollyGameObject gameObject)
        {
            if (gameObject.UsePhysics)
            {
                if (gameObject.IsGrounded)
                {
                    return _greenTexture;
                }

                return _redTexture;
            }

            return _orangeTexture;
        }

        private void DrawTileMap(JollyTileMap tileMap, float tileMapX, float tileMapY)
        {
            for (int y = 0; y < tileMap.Height; y++)
            {
                for (int x = 0; x < tileMap.Width; x++)
                {
                    if (!tileMap.IsSolid(x, y))
                    {
                        continue;
                    }

                    float left = tileMapX + x * tileMap.TileWidth;
                    float top = tileMapY + y * tileMap.TileHeight;
                    float right = left + tileMap.TileWidth;
                    float bottom = top + tileMap.TileHeight;

                    DrawLine(_whiteTexture, left, top, right, top, 1.0f);
                    DrawLine(_whiteTexture, left, bottom, right, bottom, 1.0f);
                    DrawLine(_whiteTexture, left, top, left, bottom, 1.0f);
                    DrawLine(_whiteTexture, right, top, right, bottom, 1.0f);
                }
            }
        }

        private void DrawRectangle(JollyGameObject gameObject, JollyCollider collider, JollyTexture texture)
        {
            float width = collider.Width;
            float height = collider.Height;

            if (width <= 0.0f || height <= 0.0f)
            {
                return;
            }

            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;
            float thickness = 2.0f;

            DrawLine(texture, gameObject.Position.X - halfWidth, gameObject.Position.Y - halfHeight, gameObject.Position.X + halfWidth, gameObject.Position.Y - halfHeight, thickness);
            DrawLine(texture, gameObject.Position.X - halfWidth, gameObject.Position.Y + halfHeight, gameObject.Position.X + halfWidth, gameObject.Position.Y + halfHeight, thickness);
            DrawLine(texture, gameObject.Position.X - halfWidth, gameObject.Position.Y - halfHeight, gameObject.Position.X - halfWidth, gameObject.Position.Y + halfHeight, thickness);
            DrawLine(texture, gameObject.Position.X + halfWidth, gameObject.Position.Y - halfHeight, gameObject.Position.X + halfWidth, gameObject.Position.Y + halfHeight, thickness);
        }

        private void DrawCircle(JollyGameObject gameObject, JollyCollider collider, JollyTexture texture)
        {
            float radius = collider.Radius;

            if (radius <= 0.0f)
            {
                return;
            }

            const int segments = 32;
            float previousX = gameObject.Position.X + MathF.Cos(0.0f) * radius;
            float previousY = gameObject.Position.Y + MathF.Sin(0.0f) * radius;

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * MathF.PI * 2.0f / segments;
                float currentX = gameObject.Position.X + MathF.Cos(angle) * radius;
                float currentY = gameObject.Position.Y + MathF.Sin(angle) * radius;

                DrawLine(texture, previousX, previousY, currentX, currentY, 2.0f);

                previousX = currentX;
                previousY = currentY;
            }
        }

        private void DrawCentrePoint(JollyGameObject gameObject)
        {
            JollySprite sprite = _renderer.CreateSprite(_whiteTexture);
            sprite.Name = "PhysicsDebugCentre";
            sprite.Position = gameObject.Position;
            sprite.Size = new JollyVector2(6.0f, 6.0f);
            sprite.Rotation = 0.0f;
            sprite.Opacity = 1.0f;
            sprite.Visible = true;
            sprite.Layer = 10000;

            _sprites.Add(sprite);
        }

        private void DrawVelocity(JollyGameObject gameObject)
        {
            float velocityX = gameObject.Velocity.X;
            float velocityY = gameObject.Velocity.Y;
            float length = MathF.Sqrt(velocityX * velocityX + velocityY * velocityY);

            if (length < 0.1f)
            {
                return;
            }

            float scale = 0.20f;
            float endX = gameObject.Position.X + velocityX * scale;
            float endY = gameObject.Position.Y + velocityY * scale;

            DrawLine(_yellowTexture, gameObject.Position.X, gameObject.Position.Y, endX, endY, 4.0f);
        }

        private void DrawLine(JollyTexture texture, float startX, float startY, float endX, float endY, float thickness)
        {
            float deltaX = endX - startX;
            float deltaY = endY - startY;
            float length = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);

            if (length <= 0.001f)
            {
                return;
            }

            float centreX = (startX + endX) * 0.5f;
            float centreY = (startY + endY) * 0.5f;
            float rotation = MathF.Atan2(deltaY, deltaX);

            JollySprite sprite = _renderer.CreateSprite(texture);
            sprite.Name = "PhysicsDebugLine";
            sprite.Position = new JollyVector2(centreX, centreY);
            sprite.Size = new JollyVector2(length, thickness);
            sprite.Rotation = rotation;
            sprite.Opacity = 1.0f;
            sprite.Visible = true;
            sprite.Layer = 10000;

            _sprites.Add(sprite);
        }
    }
}