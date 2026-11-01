using Jolly2DEngine.Math;
using System.Numerics;

namespace Jolly2DEngine.Particles
{
    public sealed class JollyParticleEmitter
    {
        private readonly Random _random = new();
        private float _emissionTime;

        public Vector2 Position { get; set; }
        public bool Enabled { get; set; } = true;
        public float ParticlesPerSecond { get; set; } = 20.0f;
        public float MinimumLife { get; set; } = 0.5f;
        public float MaximumLife { get; set; } = 1.0f;
        public float MinimumSize { get; set; } = 4.0f;
        public float MaximumSize { get; set; } = 8.0f;
        public float MinimumSpeed { get; set; } = 20.0f;
        public float MaximumSpeed { get; set; } = 60.0f;
        public float Direction { get; set; }
        public float Spread { get; set; } = MathF.PI * 2.0f;
        public Vector2 Gravity { get; set; }
        public float Drag { get; set; }
        public float MinimumRotationSpeed { get; set; }
        public float MaximumRotationSpeed { get; set; }
        public float Opacity { get; set; } = 1.0f;
        public JollyColor Color { get; internal set; } = JollyColor.White;
        public int Layer { get; set; }
        public JollyParticleEmitterShape Shape { get; set; } = JollyParticleEmitterShape.Point;
        public float ShapeWidth { get; set; } = 32.0f;
        public float ShapeHeight { get; set; } = 32.0f;
        public float ShapeRadius { get; set; } = 16.0f;

        public int Update(float deltaSeconds, List<JollyParticle> particles)
        {
            ArgumentNullException.ThrowIfNull(particles);

            if (!Enabled || ParticlesPerSecond <= 0.0f || deltaSeconds <= 0.0f)
            {
                return 0;
            }

            _emissionTime += deltaSeconds;

            float interval = 1.0f / ParticlesPerSecond;
            int emitted = 0;

            while (_emissionTime >= interval)
            {
                _emissionTime -= interval;
                particles.Add(CreateParticle());
                emitted++;

                if (emitted >= 100)
                {
                    _emissionTime = 0.0f;
                    break;
                }
            }

            return emitted;
        }

        public void Emit(int count, List<JollyParticle> particles)
        {
            ArgumentNullException.ThrowIfNull(particles);

            if (count <= 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                particles.Add(CreateParticle());
            }
        }

        public void Reset()
        {
            _emissionTime = 0.0f;
        }

        private JollyParticle CreateParticle()
        {
            float life = RandomRange(MinimumLife, MaximumLife);
            float size = RandomRange(MinimumSize, MaximumSize);
            float speed = RandomRange(MinimumSpeed, MaximumSpeed);
            float angle = Direction + RandomRange(-Spread * 0.5f, Spread * 0.5f);
            float rotationSpeed = RandomRange(MinimumRotationSpeed, MaximumRotationSpeed);

            Vector2 position = GetSpawnPosition();

            return new JollyParticle
            {
                Position = position,
                Velocity = new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
                Acceleration = Gravity,
                Drag = MathF.Max(0.0f, Drag),
                Life = life,
                MaximumLife = life,
                Size = size,
                StartSize = size,
                EndSize = 0.0f,
                Rotation = 0.0f,
                RotationSpeed = rotationSpeed,
                Opacity = System.Math.Clamp(Opacity, 0.0f, 1.0f),
                Color = Color,
                Layer = Layer
            };
        }

        private Vector2 GetSpawnPosition()
        {
            switch (Shape)
            {
                case JollyParticleEmitterShape.Point:
                    return Position;

                case JollyParticleEmitterShape.Circle:
                    float angle = RandomRange(0.0f, MathF.PI * 2.0f);
                    float radius = MathF.Sqrt((float)_random.NextDouble()) * MathF.Max(0.0f, ShapeRadius);
                    return Position + new Vector2(MathF.Cos(angle) * radius, MathF.Sin(angle) * radius);

                case JollyParticleEmitterShape.Rectangle:
                    float x = RandomRange(-ShapeWidth * 0.5f, ShapeWidth * 0.5f);
                    float y = RandomRange(-ShapeHeight * 0.5f, ShapeHeight * 0.5f);
                    return Position + new Vector2(x, y);

                case JollyParticleEmitterShape.Line:
                    float xx = RandomRange(-ShapeWidth * 0.5f, ShapeWidth * 0.5f);
                    return Position + new Vector2(xx, 0.0f);

                default:
                    return Position;
            }
        }

        private float RandomRange(float minimum, float maximum)
        {
            if (maximum <= minimum)
            {
                return minimum;
            }

            return minimum + (float)_random.NextDouble() * (maximum - minimum);
        }

        public void Burst(int count, List<JollyParticle> particles)
        {
            ArgumentNullException.ThrowIfNull(particles);

            if (count <= 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                particles.Add(CreateParticle());
            }
        }

        public void ApplyPreset(JollyParticlePreset preset)
        {
            switch (preset)
            {
                case JollyParticlePreset.Sparks:
                    ParticlesPerSecond = 40.0f;
                    MinimumLife = 0.25f;
                    MaximumLife = 0.6f;
                    MinimumSize = 3.0f;
                    MaximumSize = 6.0f;
                    MinimumSpeed = 100.0f;
                    MaximumSpeed = 260.0f;
                    Direction = -MathF.PI * 0.5f;
                    Spread = MathF.PI * 0.5f;
                    Gravity = new System.Numerics.Vector2(0.0f, 500.0f);
                    Drag = 0.5f;
                    MinimumRotationSpeed = -8.0f;
                    MaximumRotationSpeed = 8.0f;
                    Opacity = 1.0f;
                    Color = JollyColor.FromBytes(255, 220, 80);
                    break;

                case JollyParticlePreset.Smoke:
                    ParticlesPerSecond = 12.0f;
                    MinimumLife = 1.5f;
                    MaximumLife = 3.0f;
                    MinimumSize = 8.0f;
                    MaximumSize = 16.0f;
                    MinimumSpeed = 20.0f;
                    MaximumSpeed = 50.0f;
                    Direction = -MathF.PI * 0.5f;
                    Spread = MathF.PI * 0.75f;
                    Gravity = new System.Numerics.Vector2(0.0f, -20.0f);
                    Drag = 1.5f;
                    MinimumRotationSpeed = -1.0f;
                    MaximumRotationSpeed = 1.0f;
                    Opacity = 0.5f;
                    Color = JollyColor.FromBytes(150, 150, 150, 220);
                    break;

                case JollyParticlePreset.Fire:
                    ParticlesPerSecond = 35.0f;
                    MinimumLife = 0.3f;
                    MaximumLife = 0.8f;
                    MinimumSize = 6.0f;
                    MaximumSize = 14.0f;
                    MinimumSpeed = 30.0f;
                    MaximumSpeed = 100.0f;
                    Direction = -MathF.PI * 0.5f;
                    Spread = MathF.PI * 0.6f;
                    Gravity = new System.Numerics.Vector2(0.0f, -100.0f);
                    Drag = 1.0f;
                    MinimumRotationSpeed = -3.0f;
                    MaximumRotationSpeed = 3.0f;
                    Opacity = 1.0f;
                    Color = JollyColor.FromBytes(255, 100, 20);
                    break;

                case JollyParticlePreset.Explosion:
                    ParticlesPerSecond = 0.0f;
                    MinimumLife = 0.25f;
                    MaximumLife = 0.7f;
                    MinimumSize = 4.0f;
                    MaximumSize = 12.0f;
                    MinimumSpeed = 100.0f;
                    MaximumSpeed = 350.0f;
                    Direction = 0.0f;
                    Spread = MathF.PI * 2.0f;
                    Gravity = new System.Numerics.Vector2(0.0f, 250.0f);
                    Drag = 1.0f;
                    MinimumRotationSpeed = -10.0f;
                    MaximumRotationSpeed = 10.0f;
                    Opacity = 1.0f;
                    Color = JollyColor.FromBytes(255, 70, 20);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(preset), preset, null);
            }

            Reset();
        }
    }
}