using Jolly2DEngine.Core;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using System.Numerics;

namespace Jolly2DBeamRider.Game
{
    public sealed class BeamRiderBackground : IDisposable
    {
        private const int VerticalBeamCount = 7;
        private const int HorizontalLineCount = 16;
        private const float VanishingY = 40.0f;
        private const float HorizontalSpacing = 34.0f;
        private const float ScrollSpeed = 95.0f;
        private const float BeamTopWidth = 2.0f;
        private const float BeamBottomWidth = 9.0f;
        private const float BeamSpread = 620.0f;
        private const float BeamCurve = 45.0f;

        private readonly float _width;
        private readonly float _height;
        private readonly List<JollyLineSnapshot> _lines = [];
        private float _scrollOffset;
        private bool _disposed;

        public BeamRiderBackground(float width, float height)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0.0f);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0.0f);

            _width = width;
            _height = height;

            RebuildLines();
        }

        public IReadOnlyList<JollyLineSnapshot> Lines => _lines;

        public void Update(JollyGameTime gameTime)
        {
            if (_disposed)
            {
                return;
            }

            float deltaTime = (float)gameTime.DeltaSeconds;

            if (deltaTime <= 0.0f)
            {
                return;
            }

            _scrollOffset += ScrollSpeed * deltaTime;

            float cycle = HorizontalSpacing * HorizontalLineCount;

            while (_scrollOffset >= cycle)
            {
                _scrollOffset -= cycle;
            }

            RebuildLines();
        }

        private void RebuildLines()
        {
            _lines.Clear();

            CreateVerticalLines();
            CreateHorizontalLines();
        }

        private void CreateVerticalLines()
        {
            for (int beamIndex = 0; beamIndex < VerticalBeamCount; beamIndex++)
            {
                float normalized = beamIndex / (float)(VerticalBeamCount - 1);
                float bottomX = _width * 0.5f + (normalized - 0.5f) * BeamSpread;
                float x = CalculateBeamX(bottomX, 1.0f);

                _lines.Add(new JollyLineSnapshot(new Vector2(_width * 0.5f, VanishingY), new Vector2(x, _height), new JollyColor(0, 220, 255, 255), BeamBottomWidth, BeamTopWidth, BeamBottomWidth));
            }
        }

        private float CalculateBeamX(float bottomX, float t)
        {
            float x = _width * 0.5f + (bottomX - _width * 0.5f) * MathF.Pow(t, 0.92f);
            float curve = MathF.Sin(t * MathF.PI) * BeamCurve * (bottomX < _width * 0.5f ? -1.0f : 1.0f);
            return x + curve;
        }

        private void CreateHorizontalLines()
        {
            for (int index = 0; index < HorizontalLineCount; index++)
            {
                float distance = index * HorizontalSpacing + _scrollOffset;
                float normalizedDistance = distance / (HorizontalSpacing * HorizontalLineCount);
                float perspective = MathF.Pow(normalizedDistance, 1.45f);
                float y = VanishingY + perspective * (_height - VanishingY);
                float thickness = 1.5f + perspective * 3.0f;
                float opacity = 0.18f + perspective * 0.55f;

                _lines.Add(new JollyLineSnapshot(new Vector2(0.0f, y), new Vector2(_width, y), new JollyColor(0, 120, 180, (byte)(Math.Clamp(opacity, 0.0f, 1.0f) * 255.0f)), thickness, thickness, thickness));
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _lines.Clear();
        }
    }
}
