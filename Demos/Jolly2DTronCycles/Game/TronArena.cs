using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.DirectX;

namespace Jolly2DTronCycles.Game
{
    public sealed class TronArena
    {
        private const float WallLayerSize = 4.0f;

        private readonly DirectXJollyRenderer _renderer;
        private readonly List<JollySprite> _sprites = [];

        private const float ScoreAreaHeight = 80.0f;
        private const int GridColumns = 15;
        private const int GridRows = 14;

        private readonly List<JollySprite> _gridSprites = [];
        private readonly List<JollySprite> _borderSprites = [];

        private readonly JollyTexture _gridTexture;

        public float X => 0.0f;
        public float Y => ScoreAreaHeight;
        public float Width { get; private set; }
        public float Height { get; private set; }
        public float PlayableHeight => Height;

        public float Left => X;
        public float Top => Y;
        public float Right => X + Width;
        public float Bottom => Y + Height;

        public IReadOnlyList<JollySprite> Sprites => _sprites;

        public TronArena(DirectXJollyRenderer renderer, float width, float height)
        {
            ArgumentNullException.ThrowIfNull(renderer);

            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0.0f);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, ScoreAreaHeight);

            _renderer = renderer;

            _gridTexture = _renderer.CreateSolidTexture(28, 28, 28, 255);

            UpdateBounds(width, height);

            CreateGrid();
            CreateBorder();
        }

        public void Resize(float width, float height)
        {
            if (width <= 0.0f || height <= ScoreAreaHeight)
            {
                return;
            }

            UpdateBounds(width, height);
            ResizeGrid();
            ResizeWalls();
        }

        private void CreateGrid()
        {
            float gridHeight = PlayableHeight;
            float horizontalSpacing = Width / GridColumns;
            float verticalSpacing = gridHeight / GridRows;

            for (int y = 1; y < GridRows; y++)
            {
                float positionY = Y + y * verticalSpacing;

                JollySprite sprite = _renderer.CreateSprite(_gridTexture);
                sprite.Name = "TronGridHorizontal";
                sprite.Position = new JollyVector2(X + Width * 0.5f, positionY);
                sprite.Size = new JollyVector2(Width, 1.0f);
                sprite.Layer = 1;

                _gridSprites.Add(sprite);
                _sprites.Add(sprite);
            }

            for (int x = 1; x < GridColumns; x++)
            {
                float positionX = X + x * horizontalSpacing;

                JollySprite sprite = _renderer.CreateSprite(_gridTexture);
                sprite.Name = "TronGridVertical";
                sprite.Position = new JollyVector2(positionX, Y + gridHeight * 0.5f);
                sprite.Size = new JollyVector2(1.0f, gridHeight);
                sprite.Layer = 1;

                _gridSprites.Add(sprite);
                _sprites.Add(sprite);
            }
        }

        private void CreateBorder()
        {
            CreateBorderLayer(12.0f, 0, 16, 64, 2);
            CreateBorderLayer(8.0f, 0, 77, 166, 3);
            CreateBorderLayer(5.0f, 0, 255, 255, 4);
            CreateBorderLayer(2.0f, 125, 125, 125, 5);
        }

        private void CreateBorderLayer(float thickness, byte red, byte green, byte blue, int layer)
        {
            float gridTop = ScoreAreaHeight;
            float gridHeight = Height;

            JollyTexture texture = _renderer.CreateSolidTexture(red, green, blue, 255);

            JollySprite top = _renderer.CreateSprite(texture);
            top.Name = "TronBorderTop";
            top.Position = new JollyVector2(Width * 0.5f, gridTop + thickness * 0.5f);
            top.Size = new JollyVector2(Width, thickness);
            top.Layer = layer;
            _borderSprites.Add(top);
            _sprites.Add(top);

            JollySprite bottom = _renderer.CreateSprite(texture);
            bottom.Name = "TronBorderBottom";
            bottom.Position = new JollyVector2(Width * 0.5f, gridTop + gridHeight - thickness * 0.5f);
            bottom.Size = new JollyVector2(Width, thickness);
            bottom.Layer = layer;
            _borderSprites.Add(bottom);
            _sprites.Add(bottom);

            JollySprite left = _renderer.CreateSprite(texture);
            left.Name = "TronBorderLeft";
            left.Position = new JollyVector2(thickness * 0.5f, gridTop + gridHeight * 0.5f);
            left.Size = new JollyVector2(thickness, gridHeight);
            left.Layer = layer;
            _borderSprites.Add(left);
            _sprites.Add(left);

            JollySprite right = _renderer.CreateSprite(texture);
            right.Name = "TronBorderRight";
            right.Position = new JollyVector2(Width - thickness * 0.5f, gridTop + gridHeight * 0.5f);
            right.Size = new JollyVector2(thickness, gridHeight);
            right.Layer = layer;
            _borderSprites.Add(right);
            _sprites.Add(right);
        }

        private void UpdateBounds(float width, float height)
        {
            Width = width;
            Height = height - ScoreAreaHeight;
        }

        private void ResizeGrid()
        {
            float horizontalSpacing = Width / GridColumns;
            float verticalSpacing = Height / GridRows;

            for (int y = 1; y < GridRows; y++)
            {
                float positionY = Y + y * verticalSpacing;
                JollySprite sprite = _gridSprites[y - 1];
                sprite.Position = new JollyVector2(X + Width * 0.5f, positionY);
                sprite.Size = new JollyVector2(Width, 1.0f);
            }

            for (int x = 1; x < GridColumns; x++)
            {
                float positionX = X + x * horizontalSpacing;
                JollySprite sprite = _gridSprites[(GridRows - 1) + (x - 1)];
                sprite.Position = new JollyVector2(positionX, Y + Height * 0.5f);
                sprite.Size = new JollyVector2(1.0f, Height);
            }
        }

        private void ResizeWalls()
        {
            float gridTop = ScoreAreaHeight;
            float gridHeight = Height;

            ResizeBorderLayer(0, 12.0f, gridTop, gridHeight);
            ResizeBorderLayer(4, 8.0f, gridTop, gridHeight);
            ResizeBorderLayer(8, 5.0f, gridTop, gridHeight);
            ResizeBorderLayer(12, 2.0f, gridTop, gridHeight);
        }

        private void ResizeBorderLayer(int index, float thickness, float gridTop, float gridHeight)
        {
            JollySprite top = _borderSprites[index];
            top.Position = new JollyVector2(Width * 0.5f, gridTop + thickness * 0.5f);
            top.Size = new JollyVector2(Width, thickness);

            JollySprite bottom = _borderSprites[index + 1];
            bottom.Position = new JollyVector2(Width * 0.5f, gridTop + gridHeight - thickness * 0.5f);
            bottom.Size = new JollyVector2(Width, thickness);

            JollySprite left = _borderSprites[index + 2];
            left.Position = new JollyVector2(thickness * 0.5f, gridTop + gridHeight * 0.5f);
            left.Size = new JollyVector2(thickness, gridHeight);

            JollySprite right = _borderSprites[index + 3];
            right.Position = new JollyVector2(Width - thickness * 0.5f, gridTop + gridHeight * 0.5f);
            right.Size = new JollyVector2(thickness, gridHeight);
        }

        private JollySprite CreateSprite(JollyTexture texture, float x, float y, float width, float height, int layer)
        {
            JollySprite sprite = _renderer.CreateSprite(texture);
            sprite.Position = new JollyVector2(x, y);
            sprite.Size = new JollyVector2(width, height);
            sprite.Rotation = 0.0f;
            sprite.Opacity = 1.0f;
            sprite.Visible = true;
            sprite.Layer = layer;
            sprite.FlipHorizontal = false;
            sprite.FlipVertical = false;

            _sprites.Add(sprite);

            return sprite;
        }

        public bool IsOutside(float x, float y)
        {
            return x < Left || x > Right || y < Top || y > Bottom;
        }
    }
}