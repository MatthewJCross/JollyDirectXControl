using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using System.Numerics;

namespace Jolly2DBeamRider.Game
{
    public sealed class BeamRiderProjectile
    {
        private const float ProjectileSpeed = 620.0f;
        private const float HorizonY = 105.0f;
        private const float ArenaBottomY = 720.0f;

        private readonly BeamRiderArena _arena;
        private readonly float _lane;
        private float _y;
        private float _rotation;
        private bool _active;
        private readonly float _radiusX;
        private readonly float _radiusY;
        private readonly JollyColor _color;

        public BeamRiderProjectile(BeamRiderArena arena, float lane, float y, float rotation, float radiusX, float radiusY, JollyColor color)
        {
            ArgumentNullException.ThrowIfNull(arena);

            _arena = arena;
            _lane = lane;
            _y = y;
            _rotation = rotation;
            _active = true;
            _radiusX = radiusX;
            _radiusY = radiusY;
            _color = color;
        }

        public bool Active => _active;

        public Vector2 Position => _arena.GetBeamPosition(_lane, _y);

        public void Update(float deltaTime)
        {
            if (!_active || deltaTime <= 0.0f)
            {
                return;
            }

            _y -= ProjectileSpeed * deltaTime;
            _rotation += deltaTime * 20.0f;

            if (_y <= HorizonY)
            {
                _y = HorizonY;
                _active = false;
            }
        }

        public bool TryHitEnemy(BeamRiderEnemy enemy)
        {
            ArgumentNullException.ThrowIfNull(enemy);

            if (!_active || !enemy.Alive)
            {

                return false;
            }

            Vector2 projectilePosition = Position;
            float projectileRadius = GetCollisionRadius();

            if (!enemy.CheckCollision(projectilePosition, projectileRadius))
            {
                return false;
            }

            enemy.TryHitByLaser();
            _active = false;

            return true;
        }

        public void Deactivate()
        {
            _active = false;
        }

        public IReadOnlyList<JollyLineSnapshot> GetLines()
        {
            List<JollyLineSnapshot> lines = [];

            if (!_active)
            {
                return lines;
            }

            Vector2 centre = Position;

            const int segments = 20;
            const float arcSize = MathF.Tau * 0.92f;

            float angleStep = arcSize / segments;
            float startAngle = _rotation - (arcSize * 0.5f);

            float perspective = Math.Clamp((_y - HorizonY) / (ArenaBottomY - HorizonY), 0.0f, 1.0f);

            float scale = 0.25f + (perspective * 0.75f);

            float radiusX = _radiusX * scale;
            float radiusY = _radiusY * scale;

            for (int index = 0; index < segments; index++)
            {
                float angle1 = startAngle + (index * angleStep);
                float angle2 = startAngle + ((index + 1) * angleStep);

                Vector2 start = centre + new Vector2(MathF.Cos(angle1) * radiusX, MathF.Sin(angle1) * radiusY);
                Vector2 end = centre + new Vector2(MathF.Cos(angle2) * radiusX, MathF.Sin(angle2) * radiusY);

                lines.Add(new JollyLineSnapshot(start, end, _color, 1.25f, 2.0f, 0.8f));
            }

            return lines;
        }

        private float GetCollisionRadius()
        {
            float perspective = Math.Clamp((_y - HorizonY) / (ArenaBottomY - HorizonY), 0.0f, 1.0f);
            float scale = 0.25f + (perspective * 0.75f);
            float radius = MathF.Max(_radiusX * scale, _radiusY * scale);
            return radius;
        }
    }
}
