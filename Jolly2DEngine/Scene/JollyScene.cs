using Jolly2DEngine.Core;
using Jolly2DEngine.Math;
using Jolly2DEngine.Physics;
using Jolly2DEngine.World;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Jolly2DEngine.Graphics.Scene
{
    public abstract class JollyScene
    {
        private readonly List<JollyGameObject> _objects = new();
        private readonly List<JollyGameObject> _pendingAdds = new();
        private readonly List<JollyGameObject> _pendingRemoves = new();
        private bool _isUpdating;
        private readonly HashSet<CollisionPair> _activeCollisions = new();
        private readonly HashSet<CollisionPair> _activeTriggers = new();
        private bool _initialized;
        private JollyTileMap? _tileMap;
        private float _tileMapX;
        private float _tileMapY;

        public IReadOnlyList<JollyGameObject> Objects => _objects;
        public int EntityCount => _objects.Count;
        public string Name { get; set; } = string.Empty;
        public bool IsInitialized => _initialized;
        public JollyTileMap? TileMap => _tileMap;

        public void SetTileMap(JollyTileMap tileMap, float x, float y)
        {
            ArgumentNullException.ThrowIfNull(tileMap);

            _tileMap = tileMap;
            _tileMapX = x;
            _tileMapY = y;
        }

        public void ClearTileMap()
        {
            _tileMap = null;
            _tileMapX = 0.0f;
            _tileMapY = 0.0f;
        }

        public virtual void OnEnter()
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            foreach (JollyGameObject gameObject in _objects)
            {
                gameObject.Initialize();
            }
        }

        public void Add(JollyGameObject gameObject)
        {
            ArgumentNullException.ThrowIfNull(gameObject);

            if (_objects.Contains(gameObject) || _pendingAdds.Contains(gameObject))
            {
                return;
            }

            if (_isUpdating)
            {
                _pendingRemoves.Remove(gameObject);
                _pendingAdds.Add(gameObject);
                return;
            }

            AddImmediate(gameObject);
        }

        public void Spawn(JollyGameObject gameObject)
        {
            Add(gameObject);
        }

        public void Remove(JollyGameObject gameObject)
        {
            ArgumentNullException.ThrowIfNull(gameObject);

            if (_pendingAdds.Remove(gameObject))
            {
                gameObject.Scene = null;
                return;
            }

            if (!_objects.Contains(gameObject))
            {
                return;
            }

            if (_isUpdating)
            {
                if (!_pendingRemoves.Contains(gameObject))
                {
                    _pendingRemoves.Add(gameObject);
                }

                return;
            }

            RemoveImmediate(gameObject);
        }

        public void Despawn(JollyGameObject gameObject)
        {
            Remove(gameObject);
        }

        private void AddImmediate(JollyGameObject gameObject)
        {
            if (_objects.Contains(gameObject))
            {
                return;
            }

            _objects.Add(gameObject);
            gameObject.Scene = this;

            if (_initialized)
            {
                gameObject.Initialize();
            }
        }

        private void RemoveImmediate(JollyGameObject gameObject)
        {
            if (!_objects.Remove(gameObject))
            {
                return;
            }

            RemoveCollisionPairs(gameObject);
            gameObject.Scene = null;

            if (_initialized)
            {
                gameObject.Shutdown();
            }
        }

        private void ApplyPendingChanges()
        {
            if (_pendingRemoves.Count > 0)
            {
                foreach (JollyGameObject gameObject in _pendingRemoves)
                {
                    RemoveImmediate(gameObject);
                }

                _pendingRemoves.Clear();
            }

            if (_pendingAdds.Count > 0)
            {
                foreach (JollyGameObject gameObject in _pendingAdds)
                {
                    if (!gameObject.IsDestroyed)
                    {
                        AddImmediate(gameObject);
                    }
                }

                _pendingAdds.Clear();
            }
        }

        public virtual void Update(JollyGameTime gameTime)
        {
            _isUpdating = true;

            foreach (JollyGameObject gameObject in _objects)
            {
                if (!gameObject.Active || !gameObject.Visible || gameObject.IsDestroyed)
                {
                    continue;
                }

                gameObject.Update(gameTime);
            }

            foreach (JollyGameObject gameObject in _objects)
            {
                if (!gameObject.Active || gameObject.IsDestroyed)
                {
                    continue;
                }

                JollyPhysicsSystem.Update(gameObject, gameTime);
            }

            ResolveTileMapCollisions((float)gameTime.DeltaSeconds);
            UpdateCollisions(gameTime);

            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                JollyGameObject gameObject = _objects[i];

                if (!gameObject.IsDestroyed)
                {
                    continue;
                }

                if (!_pendingRemoves.Contains(gameObject))
                {
                    _pendingRemoves.Add(gameObject);
                }
            }

            _isUpdating = false;
            ApplyPendingChanges();
        }

        public virtual void Draw()
        {
            foreach (JollyGameObject gameObject in _objects)
            {
                if (gameObject.Active && gameObject.Visible && !gameObject.IsDestroyed)
                {
                    gameObject.Draw();
                }
            }
        }

        public IReadOnlyList<JollySprite> GetSprites()
        {
            List<JollySprite> sprites = new();

            foreach (JollyGameObject gameObject in _objects)
            {
                if (!gameObject.Active || gameObject.IsDestroyed)
                {
                    continue;
                }

                if (gameObject is JollySpriteObject spriteObject)
                {
                    if (spriteObject.Sprite.Visible)
                    {
                        sprites.Add(spriteObject.Sprite);
                    }
                }
            }

            return sprites;
        }

        public JollyGameObject? FindBySaveId(string saveId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(saveId);

            foreach (JollyGameObject gameObject in _objects)
            {
                if (string.Equals(gameObject.SaveId, saveId, StringComparison.Ordinal))
                {
                    return gameObject;
                }
            }

            return null;
        }

        public JollyGameObject? Find(string name)
        {
            ArgumentNullException.ThrowIfNull(name);

            foreach (JollyGameObject gameObject in _objects)
            {
                if (string.Equals(gameObject.Name, name, StringComparison.Ordinal))
                {
                    return gameObject;
                }
            }

            return null;
        }

        public T? Find<T>(string name) where T : JollyGameObject
        {
            return Find(name) as T;
        }

        public IReadOnlyList<T> FindAll<T>() where T : JollyGameObject
        {
            List<T> result = new();

            foreach (JollyGameObject gameObject in _objects)
            {
                if (gameObject is T typedObject)
                {
                    result.Add(typedObject);
                }
            }

            return result;
        }

        public bool Contains(JollyGameObject gameObject)
        {
            ArgumentNullException.ThrowIfNull(gameObject);
            return _objects.Contains(gameObject);
        }

        public virtual void Shutdown()
        {
            foreach (CollisionPair pair in _activeCollisions)
            {
                pair.ObjectA.OnCollisionExit(pair.ObjectB);
                pair.ObjectB.OnCollisionExit(pair.ObjectA);
            }

            _activeCollisions.Clear();

            if (_initialized)
            {
                foreach (JollyGameObject gameObject in _objects)
                {
                    gameObject.Shutdown();
                    gameObject.Scene = null;
                }
            }
            else
            {
                foreach (JollyGameObject gameObject in _objects)
                {
                    gameObject.Scene = null;
                }
            }

            _objects.Clear();
            _pendingAdds.Clear();
            _pendingRemoves.Clear();
            _isUpdating = false;
            _initialized = false;
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
                    if (!tileMap.IsSolid(tileX, tileY))
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
                    }
                }
            }
        }

        private void ResolveTileMapCollisions(float deltaTime)
        {
            if (_tileMap == null)
            {
                return;
            }

            foreach (JollyGameObject gameObject in _objects)
            {
                if (!gameObject.Active || !gameObject.Visible || gameObject.IsDestroyed)
                {
                    continue;
                }

                if (!gameObject.UsePhysics)
                {
                    continue;
                }

                JollyPhysicsSystem.ResolveTileMap(gameObject, _tileMap, _tileMapX, _tileMapY, deltaTime);
            }
        }

        private static bool TryGetTileCollision(JollyGameObject movingObject, float tileLeft, float tileTop, float tileRight, float tileBottom, out JollyVector2 normal, out float penetration)
        {
            normal = new JollyVector2(0.0f, 0.0f);
            penetration = 0.0f;

            if (movingObject.Collider!.Shape == JollyColliderShape.Circle)
            {
                return TryGetCircleTileCollision(movingObject, tileLeft, tileTop, tileRight, tileBottom, out normal, out penetration);
            }

            GetCollisionBounds(movingObject, out float movingLeft, out float movingTop, out float movingRight, out float movingBottom);

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

        private static bool TryGetCircleTileCollision(JollyGameObject movingObject, float tileLeft, float tileTop, float tileRight, float tileBottom, out JollyVector2 normal, out float penetration)
        {
            normal = new JollyVector2(0.0f, 0.0f);
            penetration = 0.0f;

            float circleX = movingObject.Position.X;
            float circleY = movingObject.Position.Y;
            float scaleX = MathF.Abs(movingObject.Scale.X);
            float scaleY = MathF.Abs(movingObject.Scale.Y);
            float scale = MathF.Max(scaleX, scaleY);
            float radius = movingObject.Collider!.Radius * scale;
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

        private static void GetCollisionBounds(JollyGameObject gameObject, out float left, out float top, out float right, out float bottom)
        {
            JollyCollider collider = gameObject.Collider!;

            if (collider.Shape == JollyColliderShape.Circle)
            {
                float scaleX = MathF.Abs(gameObject.Scale.X);
                float scaleY = MathF.Abs(gameObject.Scale.Y);
                float scale = MathF.Max(scaleX, scaleY);
                float radius = collider.Radius * scale;

                left = gameObject.Position.X - radius;
                top = gameObject.Position.Y - radius;
                right = gameObject.Position.X + radius;
                bottom = gameObject.Position.Y + radius;

                return;
            }

            float width = collider.Width * MathF.Abs(gameObject.Scale.X);
            float height = collider.Height * MathF.Abs(gameObject.Scale.Y);

            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            float rotation = gameObject.Rotation;
            float cosine = MathF.Abs(MathF.Cos(rotation));
            float sine = MathF.Abs(MathF.Sin(rotation));

            float rotatedHalfWidth = halfWidth * cosine + halfHeight * sine;
            float rotatedHalfHeight = halfWidth * sine + halfHeight * cosine;

            left = gameObject.Position.X - rotatedHalfWidth;
            top = gameObject.Position.Y - rotatedHalfHeight;
            right = gameObject.Position.X + rotatedHalfWidth;
            bottom = gameObject.Position.Y + rotatedHalfHeight;
        }

        private void UpdateCollisions(JollyGameTime gameTime)
        {
            HashSet<CollisionPair> currentCollisions = new();
            HashSet<CollisionPair> currentTriggers = new();

            for (int i = 0; i < _objects.Count; i++)
            {
                JollyGameObject objectA = _objects[i];

                if (!objectA.Active || objectA.IsDestroyed || objectA.Collider == null || !objectA.Collider.Enabled)
                {
                    continue;
                }

                for (int j = i + 1; j < _objects.Count; j++)
                {
                    JollyGameObject objectB = _objects[j];

                    if (!objectB.Active || objectB.IsDestroyed || objectB.Collider == null || !objectB.Collider.Enabled)
                    {
                        continue;
                    }

                    if (!objectA.Collider.CanCollideWith(objectB.Collider))
                    {
                        continue;
                    }

                    if (!objectA.Collider.Check(objectB.Collider))
                    {
                        continue;
                    }

                    CollisionPair pair = new CollisionPair(objectA, objectB);

                    if (objectA.Collider.IsTrigger || objectB.Collider.IsTrigger)
                    {
                        currentTriggers.Add(pair);

                        if (_activeTriggers.Contains(pair))
                        {
                            JollyCollisionDebug.TriggerStay(objectA, objectB);
                            objectA.OnTriggerStay(objectB);
                            objectB.OnTriggerStay(objectA);
                        }
                        else
                        {
                            JollyCollisionDebug.TriggerEnter(objectA, objectB);
                            objectA.OnTriggerEnter(objectB);
                            objectB.OnTriggerEnter(objectA);
                        }

                        continue;
                    }

                    currentCollisions.Add(pair);

                    JollyPhysicsSystem.Resolve(objectA, objectB, (float)gameTime.DeltaSeconds);

                    if (_activeCollisions.Contains(pair))
                    {
                        JollyCollisionDebug.CollisionStay(objectA, objectB);
                        objectA.OnCollisionStay(objectB);
                        objectB.OnCollisionStay(objectA);
                    }
                    else
                    {
                        JollyCollisionDebug.CollisionEnter(objectA, objectB);
                        objectA.OnCollisionEnter(objectB);
                        objectB.OnCollisionEnter(objectA);
                    }
                }
            }

            ProcessCollisionExits(currentCollisions);
            ProcessTriggerExits(currentTriggers);

            _activeCollisions.Clear();
            _activeTriggers.Clear();

            foreach (CollisionPair pair in currentCollisions)
            {
                _activeCollisions.Add(pair);
            }

            foreach (CollisionPair pair in currentTriggers)
            {
                _activeTriggers.Add(pair);
            }
        }

        private void ProcessCollisionExits(HashSet<CollisionPair> currentCollisions)
        {
            foreach (CollisionPair pair in _activeCollisions)
            {
                if (currentCollisions.Contains(pair))
                {
                    continue;
                }

                JollyCollisionDebug.CollisionExit(pair.ObjectA, pair.ObjectB);
                pair.ObjectA.OnCollisionExit(pair.ObjectB);
                pair.ObjectB.OnCollisionExit(pair.ObjectA);
            }
        }

        private void ProcessTriggerExits(HashSet<CollisionPair> currentTriggers)
        {
            List<CollisionPair> exitedTriggers = new();

            foreach (CollisionPair pair in _activeTriggers)
            {
                if (!currentTriggers.Contains(pair))
                {
                    exitedTriggers.Add(pair);
                }
            }

            foreach (CollisionPair pair in exitedTriggers)
            {
                JollyGameObject objectA = pair.ObjectA;
                JollyGameObject objectB = pair.ObjectB;

                JollyCollisionDebug.TriggerExit(objectA, objectB);
                objectA.OnTriggerExit(objectB);
                objectB.OnTriggerExit(objectA);
            }
        }

        private void RemoveCollisionPairs(JollyGameObject gameObject)
        {
            List<CollisionPair> pairsToRemove = new();

            foreach (CollisionPair pair in _activeCollisions)
            {
                if (ReferenceEquals(pair.ObjectA, gameObject) || ReferenceEquals(pair.ObjectB, gameObject))
                {
                    pairsToRemove.Add(pair);
                }
            }

            foreach (CollisionPair pair in pairsToRemove)
            {
                JollyGameObject other = ReferenceEquals(pair.ObjectA, gameObject) ? pair.ObjectB : pair.ObjectA;

                gameObject.OnCollisionExit(other);
                other.OnCollisionExit(gameObject);

                _activeCollisions.Remove(pair);
            }
        }

        private readonly struct CollisionPair : IEquatable<CollisionPair>
        {
            public JollyGameObject ObjectA { get; }

            public JollyGameObject ObjectB { get; }

            public CollisionPair(JollyGameObject objectA, JollyGameObject objectB)
            {
                if (RuntimeHelpers.GetHashCode(objectA) <= RuntimeHelpers.GetHashCode(objectB))
                {
                    ObjectA = objectA;
                    ObjectB = objectB;
                }
                else
                {
                    ObjectA = objectB;
                    ObjectB = objectA;
                }
            }

            public bool Equals(CollisionPair other)
            {
                return ReferenceEquals(ObjectA, other.ObjectA) && ReferenceEquals(ObjectB, other.ObjectB);
            }

            public override bool Equals(object? obj)
            {
                return obj is CollisionPair other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(RuntimeHelpers.GetHashCode(ObjectA), RuntimeHelpers.GetHashCode(ObjectB));
            }
        }
    }
}