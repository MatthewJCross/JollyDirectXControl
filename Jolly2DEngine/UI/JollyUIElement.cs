using System.Numerics;

namespace Jolly2DEngine.UI
{
    public class JollyUIElement
    {
        private readonly List<JollyUIElement> _children = new();
        public JollyUIElement? Parent { get; private set; }
        public IReadOnlyList<JollyUIElement> Children => _children;
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public bool Visible { get; set; } = true;
        public bool Enabled { get; set; } = true;
        public int Layer { get; set; }
        public float Opacity { get; set; } = 1.0f;
        public JollyUIAnchor Anchor { get; set; } = JollyUIAnchor.TopLeft;
        public float MarginLeft { get; set; }
        public float MarginTop { get; set; }
        public float MarginRight { get; set; }
        public float MarginBottom { get; set; }

        public float X
        {
            get
            {
                return Position.X;
            }
            set
            {
                Position = new Vector2(value, Position.Y);
            }
        }

        public float Y
        {
            get
            {
                return Position.Y;
            }
            set
            {
                Position = new Vector2(Position.X, value);
            }
        }

        public float Width
        {
            get
            {
                return Size.X;
            }
            set
            {
                Size = new Vector2(value, Size.Y);
            }
        }

        public float Height
        {
            get
            {
                return Size.Y;
            }
            set
            {
                Size = new Vector2(Size.X, value);
            }
        }

        public float AbsoluteX
        {
            get
            {
                return Parent == null ? X : Parent.AbsoluteX + X;
            }
        }

        public float AbsoluteY
        {
            get
            {
                return Parent == null ? Y : Parent.AbsoluteY + Y;
            }
        }

        public Vector2 AbsolutePosition
        {
            get
            {
                return new Vector2(AbsoluteX, AbsoluteY);
            }
        }

        public bool ContainsPoint(Vector2 point)
        {
            if (!Visible || !Enabled)
            {
                return false;
            }

            float absoluteX = AbsoluteX;
            float absoluteY = AbsoluteY;

            return point.X >= absoluteX && point.X < absoluteX + Width && point.Y >= absoluteY && point.Y < absoluteY + Height;
        }

        public void AddChild(JollyUIElement child)
        {
            ArgumentNullException.ThrowIfNull(child);

            if (ReferenceEquals(child, this))
            {
                throw new InvalidOperationException("A UI element cannot be its own child.");
            }

            if (child.Parent is not null)
            {
                child.Parent.RemoveChild(child);
            }

            child.Parent = this;
            _children.Add(child);
        }

        public bool RemoveChild(JollyUIElement child)
        {
            ArgumentNullException.ThrowIfNull(child);

            if (!_children.Remove(child))
            {
                return false;
            }

            child.Parent = null;
            return true;
        }

        public void RemoveAllChildren()
        {
            foreach (JollyUIElement child in _children)
            {
                child.Parent = null;
            }

            _children.Clear();
        }

        public void SetMargin(float left, float top, float right, float bottom)
        {
            MarginLeft = left;
            MarginTop = top;
            MarginRight = right;
            MarginBottom = bottom;
        }
    }
}
