
using Jolly2DEngine.Math;
using Jolly2DEngine.Physics;
using Jolly2DEngine.Graphics.Scene;

namespace Jolly2DEngine.Core
{
    public abstract class JollyGameObject
    {
        public string SaveId { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = string.Empty;
        public string Tag { get; set; } = string.Empty;
        public bool Active { get; set; } = true;
        public JollyVector2 Position { get; set; }
        public JollyVector2 Scale { get; set; } = JollyVector2.One;
        public float Rotation { get; set; }
        public bool Visible { get; set; } = true;
        public JollyVector2 Velocity { get; set; }
        public JollyVector2 Acceleration { get; set; }
        public JollyVector2 Gravity { get; set; }
        public float Drag { get; set; }
        public bool UsePhysics { get; set; }
        public bool UseGravity { get; set; }
        public bool IsGrounded { get; set; }
        public bool WasGrounded { get; set; }
        public float GroundFriction { get; set; } = 12.0f;
        public float AirFriction { get; set; } = 0.0f;
        public float Restitution { get; set; } = 0.0f;
        public JollyCollider? Collider { get; private set; }
        private JollyCharacterController? _characterController;
        public JollyCharacterController? CharacterController => _characterController;
        public bool IsDestroyed { get; private set; }
        internal JollyScene? Scene { get; set; }

        public virtual void Initialize()
        {
        }

        public virtual void Update(JollyGameTime gameTime)
        {
            _characterController?.Update(gameTime);
        }

        public virtual void Draw()
        {
        }

        public virtual void Shutdown()
        {
        }

        public void SetCollider(JollyCollider collider)
        {
            ArgumentNullException.ThrowIfNull(collider);
            Collider = collider;
        }

        public void RemoveCollider()
        {
            Collider = null;
            IsGrounded = false;
        }

        public virtual void OnCollisionEnter(JollyGameObject other)
        {
        }

        public virtual void OnCollisionStay(JollyGameObject other)
        {
        }

        public virtual void OnCollisionExit(JollyGameObject other)
        {
        }

        public virtual void OnTriggerEnter(JollyGameObject other)
        {
        }

        public virtual void OnTriggerStay(JollyGameObject other)
        {
        }

        public virtual void OnTriggerExit(JollyGameObject other)
        {
        }

        public JollyCharacterController AddCharacterController()
        {
            _characterController ??= new JollyCharacterController(this);
            return _characterController;
        }

        public void Destroy()
        {
            IsDestroyed = true;
            Active = false;
        }
    }
}