using Jolly2DEngine.Core;
using Jolly2DEngine.Math;
using Jolly2DEngine.World;

namespace Jolly2DEngine.Physics
{
    public static class JollyPhysicsSystem
    {
        public static void Update(JollyGameObject gameObject, JollyGameTime gameTime)
        {
            ArgumentNullException.ThrowIfNull(gameObject);
            ArgumentNullException.ThrowIfNull(gameTime);

            if (!gameObject.UsePhysics)
            {
                return;
            }

            float deltaTime = (float)gameTime.DeltaSeconds;

            if (deltaTime <= 0.0f)
            {
                return;
            }

            gameObject.WasGrounded = gameObject.IsGrounded;
            gameObject.IsGrounded = false;

            float velocityX = gameObject.Velocity.X + gameObject.Acceleration.X * deltaTime;
            float velocityY = gameObject.Velocity.Y + gameObject.Acceleration.Y * deltaTime;

            if (gameObject.UseGravity)
            {
                velocityX += gameObject.Gravity.X * deltaTime;
                velocityY += gameObject.Gravity.Y * deltaTime;
            }

            gameObject.Velocity = new JollyVector2(velocityX, velocityY);
            gameObject.Position = new JollyVector2(gameObject.Position.X + gameObject.Velocity.X * deltaTime, gameObject.Position.Y + gameObject.Velocity.Y * deltaTime);
        }

        public static void ResolveTileMap(JollyGameObject gameObject, JollyTileMap tileMap, float tileMapX, float tileMapY, float deltaTime)
        {
            ArgumentNullException.ThrowIfNull(gameObject);
            ArgumentNullException.ThrowIfNull(tileMap);

            if (!gameObject.UsePhysics)
            {
                return;
            }

            if (gameObject.Collider == null || !gameObject.Collider.Enabled || gameObject.Collider.IsTrigger)
            {
                return;
            }

            if (deltaTime <= 0.0f)
            {
                return;
            }

            GetCollisionBounds(gameObject, out float left, out float top, out float right, out float bottom);

            int minimumTileX = (int)MathF.Floor((left - tileMapX) / tileMap.TileWidth) - 1;
            int maximumTileX = (int)MathF.Floor((right - tileMapX) / tileMap.TileWidth) + 1;
            int minimumTileY = (int)MathF.Floor((top - tileMapY) / tileMap.TileHeight) - 1;
            int maximumTileY = (int)MathF.Floor((bottom - tileMapY) / tileMap.TileHeight) + 1;

            minimumTileX = System.Math.Max(0, minimumTileX);
            maximumTileX = System.Math.Min(tileMap.Width - 1, maximumTileX);
            minimumTileY = System.Math.Max(0, minimumTileY);
            maximumTileY = System.Math.Min(tileMap.Height - 1, maximumTileY);

            for (int tileY = minimumTileY; tileY <= maximumTileY; tileY++)
            {
                for (int tileX = minimumTileX; tileX <= maximumTileX; tileX++)
                {
                    bool solid = tileMap.IsSolid(tileX, tileY);
                    int tileIndex = tileMap.GetTile(tileX, tileY);
                    bool hazard = tileMap.IsHazardTile(tileIndex);

                    if (!solid && !hazard)
                    {
                        continue;
                    }

                    float tileLeft = tileMapX + tileX * tileMap.TileWidth;
                    float tileTop = tileMapY + tileY * tileMap.TileHeight;
                    float tileRight = tileLeft + tileMap.TileWidth;
                    float tileBottom = tileTop + tileMap.TileHeight;

                    if (!TryGetTileCollision(gameObject, tileLeft, tileTop, tileRight, tileBottom, out JollyVector2 normal, out float penetration))
                    {
                        continue;
                    }

                    if (hazard)
                    {
                        tileMap.NotifyHazardContact(gameObject, tileX, tileY, tileIndex);
                    }

                    if (!solid)
                    {
                        continue;
                    }

                    if (penetration <= 0.0f)
                    {
                        continue;
                    }

                    gameObject.Position = new JollyVector2(gameObject.Position.X + normal.X * penetration, gameObject.Position.Y + normal.Y * penetration);

                    float velocityAlongNormal = gameObject.Velocity.X * normal.X + gameObject.Velocity.Y * normal.Y;

                    if (velocityAlongNormal < 0.0f)
                    {
                        gameObject.Velocity = new JollyVector2(gameObject.Velocity.X - normal.X * velocityAlongNormal, gameObject.Velocity.Y - normal.Y * velocityAlongNormal);
                    }

                    if (normal.Y < -0.5f)
                    {
                        gameObject.IsGrounded = true;

                        float conveyorSpeed = tileMap.GetConveyorSpeed(tileIndex);

                        if (conveyorSpeed != 0.0f)
                        {
                            gameObject.Velocity = new JollyVector2(conveyorSpeed, gameObject.Velocity.Y);
                        }
                    }
                }
            }
        }

        public static bool Jump(JollyGameObject gameObject, float velocity)
        {
            ArgumentNullException.ThrowIfNull(gameObject);

            if (!gameObject.UsePhysics)
            {
                return false;
            }

            if (velocity < 0.0f)
            {
                velocity = -velocity;
            }

            gameObject.Velocity = new JollyVector2(gameObject.Velocity.X, -velocity);
            gameObject.IsGrounded = false;
            gameObject.WasGrounded = false;

            return true;
        }

        public static void Resolve(JollyGameObject objectA, JollyGameObject objectB, float deltaTime)
        {
            ArgumentNullException.ThrowIfNull(objectA);
            ArgumentNullException.ThrowIfNull(objectB);

            if (objectA.Collider == null || objectB.Collider == null)
            {
                return;
            }

            if (!objectA.Collider.Enabled || !objectB.Collider.Enabled)
            {
                return;
            }

            if (objectA.Collider.IsTrigger || objectB.Collider.IsTrigger)
            {
                return;
            }

            if (!objectA.UsePhysics && !objectB.UsePhysics)
            {
                return;
            }

            if (objectA.UsePhysics && objectB.UsePhysics)
            {
                return;
            }

            if (objectA.UsePhysics)
            {
                ResolveMovingObject(objectA, objectB, deltaTime);
                return;
            }

            ResolveMovingObject(objectB, objectA, deltaTime);
        }

        private static void ResolveMovingObject(JollyGameObject movingObject, JollyGameObject staticObject, float deltaTime)
        {
            if (movingObject.Collider == null || staticObject.Collider == null)
            {
                return;
            }

            if (!movingObject.UsePhysics)
            {
                return;
            }

            if (!TryGetCollision(movingObject, staticObject, out JollyVector2 normal, out float penetration))
            {
                return;
            }

            if (penetration <= 0.0f)
            {
                return;
            }

            ResolvePosition(movingObject, normal, penetration);
            ResolveVelocity(movingObject, normal);
            ResolveFriction(movingObject, normal, deltaTime);
            UpdateGroundedState(movingObject, normal);
        }

        private static bool TryGetCollision(JollyGameObject movingObject, JollyGameObject staticObject, out JollyVector2 normal, out float penetration)
        {
            normal = new JollyVector2(0.0f, 0.0f);
            penetration = 0.0f;

            JollyCollider movingCollider = movingObject.Collider!;
            JollyCollider staticCollider = staticObject.Collider!;

            if (movingCollider.Shape == JollyColliderShape.Circle && staticCollider.Shape == JollyColliderShape.Circle)
            {
                return TryGetCircleCircleCollision(movingObject, staticObject, out normal, out penetration);
            }

            if (movingCollider.Shape == JollyColliderShape.Circle)
            {
                return TryGetCircleRectangleCollision(movingObject, staticObject, out normal, out penetration);
            }

            if (staticCollider.Shape == JollyColliderShape.Circle)
            {
                return TryGetRectangleCircleCollision(movingObject, staticObject, out normal, out penetration);
            }

            return TryGetRectangleRectangleCollision(movingObject, staticObject, out normal, out penetration);
        }

        private static bool TryGetRectangleRectangleCollision(JollyGameObject movingObject, JollyGameObject staticObject, out JollyVector2 normal, out float penetration)
        {
            normal = new JollyVector2(0.0f, 0.0f);
            penetration = 0.0f;

            GetRectangleBounds(movingObject, out float movingLeft, out float movingTop, out float movingRight, out float movingBottom);
            GetRectangleBounds(staticObject, out float staticLeft, out float staticTop, out float staticRight, out float staticBottom);

            float overlapX = MathF.Min(movingRight, staticRight) - MathF.Max(movingLeft, staticLeft);
            float overlapY = MathF.Min(movingBottom, staticBottom) - MathF.Max(movingTop, staticTop);

            if (overlapX <= 0.0f || overlapY <= 0.0f)
            {
                return false;
            }

            float movingCenterX = (movingLeft + movingRight) * 0.5f;
            float movingCenterY = (movingTop + movingBottom) * 0.5f;
            float staticCenterX = (staticLeft + staticRight) * 0.5f;
            float staticCenterY = (staticTop + staticBottom) * 0.5f;

            if (overlapX < overlapY)
            {
                if (movingCenterX < staticCenterX)
                {
                    normal = new JollyVector2(-1.0f, 0.0f);
                }
                else
                {
                    normal = new JollyVector2(1.0f, 0.0f);
                }

                penetration = overlapX;
                return true;
            }

            if (movingCenterY < staticCenterY)
            {
                normal = new JollyVector2(0.0f, -1.0f);
            }
            else
            {
                normal = new JollyVector2(0.0f, 1.0f);
            }

            penetration = overlapY;

            return true;
        }

        private static bool TryGetCircleCircleCollision(JollyGameObject movingObject, JollyGameObject staticObject, out JollyVector2 normal, out float penetration)
        {
            normal = new JollyVector2(0.0f, 0.0f);
            penetration = 0.0f;

            float deltaX = movingObject.Position.X - staticObject.Position.X;
            float deltaY = movingObject.Position.Y - staticObject.Position.Y;
            float distanceSquared = deltaX * deltaX + deltaY * deltaY;

            float movingRadius = movingObject.Collider!.Radius * GetMaximumScale(movingObject);
            float staticRadius = staticObject.Collider!.Radius * GetMaximumScale(staticObject);
            float combinedRadius = movingRadius + staticRadius;

            if (distanceSquared >= combinedRadius * combinedRadius)
            {
                return false;
            }

            if (distanceSquared <= 0.000001f)
            {
                normal = new JollyVector2(0.0f, -1.0f);
                penetration = combinedRadius;
                return true;
            }

            float distance = MathF.Sqrt(distanceSquared);
            normal = new JollyVector2(deltaX / distance, deltaY / distance);
            penetration = combinedRadius - distance;

            return penetration > 0.0f;
        }

        private static bool TryGetTileCollision(JollyGameObject movingObject, float tileLeft, float tileTop, float tileRight, float tileBottom, out JollyVector2 normal, out float penetration)
        {
            normal = new JollyVector2(0.0f, 0.0f);
            penetration = 0.0f;

            if (movingObject.Collider!.Shape == JollyColliderShape.Circle)
            {
                return TryGetCircleTileCollision(movingObject, tileLeft, tileTop, tileRight, tileBottom, out normal, out penetration);
            }

            GetRectangleBounds(movingObject, out float movingLeft, out float movingTop, out float movingRight, out float movingBottom);

            float overlapX = MathF.Min(movingRight, tileRight) - MathF.Max(movingLeft, tileLeft);
            float overlapY = MathF.Min(movingBottom, tileBottom) - MathF.Max(movingTop, tileTop);

            if (overlapX <= 0.0f || overlapY <= 0.0f)
            {
                return false;
            }

            float movingCenterX = (movingLeft + movingRight) * 0.5f;
            float movingCenterY = (movingTop + movingBottom) * 0.5f;
            float tileCenterX = (tileLeft + tileRight) * 0.5f;
            float tileCenterY = (tileTop + tileBottom) * 0.5f;

            if (overlapX < overlapY)
            {
                normal = movingCenterX < tileCenterX ? new JollyVector2(-1.0f, 0.0f) : new JollyVector2(1.0f, 0.0f);
                penetration = overlapX;
                return true;
            }

            normal = movingCenterY < tileCenterY ? new JollyVector2(0.0f, -1.0f) : new JollyVector2(0.0f, 1.0f);
            penetration = overlapY;
            return true;
        }

        private static float GetMaximumScale(JollyGameObject gameObject)
        {
            return MathF.Max(MathF.Abs(gameObject.Scale.X), MathF.Abs(gameObject.Scale.Y));
        }

        private static bool TryGetCircleRectangleCollision(JollyGameObject movingObject, JollyGameObject staticObject, out JollyVector2 normal, out float penetration)
        {
            normal = new JollyVector2(0.0f, 0.0f);
            penetration = 0.0f;

            float circleX = movingObject.Position.X;
            float circleY = movingObject.Position.Y;
            float radius = movingObject.Collider!.Radius * GetMaximumScale(movingObject);

            GetRectangleBounds(staticObject, out float left, out float top, out float right, out float bottom);

            float closestX = System.Math.Clamp(circleX, left, right);
            float closestY = System.Math.Clamp(circleY, top, bottom);

            float deltaX = circleX - closestX;
            float deltaY = circleY - closestY;
            float distanceSquared = deltaX * deltaX + deltaY * deltaY;

            if (distanceSquared >= radius * radius)
            {
                return false;
            }

            if (distanceSquared > 0.000001f)
            {
                float distance = MathF.Sqrt(distanceSquared);

                normal = new JollyVector2(deltaX / distance, deltaY / distance);
                penetration = radius - distance;

                return penetration > 0.0f;
            }

            float distanceLeft = MathF.Abs(circleX - left);
            float distanceRight = MathF.Abs(right - circleX);
            float distanceTop = MathF.Abs(circleY - top);
            float distanceBottom = MathF.Abs(bottom - circleY);
            float minimum = MathF.Min(MathF.Min(distanceLeft, distanceRight), MathF.Min(distanceTop, distanceBottom));

            if (minimum == distanceLeft)
            {
                normal = new JollyVector2(-1.0f, 0.0f);
            }
            else if (minimum == distanceRight)
            {
                normal = new JollyVector2(1.0f, 0.0f);
            }
            else if (minimum == distanceTop)
            {
                normal = new JollyVector2(0.0f, -1.0f);
            }
            else
            {
                normal = new JollyVector2(0.0f, 1.0f);
            }

            penetration = radius + minimum;

            return true;
        }

        private static bool TryGetRectangleCircleCollision(JollyGameObject movingObject, JollyGameObject staticObject, out JollyVector2 normal, out float penetration)
        {
            bool result = TryGetCircleRectangleCollisionForReversedObjects(staticObject, movingObject, out JollyVector2 reversedNormal, out penetration);

            normal = new JollyVector2(-reversedNormal.X, -reversedNormal.Y);

            return result;
        }

        private static bool TryGetCircleRectangleCollisionForReversedObjects(JollyGameObject circleObject, JollyGameObject rectangleObject, out JollyVector2 normal, out float penetration)
        {
            return TryGetCircleRectangleCollision(circleObject, rectangleObject, out normal, out penetration);
        }

        private static bool TryGetCircleTileCollision(JollyGameObject movingObject, float tileLeft, float tileTop, float tileRight, float tileBottom, out JollyVector2 normal, out float penetration)
        {
            normal = new JollyVector2(0.0f, 0.0f);
            penetration = 0.0f;

            float circleX = movingObject.Position.X;
            float circleY = movingObject.Position.Y;
            float radius = movingObject.Collider!.Radius * GetMaximumScale(movingObject);

            float closestX = System.Math.Clamp(circleX, tileLeft, tileRight);
            float closestY = System.Math.Clamp(circleY, tileTop, tileBottom);

            float deltaX = circleX - closestX;
            float deltaY = circleY - closestY;
            float distanceSquared = deltaX * deltaX + deltaY * deltaY;

            if (distanceSquared >= radius * radius)
            {
                return false;
            }

            if (distanceSquared > 0.000001f)
            {
                float distance = MathF.Sqrt(distanceSquared);
                normal = new JollyVector2(deltaX / distance, deltaY / distance);
                penetration = radius - distance;
                return penetration > 0.0f;
            }

            float distanceLeft = MathF.Abs(circleX - tileLeft);
            float distanceRight = MathF.Abs(tileRight - circleX);
            float distanceTop = MathF.Abs(circleY - tileTop);
            float distanceBottom = MathF.Abs(tileBottom - circleY);

            float minimum = MathF.Min(MathF.Min(distanceLeft, distanceRight), MathF.Min(distanceTop, distanceBottom));

            if (minimum == distanceLeft)
            {
                normal = new JollyVector2(-1.0f, 0.0f);
            }
            else if (minimum == distanceRight)
            {
                normal = new JollyVector2(1.0f, 0.0f);
            }
            else if (minimum == distanceTop)
            {
                normal = new JollyVector2(0.0f, -1.0f);
            }
            else
            {
                normal = new JollyVector2(0.0f, 1.0f);
            }

            penetration = radius + minimum;

            return true;
        }

        private static void GetRectangleBounds(JollyGameObject gameObject, out float left, out float top, out float right, out float bottom)
        {
            JollyCollider collider = gameObject.Collider!;

            float halfWidth = collider.Width * MathF.Abs(gameObject.Scale.X) * 0.5f;
            float halfHeight = collider.Height * MathF.Abs(gameObject.Scale.Y) * 0.5f;

            float radians = gameObject.Rotation;
            float cos = MathF.Abs(MathF.Cos(radians));
            float sin = MathF.Abs(MathF.Sin(radians));

            float worldHalfWidth = halfWidth * cos + halfHeight * sin;
            float worldHalfHeight = halfWidth * sin + halfHeight * cos;

            left = gameObject.Position.X - worldHalfWidth;
            top = gameObject.Position.Y - worldHalfHeight;
            right = gameObject.Position.X + worldHalfWidth;
            bottom = gameObject.Position.Y + worldHalfHeight;
        }

        private static void GetCollisionBounds(JollyGameObject gameObject, out float left, out float top, out float right, out float bottom)
        {
            if (gameObject.Collider!.Shape == JollyColliderShape.Circle)
            {
                float radius = gameObject.Collider.Radius * GetMaximumScale(gameObject);

                left = gameObject.Position.X - radius;
                top = gameObject.Position.Y - radius;
                right = gameObject.Position.X + radius;
                bottom = gameObject.Position.Y + radius;

                return;
            }

            GetRectangleBounds(gameObject, out left, out top, out right, out bottom);
        }

        private static void ResolvePosition(JollyGameObject movingObject, JollyVector2 normal, float penetration)
        {
            const float separationEpsilon = 0.001f;

            float correction = penetration + separationEpsilon;

            movingObject.Position = new JollyVector2(movingObject.Position.X + normal.X * correction, movingObject.Position.Y + normal.Y * correction);
        }

        private static void ResolveVelocity(JollyGameObject movingObject, JollyVector2 normal)
        {
            float velocityAlongNormal = movingObject.Velocity.X * normal.X + movingObject.Velocity.Y * normal.Y;

            if (velocityAlongNormal >= 0.0f)
            {
                return;
            }

            if (normal.Y < -0.5f)
            {
                movingObject.Velocity = new JollyVector2(movingObject.Velocity.X, 0.0f);
                return;
            }

            float restitution = System.Math.Clamp(movingObject.Restitution, 0.0f, 1.0f);
            float normalVelocityX = normal.X * velocityAlongNormal;
            float normalVelocityY = normal.Y * velocityAlongNormal;
            float tangentVelocityX = movingObject.Velocity.X - normalVelocityX;
            float tangentVelocityY = movingObject.Velocity.Y - normalVelocityY;
            float bounceVelocityX = -normalVelocityX * restitution;
            float bounceVelocityY = -normalVelocityY * restitution;

            movingObject.Velocity = new JollyVector2(tangentVelocityX + bounceVelocityX, tangentVelocityY + bounceVelocityY);
        }

        private static void ResolveFriction(JollyGameObject movingObject, JollyVector2 normal, float deltaTime)
        {
            float friction = normal.Y < -0.5f ? movingObject.GroundFriction : movingObject.AirFriction;

            if (friction <= 0.0f || deltaTime <= 0.0f)
            {
                return;
            }

            float normalVelocity = movingObject.Velocity.X * normal.X + movingObject.Velocity.Y * normal.Y;

            float tangentX = movingObject.Velocity.X - normal.X * normalVelocity;
            float tangentY = movingObject.Velocity.Y - normal.Y * normalVelocity;

            float amount = 1.0f - MathF.Exp(-friction * deltaTime);

            tangentX *= 1.0f - amount;
            tangentY *= 1.0f - amount;

            movingObject.Velocity = new JollyVector2(tangentX + normal.X * normalVelocity, tangentY + normal.Y * normalVelocity);
        }

        private static void UpdateGroundedState(JollyGameObject movingObject, JollyVector2 normal)
        {
            if (normal.Y < -0.5f)
            {
                movingObject.IsGrounded = true;

                if (movingObject.Velocity.Y > 0.0f)
                {
                    movingObject.Velocity = new JollyVector2(movingObject.Velocity.X, 0.0f);
                }
            }
        }
    }
}