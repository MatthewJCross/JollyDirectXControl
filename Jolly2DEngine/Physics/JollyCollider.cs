using Jolly2DEngine.Graphics;

namespace Jolly2DEngine.Physics
{
    public sealed class JollyCollider
    {
        public JollySprite Sprite { get; }
        public bool Enabled { get; set; } = true;
        public bool IsTrigger { get; set; }
        public JollyCollisionLayer Layer { get; set; } = JollyCollisionLayer.Layer0;
        public JollyCollisionLayer Mask { get; set; } = JollyCollisionLayer.All;
        public JollyColliderShape Shape { get; set; } = JollyColliderShape.Rectangle;
        public float Width { get; set; }
        public float Height { get; set; }
        public float Radius { get; set; }
        public byte AlphaThreshold { get; set; } = 1;

        public JollyCollisionMode Mode
        {
            get
            {
                return Shape switch
                {
                    JollyColliderShape.Pixel => JollyCollisionMode.Pixel,
                    JollyColliderShape.Rectangle => JollyCollisionMode.Bounds,
                    JollyColliderShape.Circle => JollyCollisionMode.Bounds,
                    _ => JollyCollisionMode.None
                };
            }
        }

        public JollyCollider(JollySprite sprite)
        {
            ArgumentNullException.ThrowIfNull(sprite);
            Sprite = sprite;
            Width = sprite.Width;
            Height = sprite.Height;
        }

        public bool CanCollideWith(JollyCollider other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (!Enabled || !other.Enabled)
            {
                return false;
            }

            if (Shape == JollyColliderShape.None || other.Shape == JollyColliderShape.None)
            {
                return false;
            }

            if ((Mask & other.Layer) == 0)
            {
                return false;
            }

            if ((other.Mask & Layer) == 0)
            {
                return false;
            }

            return true;
        }

        public bool CollidesWith(JollyCollider other)
        {
            ArgumentNullException.ThrowIfNull(other);
            return JollyCollisionSystem.Check(this, other);
        }

        public bool Check(JollyCollider other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (!CanCollideWith(other))
            {
                return false;
            }

            return JollyCollisionSystem.Check(this, other);
        }
    }
}