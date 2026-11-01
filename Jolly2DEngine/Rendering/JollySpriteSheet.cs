
using Jolly2DEngine.Graphics;

namespace Jolly2DEngine.Rendering
{
    public enum JollySpriteSheetOrder
    {
        Horizontal,
        Vertical
    }

    public readonly struct JollySpriteSheetFrame
    {
        public int Index { get; }
        public int Column { get; }
        public int Row { get; }
        public int X { get; }
        public int Y { get; }
        public int Width { get; }
        public int Height { get; }

        internal JollySpriteSheetFrame(int index, int column, int row, int x, int y, int width, int height)
        {
            Index = index;
            Column = column;
            Row = row;
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }
    }

    public sealed class JollySpriteSheet
    {
        private readonly int _columns;
        private readonly int _rows;

        public JollyTexture Texture { get; }
        public int FrameWidth { get; }
        public int FrameHeight { get; }
        public int Columns => _columns;
        public int Rows => _rows;
        public int FrameCount => _columns * _rows;
        public JollySpriteSheetOrder Order { get; }

        public JollySpriteSheet(JollyTexture texture, int frameWidth, int frameHeight, JollySpriteSheetOrder order = JollySpriteSheetOrder.Horizontal)
        {
            ArgumentNullException.ThrowIfNull(texture);

            if (frameWidth <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frameWidth));
            }

            if (frameHeight <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(frameHeight));
            }

            _columns = texture.Width / frameWidth;
            _rows = texture.Height / frameHeight;

            if (_columns <= 0)
            {
                throw new ArgumentException("The frame width is larger than the texture width.", nameof(frameWidth));
            }

            if (_rows <= 0)
            {
                throw new ArgumentException("The frame height is larger than the texture height.", nameof(frameHeight));
            }

            Texture = texture;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            Order = order;
        }

        public JollySpriteSheetFrame GetFrame(int index)
        {
            if (index < 0 || index >= FrameCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            int column;
            int row;

            if (Order == JollySpriteSheetOrder.Horizontal)
            {
                column = index % _columns;
                row = index / _columns;
            }
            else
            {
                column = index / _rows;
                row = index % _rows;
            }

            return new JollySpriteSheetFrame(
                index,
                column,
                row,
                column * FrameWidth,
                row * FrameHeight,
                FrameWidth,
                FrameHeight);
        }
    }
}