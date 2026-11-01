using System.Diagnostics;
using System.Numerics;

namespace Jolly2DEngine.Particles
{
    public sealed class JollyParticleSystem
    {
        private readonly List<JollyParticle> _particles = new();
        private readonly List<JollyParticleEmitter> _emitters = new();

        public float Drag { get; internal set; }

        public IReadOnlyList<JollyParticle> Particles => _particles;
        public IReadOnlyList<JollyParticleEmitter> Emitters => _emitters;

        public int ParticleCount => _particles.Count;

        public void AddEmitter(JollyParticleEmitter emitter)
        {
            ArgumentNullException.ThrowIfNull(emitter);

            if (!_emitters.Contains(emitter))
            {
                _emitters.Add(emitter);
            }
        }

        public void RemoveEmitter(JollyParticleEmitter emitter)
        {
            ArgumentNullException.ThrowIfNull(emitter);
            _emitters.Remove(emitter);
        }

        public void Update(float deltaSeconds)
        {
            if (deltaSeconds <= 0.0f)
            {
                return;
            }

            foreach (JollyParticleEmitter emitter in _emitters)
            {
                emitter.Update(deltaSeconds, _particles);
            }

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                JollyParticle particle = _particles[i];

                particle.Velocity += particle.Acceleration * deltaSeconds;

                if (particle.Drag > 0.0f)
                {
                    float drag = MathF.Max(0.0f, 1.0f - particle.Drag * deltaSeconds);
                    particle.Velocity *= drag;
                }

                particle.Position += particle.Velocity * deltaSeconds;
                particle.Rotation += particle.RotationSpeed * deltaSeconds;
                particle.Life -= deltaSeconds;

                float sizeProgress = 1.0f - particle.NormalizedLife;
                particle.Size = particle.StartSize + (particle.EndSize - particle.StartSize) * sizeProgress;

                if (particle.Life <= 0.0f)
                {
                    _particles.RemoveAt(i);
                }
            }
        }

        public void Clear()
        {
            _particles.Clear();
        }

        public void Emit(JollyParticleEmitter emitter, int count)
        {
            ArgumentNullException.ThrowIfNull(emitter);

            if (count <= 0)
            {
                return;
            }

            emitter.Emit(count, _particles);
        }
    }
}