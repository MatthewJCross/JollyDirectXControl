using Jolly2DEngine.Graphics;

namespace Jolly2DEngine.Physics
{
    public readonly struct JollyCollisionResult
    {
        public JollySprite SpriteA { get; }
        public JollySprite SpriteB { get; }
        public bool Collided { get; }

        public JollyCollisionResult(JollySprite spriteA, JollySprite spriteB, bool collided)
        {
            SpriteA = spriteA;
            SpriteB = spriteB;
            Collided = collided;
        }
    }
}
