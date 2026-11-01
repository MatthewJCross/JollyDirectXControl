using Jolly2DEngine.Assets;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.DirectX;
using System.Numerics;

namespace Jolly2DBeamRider.Game
{
    public sealed class BeamRiderArena : IDisposable
    {
        private const int BeamCount = 7;
        private const int GridLineCount = 5;
        private const float HorizonY = 200.0f;
        private const float ScrollSpeed = 1.25f;
        private const int BackgroundLayer = 100;

        private readonly float _width;
        private readonly float _height;
        private readonly DirectXJollyRenderer _renderer;
        private readonly JollyAssetManager _assets;
        private readonly List<JollySprite> _sprites = [];
        private readonly List<JollyLineSnapshot> _lines = [];

        private JollySprite? _backgroundSprite;
        private float _scroll;
        private bool _disposed;

        public BeamRiderArena(float width, float height, DirectXJollyRenderer renderer, JollyAssetManager assets)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0.0f);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0.0f);
            ArgumentNullException.ThrowIfNull(renderer);
            ArgumentNullException.ThrowIfNull(assets);

            _width = width;
            _height = height;
            _renderer = renderer;
            _assets = assets;

            CreateBackground();
            RebuildLines();
        }

        public IReadOnlyList<JollySprite> Sprites => _sprites;

        public IReadOnlyList<JollyLineSnapshot> Lines => _lines;

        public void Update(float deltaTime)
        {
            if (_disposed || deltaTime <= 0.0f)
            {
                return;
            }

            _scroll += deltaTime * ScrollSpeed;

            while (_scroll >= 1.0f)
            {
                _scroll -= 1.0f;
            }

            RebuildLines();
        }

        private void CreateBackground()
        {
            JollyTexture backgroundTexture = _assets.LoadTexture("BeamRiderBackground.png");
            _backgroundSprite = _renderer.CreateSprite(backgroundTexture);

            float textureWidth = backgroundTexture.Width;
            float textureHeight = backgroundTexture.Height;
            float scale = _width / textureWidth;
            float scaledHeight = textureHeight * scale;

            _backgroundSprite.Position = new JollyVector2(_width * 0.5f, 0 + (scaledHeight * 0.5f));
            _backgroundSprite.Size = new JollyVector2(_width, scaledHeight);
            _backgroundSprite.Rotation = 0.0f;
            _backgroundSprite.Opacity = 1.0f;
            _backgroundSprite.Layer = BackgroundLayer;
            _sprites.Add(_backgroundSprite);
        }

        private void RebuildLines()
        {
            _lines.Clear();
            CreatePerspectiveBeams();
            CreatePerspectiveGrid();
        }

        private void CreatePerspectiveBeams()
        {
            float vanishingPointX = _width * 0.5f;
            float horizonSpread = 180.0f;
            float bottomSpread = 1600.0f;

            for (int index = 0; index < BeamCount; index++)
            {
                float normalized = index / (float)(BeamCount - 1);
                float offset = normalized - 0.5f;
                float startX = vanishingPointX + (offset * horizonSpread);
                float endX = vanishingPointX + (offset * bottomSpread);
                Vector2 start = new(startX, HorizonY);
                Vector2 end = new(endX, _height + 10.0f);

                _lines.Add(new JollyLineSnapshot(start, end, new JollyColor(0, 220, 255, 255), 6.0f, 0.4f, 0.8f));
            }
        }

        private void CreatePerspectiveGrid()
        {
            for (int index = 0; index < GridLineCount; index++)
            {
                float normalized = (index + _scroll) / GridLineCount;
                normalized -= MathF.Floor(normalized);
                float perspective = MathF.Pow(normalized, 2.2f);
                float y = HorizonY + perspective * (_height - HorizonY);
                float thickness = 0.5f + perspective * 1.5f;
                byte alpha = (byte)Math.Clamp((0.15f + perspective * 0.65f) * 255.0f, 0.0f, 255.0f);

                _lines.Add(new JollyLineSnapshot(new Vector2(0.0f, y), new Vector2(_width, y), new JollyColor(0, 130, 220, alpha), thickness, thickness, thickness));
            }
        }

        public float GetBeamRotation(float lane)
        {
            float normalized = (lane + 3.0f) / 6.0f;
            float vanishingPointX = _width * 0.5f;
            float horizonSpread = 180.0f;
            float bottomSpread = 1600.0f;
            float startX = vanishingPointX + ((normalized - 0.5f) * horizonSpread);
            float endX = vanishingPointX + ((normalized - 0.5f) * bottomSpread);
            float startY = HorizonY;
            float endY = _height + 10.0f;
            float beamAngle = MathF.Atan2(endY - startY, endX - startX);

            return beamAngle - (MathF.PI * 0.5f);
        }

        public Vector2 GetBeamPosition(float lane)
        {
            return GetBeamPosition(lane, _height + 10.0f);
        }

        public Vector2 GetBeamPosition(float lane, float y)
        {
            float normalized = (lane + 3.0f) / 6.0f;
            float vanishingPointX = _width * 0.5f;
            float horizonSpread = 180.0f;
            float bottomSpread = 1600.0f;

            float startX = vanishingPointX + ((normalized - 0.5f) * horizonSpread);
            float endX = vanishingPointX + ((normalized - 0.5f) * bottomSpread);
            float startY = HorizonY;
            float endY = _height + 10.0f;
            float amount = Math.Clamp((y - startY) / (endY - startY), 0.0f, 1.0f);
            float x = startX + ((endX - startX) * amount);

            return new Vector2(x, y);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _sprites.Clear();
            _lines.Clear();
            _backgroundSprite = null;
        }
    }
}