using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using System.Numerics;

namespace Jolly2DBeamRider.Game
{
    public sealed class BeamRiderSaucer : BeamRiderEnemy
    {
        private const float BaseRadiusX = 28.0f;
        private const float BaseRadiusY = 10.0f;
        private const float BaseBodyThickness = 2.0f;
        private const float MovementAmplitude = 0.22f;
        private const float MovementSpeed = 2.2f;
        private const int BodySegments = 24;
        private const int DomeSegments = 12;

        private readonly float _movementOffset;
        private readonly JollyColor _bodyColor;
        private readonly JollyColor _detailColor;

        private float _movementTime;

        public BeamRiderSaucer(BeamRiderArena arena, float lane, float depth, float speed = 0.22f) : base(arena, BeamRiderEnemyType.Saucer, lane, depth, speed, 24.0f, BeamRiderLaserBehaviour.Destroy, BeamRiderTorpedoBehaviour.Destroy)
        {
            _movementOffset = lane * 0.73f;
            _bodyColor = JollyColor.FromBytes(255, 255, 255);
            _detailColor = JollyColor.FromBytes(150, 220, 255);
            _movementTime = 0.0f;
        }

        public override void Update(float deltaTime)
        {
            if (!Alive)
            {
                return;
            }

            base.Update(deltaTime);

            if (!Alive)
            {
                return;
            }

            _movementTime += deltaTime * MovementSpeed;
        }

        public override IReadOnlyList<JollyLineSnapshot> GetLines()
        {
            if (!Alive)
            {
                return Array.Empty<JollyLineSnapshot>();
            }

            List<JollyLineSnapshot> lines = [];

            Vector2 centre = Position;
            float scale = GetPerspectiveScale();

            float movement = MathF.Sin(_movementTime + _movementOffset) * MovementAmplitude;
            centre.X += movement * 70.0f * scale;

            float radiusX = BaseRadiusX * scale;
            float radiusY = BaseRadiusY * scale;
            AddEllipse(lines, centre, radiusX, radiusY, _bodyColor, BaseBodyThickness * MathF.Max(scale, 0.35f), BodySegments);

            float domeRadiusX = radiusX * 0.42f;
            float domeRadiusY = radiusY * 0.95f;
            Vector2 domeCentre = centre;
            domeCentre.Y -= radiusY * 0.15f;
            AddDome(lines, domeCentre, domeRadiusX, domeRadiusY, _detailColor, MathF.Max(scale, 0.35f) * 1.5f);

            float centreWidth = radiusX * 0.55f;
            float centreHeight = MathF.Max(radiusY * 0.25f, 1.0f);
            Vector2 centreLeft = new(centre.X - centreWidth, centre.Y + centreHeight);
            Vector2 centreRight = new(centre.X + centreWidth, centre.Y + centreHeight);
            lines.Add(new JollyLineSnapshot(centreLeft, centreRight, _bodyColor, MathF.Max(scale, 0.35f) * 1.5f, MathF.Max(scale, 0.35f) * 1.5f, MathF.Max(scale, 0.35f) * 1.5f));

            return lines;
        }

        protected override void OnDestroyed()
        {
            _movementTime = 0.0f;
        }

        private static void AddEllipse(List<JollyLineSnapshot> lines, Vector2 centre, float radiusX, float radiusY, JollyColor color, float thickness, int segments)
        {
            float angleStep = MathF.Tau / segments;

            for (int index = 0; index < segments; index++)
            {
                float angle1 = index * angleStep;
                float angle2 = (index + 1) * angleStep;
                Vector2 start = new(centre.X + MathF.Cos(angle1) * radiusX, centre.Y + MathF.Sin(angle1) * radiusY);
                Vector2 end = new(centre.X + MathF.Cos(angle2) * radiusX, centre.Y + MathF.Sin(angle2) * radiusY);
                lines.Add(new JollyLineSnapshot(start, end, color, thickness, thickness, thickness));
            }
        }

        private static void AddDome(List<JollyLineSnapshot> lines, Vector2 centre, float radiusX, float radiusY, JollyColor color, float thickness)
        {
            const float startAngle = MathF.PI;
            const float endAngle = MathF.Tau;
            const int segments = DomeSegments;

            float angleStep = (endAngle - startAngle) / segments;

            for (int index = 0; index < segments; index++)
            {
                float angle1 = startAngle + (index * angleStep);
                float angle2 = startAngle + ((index + 1) * angleStep);
                Vector2 start = new(centre.X + MathF.Cos(angle1) * radiusX, centre.Y + MathF.Sin(angle1) * radiusY);
                Vector2 end = new(centre.X + MathF.Cos(angle2) * radiusX, centre.Y + MathF.Sin(angle2) * radiusY);
                lines.Add(new JollyLineSnapshot(start, end, color, thickness, thickness, thickness));
            }
        }
    }
}

