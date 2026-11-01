using Jolly2DEngine.Math;

namespace Jolly2DEngine.Graphics
{
    public readonly struct JollySpriteSnapshot
    {
        public string Name { get; }
        public JollyTexture Texture { get; }
        public float X { get; }
        public float Y { get; }
        public float Width { get; }
        public float Height { get; }
        public float Rotation { get; }
        public float Opacity { get; }
        public JollyColor Color { get; init; }
        public bool Visible { get; }
        public int Layer { get; }
        public bool FlipHorizontal { get; }
        public bool FlipVertical { get; }
        public float SourceX { get; }
        public float SourceY { get; }
        public float SourceWidth { get; }
        public float SourceHeight { get; }
        public JollyVector2 Scale { get; }

        internal JollySpriteSnapshot(JollySprite sprite)
        {
            Name = sprite.Name;
            Texture = sprite.Texture;
            X = sprite.X;
            Y = sprite.Y;
            Width = sprite.Width;
            Height = sprite.Height;
            Rotation = sprite.Rotation;
            Opacity = sprite.Opacity;
            Visible = sprite.Visible;
            Layer = sprite.Layer;
            FlipHorizontal = sprite.FlipHorizontal;
            FlipVertical = sprite.FlipVertical;
            SourceX = sprite.SourceX;
            SourceY = sprite.SourceY;
            SourceWidth = sprite.SourceWidth;
            SourceHeight = sprite.SourceHeight;
            Scale = sprite.Scale;
        }
    }
}

