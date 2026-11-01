using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;

namespace Jolly2DEngine.Particles
{
    public sealed class JollyParticleRenderer
    {
        private readonly JollyParticleSystem _particleSystem;
        private readonly Func<JollyTexture, JollySprite> _spriteFactory;
        private readonly Func<JollyColor, JollyTexture> _textureFactory;
        private readonly List<JollySprite> _sprites = new();
        private readonly Dictionary<JollyColor, List<JollySprite>> _spritePools = new();
        private readonly Dictionary<JollyColor, JollyTexture> _textures = new();

        public IReadOnlyList<JollySprite> Sprites => _sprites;

        public JollyParticleRenderer(JollyParticleSystem particleSystem, Func<JollyTexture, JollySprite> spriteFactory, Func<JollyColor, JollyTexture> textureFactory)
        {
            ArgumentNullException.ThrowIfNull(particleSystem);
            ArgumentNullException.ThrowIfNull(spriteFactory);
            ArgumentNullException.ThrowIfNull(textureFactory);

            _particleSystem = particleSystem;
            _spriteFactory = spriteFactory;
            _textureFactory = textureFactory;
        }

        public void Update(JollyTexture texture)
        {
            ArgumentNullException.ThrowIfNull(texture);

            _sprites.Clear();

            Dictionary<JollyColor, int> requiredCounts = new();

            for (int i = 0; i < _particleSystem.ParticleCount; i++)
            {
                JollyParticle particle = _particleSystem.Particles[i];

                if (!particle.IsAlive)
                {
                    continue;
                }

                if (!requiredCounts.TryGetValue(particle.Color, out int count))
                {
                    count = 0;
                }

                requiredCounts[particle.Color] = count + 1;
            }

            foreach (KeyValuePair<JollyColor, int> entry in requiredCounts)
            {
                List<JollySprite> pool = GetPool(entry.Key);

                while (pool.Count < entry.Value)
                {
                    JollyTexture particleTexture = GetTexture(entry.Key, texture);
                    JollySprite sprite = _spriteFactory(particleTexture);
                    sprite.Visible = false;
                    pool.Add(sprite);
                }
            }

            Dictionary<JollyColor, int> poolIndices = new();

            for (int i = 0; i < _particleSystem.ParticleCount; i++)
            {
                JollyParticle particle = _particleSystem.Particles[i];

                if (!particle.IsAlive)
                {
                    continue;
                }

                if (!poolIndices.TryGetValue(particle.Color, out int poolIndex))
                {
                    poolIndex = 0;
                }

                List<JollySprite> pool = _spritePools[particle.Color];
                JollySprite sprite = pool[poolIndex];
                poolIndices[particle.Color] = poolIndex + 1;

                sprite.X = particle.Position.X;
                sprite.Y = particle.Position.Y;
                sprite.Width = particle.Size;
                sprite.Height = particle.Size;
                sprite.Rotation = particle.Rotation;
                sprite.Opacity = particle.Opacity * particle.NormalizedLife;
                sprite.Layer = particle.Layer;
                sprite.SourceX = 0.0f;
                sprite.SourceY = 0.0f;
                sprite.SourceWidth = 1.0f;
                sprite.SourceHeight = 1.0f;
                sprite.Visible = true;

                _sprites.Add(sprite);
            }

        }

        private List<JollySprite> GetPool(JollyColor color)
        {
            if (_spritePools.TryGetValue(color, out List<JollySprite>? pool))
            {
                return pool;
            }

            pool = new();
            _spritePools.Add(color, pool);
            return pool;
        }

        private JollyTexture GetTexture(JollyColor color, JollyTexture baseTexture)
        {
            if (color == JollyColor.White)
            {
                return baseTexture;
            }

            if (_textures.TryGetValue(color, out JollyTexture? texture))
            {
                return texture;
            }

            texture = _textureFactory(color);
            _textures.Add(color, texture);
            return texture;
        }

        public void Clear()
        {
            _sprites.Clear();

            foreach (List<JollySprite> pool in _spritePools.Values)
            {
                foreach (JollySprite sprite in pool)
                {
                    sprite.Visible = false;
                }
            }

            _spritePools.Clear();
            _textures.Clear();
        }
    }
}
