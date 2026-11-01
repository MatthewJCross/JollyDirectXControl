using Jolly2DEngine.Graphics;

namespace Jolly2DEngine.UI
{
    public sealed class JollyUI9Slice
    {
        public JollyTexture Texture { get; }
        public float Left { get; set; }
        public float Top { get; set; }
        public float Right { get; set; }
        public float Bottom { get; set; }

        public JollyUI9Slice(JollyTexture texture, float left, float top, float right, float bottom)
        {
            ArgumentNullException.ThrowIfNull(texture);

            if (left < 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(left));
            }

            if (top < 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(top));
            }

            if (right < 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(right));
            }

            if (bottom < 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(bottom));
            }

            Texture = texture;
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }
    }
}

