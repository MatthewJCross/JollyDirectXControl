
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using System.Numerics;

namespace Jolly2DElite.Game.Graphics
{
    internal sealed class StarFieldRenderer
    {
        private const int NormalStardustCount = 20;

        private struct StardustParticle
        {
            public float X;
            public float Y;
            public float Z;
        }

        private readonly StardustParticle[] _particles = new StardustParticle[NormalStardustCount + 1];
        private readonly Random _random;
        private readonly JollyColor _starColor = new(190, 210, 220, 255);

        private float _previousPitch;
        private float _previousRoll;
        private float _previousYaw;
        private bool _hasPreviousOrientation;

        public StarFieldRenderer(int starCount = NormalStardustCount, float radius = 1400.0f, int seed = 41271)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(starCount);

            _random = new Random(seed);

            for (int index = 1; index <= NormalStardustCount; index++)
            {
                InitialiseInitialParticle(index);
            }
        }

        public void UpdateAndDraw(List<JollyTriangleSnapshot> triangles, PlayerShip playerShip, Matrix4x4 viewMatrix, Vector2 screenCenter, float viewportWidth, float viewportHeight, float deltaSeconds, float focalLength = 256.0f)
        {
            float yawDelta = 0.0f;
            float pitch = 0.0f;
            float roll = 0.0f;

            if (_hasPreviousOrientation)
            {
                yawDelta = MathF.IEEERemainder(playerShip.Yaw - _previousYaw, MathF.Tau);
                pitch = (playerShip.Pitch - _previousPitch) * 256.0f;
                float rollDelta = MathF.IEEERemainder(playerShip.Roll - _previousRoll, MathF.Tau);
                roll = rollDelta * 256.0f;
            }

            _previousYaw = playerShip.Yaw;
            _previousPitch = playerShip.Pitch;
            _previousRoll = playerShip.Roll;
            _hasPreviousOrientation = true;

            float speed = Math.Clamp(playerShip.Speed * 4.0f, 0.0f, 40.0f);

            float scaleX = viewportWidth / 256.0f;
            float scaleY = viewportHeight / 192.0f;

            for (int index = 1; index <= NormalStardustCount; index++)
            {
                StardustParticle particle = _particles[index];

                // Turning left or right moves the universe in the opposite direction.
                particle.X -= yawDelta * 256.0f;
                StepFront(ref particle, speed, pitch, roll);

                if (MathF.Abs(particle.X) >= 120.0f || MathF.Abs(particle.Y) >= 120.0f || particle.Z < 16.0f)
                {
                    InitialiseParticle(index);
                    particle = _particles[index];
                }
                else
                {
                    _particles[index] = particle;
                }

                if (particle.Z <= 0.0f)
                {
                    continue;
                }

                float screenX = screenCenter.X + particle.X * scaleX;
                float screenY = screenCenter.Y - particle.Y * scaleY;

                if (screenX < 1.0f || screenX >= viewportWidth - 1.0f || screenY < 1.0f || screenY >= viewportHeight - 1.0f)
                {
                    continue;
                }

                float starSize = particle.Z < 80.0f ? 1.5f : 1.0f;

                Vector2 top = new(screenX, screenY - starSize);
                Vector2 bottomLeft = new(screenX - starSize, screenY + starSize);
                Vector2 bottomRight = new(screenX + starSize, screenY + starSize);

                triangles.Add(new JollyTriangleSnapshot(top, bottomLeft, bottomRight, _starColor));
            }
        }

        private void StepFront(ref StardustParticle particle, float speed, float pitch, float roll)
        {
            float outwards = speed / (4.0f * MathF.Max(particle.Z, 1.0f));

            particle.Z -= speed / 4.0f;

            particle.Y += particle.Y * outwards;
            particle.X += particle.X * outwards;

            // Roll: the input values are expressed in Elite-style units.
            float rollTerm = roll / 256.0f;

            particle.Y -= rollTerm * particle.X;
            particle.X += rollTerm * particle.Y;

            // Pitch: move the particle vertically and slightly sideways.
            float pitchTerm = pitch * particle.Y / 256.0f;

            particle.X += 2.0f * pitchTerm * pitchTerm / 256.0f;
            particle.Y -= pitch;
        }

        private void InitialiseInitialParticle(int index)
        {
            _particles[index] = new StardustParticle
            {
                X = SignedByte(_random.Next(256)),
                Y = SignedByte(_random.Next(256)),
                Z = _random.Next(256) | 8
            };
        }

        private void InitialiseParticle(int index)
        {
            _particles[index] = new StardustParticle
            {
                X = SignedByte(_random.Next(256) | 8),
                Y = SignedByte(_random.Next(256) | 4),
                Z = _random.Next(256) | 144
            };
        }

        private static int SignedByte(int value)
        {
            return (value & 0x80) != 0 ? -(value & 0x7F) : value & 0x7F;
        }
    }
}
