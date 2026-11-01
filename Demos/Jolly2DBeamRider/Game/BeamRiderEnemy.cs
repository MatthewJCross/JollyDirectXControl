using Jolly2DEngine.Graphics;
using System.Numerics;

namespace Jolly2DBeamRider.Game
{
    public enum BeamRiderEnemyType
    {
        Saucer,
        Debris,
        Chirper,
        Blocker,
        BounceCraft,
        Charger,
        Tracker,
        ZigBomb,
        Rejuvenator,
        Sentinel
    }

    public enum BeamRiderLaserBehaviour
    {
        None,
        Destroy,
        Deflect
    }

    public enum BeamRiderTorpedoBehaviour
    {
        Destroy,
        None
    }

    public abstract class BeamRiderEnemy : IDisposable
    {
        private readonly BeamRiderArena _arena;
        private readonly BeamRiderEnemyType _type;
        private readonly float _speed;
        private readonly float _collisionRadius;
        private readonly BeamRiderLaserBehaviour _laserBehaviour;
        private readonly BeamRiderTorpedoBehaviour _torpedoBehaviour;

        private float _lane;
        private float _depth;
        private bool _alive;
        private bool _disposed;

        protected BeamRiderEnemy(BeamRiderArena arena, BeamRiderEnemyType type, float lane, float depth, float speed, float collisionRadius, BeamRiderLaserBehaviour laserBehaviour, BeamRiderTorpedoBehaviour torpedoBehaviour)
        {
            ArgumentNullException.ThrowIfNull(arena);
            _arena = arena;
            _type = type;
            _lane = lane;
            _depth = depth;
            _speed = speed;
            _collisionRadius = collisionRadius;
            _laserBehaviour = laserBehaviour;
            _torpedoBehaviour = torpedoBehaviour;
            _alive = true;
        }

        public BeamRiderEnemyType Type => _type;

        public float Lane => _lane;

        public float Depth => _depth;

        public float Speed => _speed;

        public float CollisionRadius => _collisionRadius;

        public BeamRiderLaserBehaviour LaserBehaviour => _laserBehaviour;

        public BeamRiderTorpedoBehaviour TorpedoBehaviour => _torpedoBehaviour;

        public bool Alive => _alive;

        public bool IsAtPlayer => _depth >= 1.0f;

        protected BeamRiderArena Arena => _arena;

        protected Vector2 Position => GetPosition();

        public virtual void Update(float deltaTime)
        {
            if (_disposed || !_alive || deltaTime <= 0.0f)
            {
                return;
            }

            _depth += _speed * deltaTime;
            OnUpdate(deltaTime);
            if (_depth >= 1.15f)
            {
                OnReachedPlayer();
            }
        }

        protected virtual void OnUpdate(float deltaTime)
        {
        }

        protected virtual void OnReachedPlayer()
        {
            Respawn();
        }

        protected virtual void Respawn()
        {
            if (_disposed)
            {
                return;
            }

            _depth = -0.05f;
            _lane = Random.Shared.Next(-2, 3);
            _alive = true;
            OnRespawned();
        }

        protected virtual void OnRespawned()
        {
        }

        public virtual IReadOnlyList<JollyLineSnapshot> GetLines()
        {
            return [];
        }

        public bool ContainsPoint(Vector2 point)
        {
            if (!_alive)
            {
                return false;
            }

            Vector2 position = GetPosition();
            float distance = Vector2.Distance(position, point);
            return distance <= _collisionRadius;
        }

        public bool CheckCollision(Vector2 point, float radius)
        {
            if (!_alive)
            {
                return false;
            }

            Vector2 position = GetPosition();
            float distance = Vector2.Distance(position, point);
            return distance <= _collisionRadius + radius;
        }

        public bool TryHitByLaser()
        {
            if (!_alive)
            {
                return false;
            }

            switch (_laserBehaviour)
            {
                case BeamRiderLaserBehaviour.None:
                    return false;

                case BeamRiderLaserBehaviour.Destroy:
                    Destroy();
                    return true;

                case BeamRiderLaserBehaviour.Deflect:
                    OnLaserDeflected();
                    return true;

                default:
                    return false;
            }
        }

        public bool TryHitByTorpedo()
        {
            if (!_alive)
            {
                return false;
            }

            if (_torpedoBehaviour != BeamRiderTorpedoBehaviour.Destroy)
            {
                return false;
            }

            Destroy();
            return true;
        }

        public void Destroy()
        {
            if (!_alive)
            {
                return;
            }

            _alive = false;
            OnDestroyed();
        }

        protected virtual void OnLaserDeflected()
        {
        }

        protected virtual void OnDestroyed()
        {
        }

        protected virtual void SetAlive(bool alive)
        {
            _alive = alive;
        }

        protected virtual Vector2 GetPosition()
        {
            return _arena.GetBeamPosition(_lane, GetScreenYFromDepth());
        }

        protected float GetScreenYFromDepth()
        {
            const float horizonY = 200.0f;
            const float playerY = 790.0f;
            float depth = Math.Clamp(_depth, 0.0f, 1.15f);
            float perspective = MathF.Pow(depth, 1.35f);
            return horizonY + ((playerY - horizonY) * perspective);
        }

        protected float GetPerspectiveScale()
        {
            float depth = Math.Clamp(_depth, 0.0f, 1.0f);
            return 0.25f + (depth * 0.75f);
        }

        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _alive = false;
        }
    }
}
