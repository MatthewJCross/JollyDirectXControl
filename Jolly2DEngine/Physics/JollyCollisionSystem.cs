using Jolly2DEngine.Math;
using Jolly2DEngine.Graphics;
using System.Numerics;

namespace Jolly2DEngine.Physics
{
    public static class JollyCollisionSystem
    {
        public static bool Check(JollyCollider colliderA, JollyCollider colliderB)
        {
            ArgumentNullException.ThrowIfNull(colliderA);
            ArgumentNullException.ThrowIfNull(colliderB);

            if (!colliderA.CanCollideWith(colliderB))
            {
                return false;
            }

            return CheckShape(colliderA, colliderB);
        }

        private static bool CheckShape(JollyCollider colliderA, JollyCollider colliderB)
        {
            if (colliderA.Shape == JollyColliderShape.Pixel || colliderB.Shape == JollyColliderShape.Pixel)
            {
                return CheckPixelCollision(colliderA, colliderB);
            }

            if (colliderA.Shape == JollyColliderShape.Circle && colliderB.Shape == JollyColliderShape.Circle)
            {
                return CheckCircleCircle(colliderA, colliderB);
            }

            if (colliderA.Shape == JollyColliderShape.Circle)
            {
                return CheckCircleRectangle(colliderA, colliderB);
            }

            if (colliderB.Shape == JollyColliderShape.Circle)
            {
                return CheckCircleRectangle(colliderB, colliderA);
            }

            return CheckRectangleRectangle(colliderA, colliderB);
        }

        private static bool CheckRectangleRectangle(JollyCollider colliderA, JollyCollider colliderB)
        {
            GetRectangleBounds(colliderA, out float aMinX, out float aMinY, out float aMaxX, out float aMaxY);
            GetRectangleBounds(colliderB, out float bMinX, out float bMinY, out float bMaxX, out float bMaxY);

            return aMinX < bMaxX && aMaxX > bMinX && aMinY < bMaxY && aMaxY > bMinY;
        }

        private static bool CheckCircleCircle(JollyCollider colliderA, JollyCollider colliderB)
        {
            float dx = colliderA.Sprite.Position.X - colliderB.Sprite.Position.X;
            float dy = colliderA.Sprite.Position.Y - colliderB.Sprite.Position.Y;
            float radius = colliderA.Radius * GetMaximumScale(colliderA) + colliderB.Radius * GetMaximumScale(colliderB);

            return dx * dx + dy * dy <= radius * radius;
        }

        private static bool CheckCircleRectangle(JollyCollider circle, JollyCollider rectangle)
        {
            JollyVector2 local = WorldToLocal(rectangle.Sprite, circle.Sprite.Position);

            float halfWidth = rectangle.Width * MathF.Abs(rectangle.Sprite.Scale.X) * 0.5f;
            float halfHeight = rectangle.Height * MathF.Abs(rectangle.Sprite.Scale.Y) * 0.5f;

            float closestX = System.Math.Clamp(local.X, -halfWidth, halfWidth);
            float closestY = System.Math.Clamp(local.Y, -halfHeight, halfHeight);

            float dx = local.X - closestX;
            float dy = local.Y - closestY;

            float radius = circle.Radius * GetMaximumScale(circle);
            return dx * dx + dy * dy <= radius * radius;
        }

        private static float GetMaximumScale(JollyCollider collider)
        {
            return MathF.Max(MathF.Abs(collider.Sprite.Scale.X), MathF.Abs(collider.Sprite.Scale.Y));
        }

        private static bool CheckPixelCollision(JollyCollider colliderA, JollyCollider colliderB)
        {
            JollySprite spriteA = colliderA.Sprite;
            JollySprite spriteB = colliderB.Sprite;

            GetWorldBounds(colliderA, out float aMinX, out float aMinY, out float aMaxX, out float aMaxY);
            GetWorldBounds(colliderB, out float bMinX, out float bMinY, out float bMaxX, out float bMaxY);

            int startX = (int)MathF.Floor(MathF.Max(aMinX, bMinX));
            int endX = (int)MathF.Ceiling(MathF.Min(aMaxX, bMaxX));
            int startY = (int)MathF.Floor(MathF.Max(aMinY, bMinY));
            int endY = (int)MathF.Ceiling(MathF.Min(aMaxY, bMaxY));

            for (int y = startY; y < endY; y++)
            {
                for (int x = startX; x < endX; x++)
                {
                    JollyVector2 worldPoint = new JollyVector2(x + 0.5f, y + 0.5f);

                    if (!IsOpaqueAtWorldPoint(colliderA, worldPoint))
                    {
                        continue;
                    }

                    if (!IsOpaqueAtWorldPoint(colliderB, worldPoint))
                    {
                        continue;
                    }

                    JollyCollisionDebug.PixelHit(spriteA, spriteB, x, y);

                    return true;
                }
            }

            return false;
        }

        private static bool IsOpaqueAtWorldPoint(JollyCollider collider, JollyVector2 worldPoint)
        {
            if (collider.Shape == JollyColliderShape.Circle)
            {
                JollyVector2 delta = worldPoint - collider.Sprite.Position;

                if (delta.LengthSquared() > collider.Radius * collider.Radius)
                {
                    return false;
                }
            }

            if (collider.Shape == JollyColliderShape.Rectangle)
            {
                JollyVector2 local = WorldToLocal(collider.Sprite, worldPoint);
                float halfWidth = collider.Width * 0.5f;
                float halfHeight = collider.Height * 0.5f;

                if (local.X < -halfWidth || local.X >= halfWidth || local.Y < -halfHeight || local.Y >= halfHeight)
                {
                    return false;
                }
            }

            JollyVector2 localSprite = WorldToLocal(collider.Sprite, worldPoint);

            float halfSpriteWidth = collider.Sprite.Width * 0.5f;
            float halfSpriteHeight = collider.Sprite.Height * 0.5f;

            if (localSprite.X < -halfSpriteWidth || localSprite.X >= halfSpriteWidth || localSprite.Y < -halfSpriteHeight || localSprite.Y >= halfSpriteHeight)
            {
                return false;
            }

            float u = (localSprite.X + halfSpriteWidth) / collider.Sprite.Width;
            float v = (localSprite.Y + halfSpriteHeight) / collider.Sprite.Height;

            if (collider.Sprite.FlipHorizontal)
            {
                u = 1.0f - u;
            }

            if (collider.Sprite.FlipVertical)
            {
                v = 1.0f - v;
            }

            float sourceU = collider.Sprite.SourceX + u * collider.Sprite.SourceWidth;
            float sourceV = collider.Sprite.SourceY + v * collider.Sprite.SourceHeight;

            int pixelX = System.Math.Clamp((int)(sourceU * collider.Sprite.Texture.Width), 0, collider.Sprite.Texture.Width - 1);
            int pixelY = System.Math.Clamp((int)(sourceV * collider.Sprite.Texture.Height), 0, collider.Sprite.Texture.Height - 1);

            return collider.Sprite.Texture.GetAlpha(pixelX, pixelY) >= collider.AlphaThreshold;
        }

        private static void GetRectangleBounds(JollyCollider collider, out float minX, out float minY, out float maxX, out float maxY)
        {
            float halfWidth = collider.Width * MathF.Abs(collider.Sprite.Scale.X) * 0.5f;
            float halfHeight = collider.Height * MathF.Abs(collider.Sprite.Scale.Y) * 0.5f;
            float radians = collider.Sprite.Rotation;
            float cos = MathF.Abs(MathF.Cos(radians));
            float sin = MathF.Abs(MathF.Sin(radians));
            float worldWidth = halfWidth * cos + halfHeight * sin;
            float worldHeight = halfWidth * sin + halfHeight * cos;

            minX = collider.Sprite.Position.X - worldWidth;
            maxX = collider.Sprite.Position.X + worldWidth;
            minY = collider.Sprite.Position.Y - worldHeight;
            maxY = collider.Sprite.Position.Y + worldHeight;
        }

        private static void GetWorldBounds(JollyCollider collider, out float minX, out float minY, out float maxX, out float maxY)
        {
            switch (collider.Shape)
            {
                case JollyColliderShape.Circle:
                    minX = collider.Sprite.Position.X - collider.Radius;
                    maxX = collider.Sprite.Position.X + collider.Radius;
                    minY = collider.Sprite.Position.Y - collider.Radius;
                    maxY = collider.Sprite.Position.Y + collider.Radius;
                    return;

                case JollyColliderShape.Rectangle:
                    GetRectangleBounds(collider, out minX, out minY, out maxX, out maxY);
                    return;

                default:
                    float halfWidth = collider.Sprite.Width * 0.5f;
                    float halfHeight = collider.Sprite.Height * 0.5f;
                    float radians = collider.Sprite.Rotation;
                    float cos = MathF.Abs(MathF.Cos(radians));
                    float sin = MathF.Abs(MathF.Sin(radians));
                    float worldWidth = halfWidth * cos + halfHeight * sin;
                    float worldHeight = halfWidth * sin + halfHeight * cos;

                    minX = collider.Sprite.Position.X - worldWidth;
                    maxX = collider.Sprite.Position.X + worldWidth;
                    minY = collider.Sprite.Position.Y - worldHeight;
                    maxY = collider.Sprite.Position.Y + worldHeight;
                    return;
            }
        }

        private static JollyVector2 WorldToLocal(JollySprite sprite, JollyVector2 worldPoint)
        {
            float radians = -sprite.Rotation;
            float cos = MathF.Cos(radians);
            float sin = MathF.Sin(radians);

            float dx = worldPoint.X - sprite.Position.X;
            float dy = worldPoint.Y - sprite.Position.Y;

            return new JollyVector2(dx * cos - dy * sin, dx * sin + dy * cos);
        }
    }
}