using Jolly2DEngine.Math;

namespace Jolly2DEngine.Particles
{
    public sealed class JollyParticle
    {
        public System.Numerics.Vector2 Position { get; internal set; }
        public System.Numerics.Vector2 Velocity { get; internal set; }
        public System.Numerics.Vector2 Acceleration { get; internal set; }
        public float Drag { get; internal set; }
        public float Life { get; internal set; }
        public float MaximumLife { get; internal set; }
        public float Size { get; internal set; }
        public float StartSize { get; internal set; }
        public float EndSize { get; internal set; }
        public float Rotation { get; internal set; }
        public float RotationSpeed { get; internal set; }
        public float Opacity { get; internal set; }
        public JollyColor Color { get; internal set; } = JollyColor.White;
        public int Layer { get; internal set; }
        public bool IsAlive => Life > 0.0f;

        public float NormalizedLife
        {
            get
            {
                if (MaximumLife <= 0.0f)
                {
                    return 0.0f;
                }

                return System.Math.Clamp(Life / MaximumLife, 0.0f, 1.0f);
            }
        }

        public float Age
        {
            get
            {
                return 1.0f - NormalizedLife;
            }
        }

        internal JollyParticle()
        {
        }
    }
}