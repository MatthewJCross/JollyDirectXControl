using Jolly2DEngine.Graphics;
using Jolly2DEngine.Physics;

namespace Jolly2DEngine.Core
{
    public class JollySpriteObject : JollyGameObject
    {
        private readonly HashSet<JollyGameObject> _collisions = new();
        private readonly HashSet<JollyGameObject> _triggers = new();
        public JollySprite Sprite { get; }
        public bool IsColliding => _collisions.Count > 0;
        public bool IsTriggered => _triggers.Count > 0;
        public IReadOnlyCollection<JollyGameObject> Collisions => _collisions;
        public IReadOnlyCollection<JollyGameObject> Triggers => _triggers;
        public JollyCollisionLayer CollisionLayer => Collider?.Layer ?? JollyCollisionLayer.None;
        public JollyCollisionLayer CollisionMask => Collider?.Mask ?? JollyCollisionLayer.None;

        public JollySpriteObject(JollySprite sprite)
        {
            ArgumentNullException.ThrowIfNull(sprite);
            Sprite = sprite;
        }

        public override void Initialize()
        {
            SynchronizeSprite();
        }

        public override void Update(JollyGameTime gameTime)
        {
            base.Update(gameTime);
            Sprite.UpdateAnimation((float)gameTime.DeltaSeconds);
            SynchronizeSprite();
        }

        public override void Draw()
        {
        }

        public override void Shutdown()
        {
            _collisions.Clear();
            _triggers.Clear();
            Sprite.Opacity = 1.0f;
        }

        public void EnableCollision(JollyCollisionMode mode = JollyCollisionMode.Bounds, byte alphaThreshold = 1)
        {
            switch (mode)
            {
                case JollyCollisionMode.Bounds:
                    SetRectangleCollider(Sprite.Width, Sprite.Height);
                    break;

                case JollyCollisionMode.Pixel:
                    SetPixelCollider(alphaThreshold);
                    break;

                default:
                    RemoveCollider();
                    break;
            }
        }

        public void SetRectangleCollider(float width, float height)
        {
            if (width <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            JollyCollider collider = new JollyCollider(Sprite);
            collider.Shape = JollyColliderShape.Rectangle;
            collider.Width = width;
            collider.Height = height;
            SetCollider(collider);
        }

        public void SetCircleCollider(float radius)
        {
            if (radius <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }

            JollyCollider collider = new JollyCollider(Sprite);
            collider.Shape = JollyColliderShape.Circle;
            collider.Radius = radius;
            SetCollider(collider);
        }

        public void SetPixelCollider(byte alphaThreshold = 1)
        {
            JollyCollider collider = new JollyCollider(Sprite);
            collider.Shape = JollyColliderShape.Pixel;
            collider.AlphaThreshold = alphaThreshold;
            SetCollider(collider);
        }

        public void SetCollisionLayers(JollyCollisionLayer layer, JollyCollisionLayer mask)
        {
            if (Collider == null)
            {
                SetRectangleCollider(Sprite.Width, Sprite.Height);
            }

            Collider!.Layer = layer;
            Collider.Mask = mask;
        }

        public void SetTrigger(bool isTrigger = true)
        {
            if (Collider == null)
            {
                SetRectangleCollider(Sprite.Width, Sprite.Height);
            }

            Collider!.IsTrigger = isTrigger;
        }

        public void DisableCollision()
        {
            if (Collider != null)
            {
                Collider.Enabled = false;
            }

            _collisions.Clear();
            _triggers.Clear();
        }

        public override void OnCollisionEnter(JollyGameObject other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (!_collisions.Add(other))
            {
                return;
            }

            Sprite.Opacity = 0.5f;
        }

        public override void OnCollisionStay(JollyGameObject other)
        {
            ArgumentNullException.ThrowIfNull(other);
            _collisions.Add(other);
        }

        public override void OnCollisionExit(JollyGameObject other)
        {
            ArgumentNullException.ThrowIfNull(other);

            if (!_collisions.Remove(other))
            {
                return;
            }

            if (_collisions.Count == 0)
            {
                Sprite.Opacity = 1.0f;
            }
        }

        public override void OnTriggerEnter(JollyGameObject other)
        {
            ArgumentNullException.ThrowIfNull(other);
            _triggers.Add(other);
        }

        public override void OnTriggerStay(JollyGameObject other)
        {
            ArgumentNullException.ThrowIfNull(other);
            _triggers.Add(other);
        }

        public override void OnTriggerExit(JollyGameObject other)
        {
            ArgumentNullException.ThrowIfNull(other);
            _triggers.Remove(other);
        }

        private void SynchronizeSprite()
        {
            Sprite.Position = Position;
            Sprite.Scale = Scale;
            Sprite.Rotation = Rotation;
            Sprite.Visible = Visible;
        }
    }
}