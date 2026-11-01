using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class PlayerShip
    {
        public Vector3 Position { get; private set; }
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public float Roll { get; private set; }
        public float Speed { get; private set; }
        public float MaximumSpeed { get; set; } = 10.0f;
        public float Energy { get; private set; } = 100.0f;
        public float ForwardShield { get; private set; } = 100.0f;
        public float AftShield { get; private set; } = 100.0f;


        public Vector3 Forward
        {
            get
            {
                float cosPitch = MathF.Cos(Pitch);
                return new Vector3(MathF.Sin(Yaw) * cosPitch, -MathF.Sin(Pitch), MathF.Cos(Yaw) * cosPitch);
            }
        }

        private Vector3 UnrolledRight
        {
            get
            {
                return new Vector3(MathF.Cos(Yaw), 0.0f, -MathF.Sin(Yaw));
            }
        }

        private Vector3 UnrolledUp
        {
            get
            {
                return Vector3.Normalize(Vector3.Cross(Forward, UnrolledRight));
            }
        }

        public Vector3 Right
        {
            get
            {
                float cosRoll = MathF.Cos(Roll);
                float sinRoll = MathF.Sin(Roll);

                return UnrolledRight * cosRoll + UnrolledUp * sinRoll;
            }
        }

        public Vector3 Up
        {
            get
            {
                float cosRoll = MathF.Cos(Roll);
                float sinRoll = MathF.Sin(Roll);

                return UnrolledUp * cosRoll - UnrolledRight * sinRoll;
            }
        }

        public void Reset()
        {
            Position = Vector3.Zero;
            Yaw = 0.0f;
            Pitch = 0.0f;
            Roll = 0.0f;
            Speed = 0.0f;
            Energy = 100.0f;
            ForwardShield = 100.0f;
            AftShield = 100.0f; 
        }

        public void Rotate(float yaw, float pitch, float roll, float deltaSeconds)
        {
            Yaw = WrapAngle(Yaw + yaw * deltaSeconds);
            Pitch = WrapAngle(Pitch + pitch * deltaSeconds);
            Roll = WrapAngle(Roll + roll * deltaSeconds);
        }

        public void SetInitialSpeed(float speed)
        {
            Speed = Math.Clamp(speed, 0.0f, MaximumSpeed);
        }

        public void AdjustSpeed(float throttle, float deltaSeconds)
        {
            Speed = Math.Clamp(Speed + throttle * deltaSeconds, 0.0f, MaximumSpeed);
        }

        public void Update(float deltaSeconds)
        {
            Position += Forward * Speed * deltaSeconds;
        }

        private static float WrapAngle(float angle)
        {
            return MathF.IEEERemainder(angle, MathF.Tau);
        }

        public void TakeDamage(float damage, bool hitFromFront)
        {
            if (damage <= 0.0f || Energy <= 0.0f)
            {
                return;
            }

            float shield = hitFromFront ? ForwardShield : AftShield;
            float shieldDamage = MathF.Min(shield, damage);

            if (hitFromFront)
            {
                ForwardShield -= shieldDamage;
            }
            else
            {
                AftShield -= shieldDamage;
            }

            damage -= shieldDamage;

            if (damage > 0.0f)
            {
                Energy = MathF.Max(0.0f, Energy - damage);
            }
        }
    }
}
