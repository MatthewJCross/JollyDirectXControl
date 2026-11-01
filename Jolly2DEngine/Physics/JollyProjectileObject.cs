using Jolly2DEngine.Physics;
using Jolly2DEngine.Graphics;

namespace Jolly2DEngine.Core
{
    public sealed class JollyProjectileObject : JollySpriteObject
    {
        private float _remainingLifetime;
        public float Lifetime { get; set; } = 5.0f;
        public int Damage { get; set; } = 1;
        public bool DestroyOnCollision { get; set; } = true;
        public bool IsExpired { get; private set; }

        public JollyProjectileObject(JollySprite sprite) : base(sprite)
        {
            UsePhysics = true;
        }

        public override void Initialize()
        {
            base.Initialize();
            _remainingLifetime = Lifetime;
            IsExpired = false;
        }

        public override void Update(JollyGameTime gameTime)
        {
            base.Update(gameTime);

            _remainingLifetime -= (float)gameTime.DeltaSeconds;

            if (_remainingLifetime <= 0.0f)
            {
                IsExpired = true;
                Destroy();
            }
        }

        public override void OnCollisionEnter(JollyGameObject other)
        {
            base.OnCollisionEnter(other);

            if (DestroyOnCollision)
            {
                IsExpired = true;
                Destroy();
            }
        }
    }
}
