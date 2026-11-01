using Jolly2DEngine.Core;
using Jolly2DEngine.Math;
using Jolly2DEngine.Physics;

namespace Jolly2DEngine.Save
{
    public sealed class JollyGameObjectSaveState
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public bool Active { get; set; }
        public JollyVector2 Position { get; set; }
        public JollyVector2 Scale { get; set; }
        public float Rotation { get; set; }
        public bool Visible { get; set; }
        public JollyVector2 Velocity { get; set; }
        public JollyVector2 Acceleration { get; set; }
        public JollyVector2 Gravity { get; set; }
        public float Drag { get; set; }
        public bool UsePhysics { get; set; }
        public bool UseGravity { get; set; }
        public bool IsGrounded { get; set; }
        public bool WasGrounded { get; set; }
        public float GroundFriction { get; set; }
        public float AirFriction { get; set; }
        public float Restitution { get; set; }
        public JollyColliderSaveState? Collider { get; set; }

        public static JollyGameObjectSaveState Capture(JollyGameObject gameObject)
        {
            ArgumentNullException.ThrowIfNull(gameObject);

            return new JollyGameObjectSaveState
            {
                Id = gameObject.SaveId,
                Name = gameObject.Name,
                Tag = gameObject.Tag,
                Active = gameObject.Active,
                Position = gameObject.Position,
                Scale = gameObject.Scale,
                Rotation = gameObject.Rotation,
                Visible = gameObject.Visible,
                Velocity = gameObject.Velocity,
                Acceleration = gameObject.Acceleration,
                Gravity = gameObject.Gravity,
                Drag = gameObject.Drag,
                UsePhysics = gameObject.UsePhysics,
                UseGravity = gameObject.UseGravity,
                IsGrounded = gameObject.IsGrounded,
                WasGrounded = gameObject.WasGrounded,
                GroundFriction = gameObject.GroundFriction,
                AirFriction = gameObject.AirFriction,
                Restitution = gameObject.Restitution,
                Collider = gameObject.Collider == null ? null : JollyColliderSaveState.Capture(gameObject.Collider)
            };
        }

        public void ApplyTo(JollyGameObject gameObject)
        {
            ArgumentNullException.ThrowIfNull(gameObject);

            gameObject.Name = Name;
            gameObject.Tag = Tag;
            gameObject.Active = Active;
            gameObject.Position = Position;
            gameObject.Scale = Scale;
            gameObject.Rotation = Rotation;
            gameObject.Visible = Visible;
            gameObject.Velocity = Velocity;
            gameObject.Acceleration = Acceleration;
            gameObject.Gravity = Gravity;
            gameObject.Drag = Drag;
            gameObject.UsePhysics = UsePhysics;
            gameObject.UseGravity = UseGravity;
            gameObject.IsGrounded = IsGrounded;
            gameObject.WasGrounded = WasGrounded;
            gameObject.GroundFriction = GroundFriction;
            gameObject.AirFriction = AirFriction;
            gameObject.Restitution = Restitution;

            if (Collider != null && gameObject.Collider != null)
            {
                Collider.ApplyTo(gameObject.Collider);
            }
        }
    }

    public sealed class JollyColliderSaveState
    {
        public bool Enabled { get; set; }
        public bool IsTrigger { get; set; }
        public JollyCollisionLayer Layer { get; set; }
        public JollyCollisionLayer Mask { get; set; }
        public JollyColliderShape Shape { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public float Radius { get; set; }
        public byte AlphaThreshold { get; set; }

        public static JollyColliderSaveState Capture(JollyCollider collider)
        {
            ArgumentNullException.ThrowIfNull(collider);

            return new JollyColliderSaveState
            {
                Enabled = collider.Enabled,
                IsTrigger = collider.IsTrigger,
                Layer = collider.Layer,
                Mask = collider.Mask,
                Shape = collider.Shape,
                Width = collider.Width,
                Height = collider.Height,
                Radius = collider.Radius,
                AlphaThreshold = collider.AlphaThreshold
            };
        }

        public void ApplyTo(JollyCollider collider)
        {
            ArgumentNullException.ThrowIfNull(collider);

            collider.Enabled = Enabled;
            collider.IsTrigger = IsTrigger;
            collider.Layer = Layer;
            collider.Mask = Mask;
            collider.Shape = Shape;
            collider.Width = Width;
            collider.Height = Height;
            collider.Radius = Radius;
            collider.AlphaThreshold = AlphaThreshold;
        }
    }
}
