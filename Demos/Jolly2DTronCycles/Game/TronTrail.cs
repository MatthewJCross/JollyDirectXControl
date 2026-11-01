using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.DirectX;

namespace Jolly2DTronCycles.Game
{
    public sealed class TronTrail
    {
        private const float TrailWidth = 4.0f;
        private const float TrailOverlap = 3.0f;

        private readonly DirectXJollyRenderer _renderer;
        private readonly JollyTexture _texture;
        private readonly List<JollySprite> _sprites = [];
        private readonly List<JollySprite> _spritePool = [];
        private readonly List<(float X1, float Y1, float X2, float Y2)> _segments = [];

        private JollySprite? _currentSprite;
        private TronDirection _direction;
        private float _startX;
        private float _startY;
        private float _currentX;
        private float _currentY;
        private bool _hasCurrentSegment;

        public IReadOnlyList<JollySprite> Sprites => _sprites;

        public TronTrail(DirectXJollyRenderer renderer, JollyTexture texture)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            ArgumentNullException.ThrowIfNull(texture);

            _renderer = renderer;
            _texture = texture;
        }

        public void Update(float x, float y, TronDirection direction, float width, float height)
        {
            _currentX = x;
            _currentY = y;

            if (_currentSprite == null)
            {
                _direction = direction;
                GetRearPoint(x, y, direction, width, height, out _startX, out _startY);
                _currentSprite = CreateSegment();
                _sprites.Add(_currentSprite);
                _hasCurrentSegment = true;
                return;
            }

            if (direction != _direction)
            {
                AddSegment(_startX, _startY, x, y);
                UpdateSegment(_currentSprite, _startX, _startY, x, y, _direction);

                JollySprite corner = CreateCorner(x, y);
                _sprites.Add(corner);

                _direction = direction;
                _startX = x;
                _startY = y;

                _currentSprite = CreateSegment();
                _sprites.Add(_currentSprite);
                _hasCurrentSegment = true;

                return;
            }

            UpdateSegment(_currentSprite, _startX, _startY, x, y, _direction);
            _hasCurrentSegment = true;
        }

        public void Clear()
        {
            foreach (JollySprite sprite in _sprites)
            {
                sprite.Visible = false;
                _spritePool.Add(sprite);
            }

            _sprites.Clear();
            _segments.Clear();
            _currentSprite = null;
            _hasCurrentSegment = false;
        }

        public bool IsColliding(float x, float y, float radius)
        {
            foreach ((float x1, float y1, float x2, float y2) in _segments)
            {
                if (IsPointNearSegment(x, y, radius, x1, y1, x2, y2))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsCollidingIncludingCurrent(float x, float y, float radius)
        {
            if (IsColliding(x, y, radius))
            {
                return true;
            }

            if (!_hasCurrentSegment)
            {
                return false;
            }

            return IsPointNearSegment(x, y, radius, _startX, _startY, _currentX, _currentY);
        }

        public bool IsCollidingRectangle(float centerX, float centerY, float width, float height)
        {
            foreach ((float x1, float y1, float x2, float y2) in _segments)
            {
                if (IsSegmentIntersectingRectangle(x1, y1, x2, y2, centerX, centerY, width, height))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsCollidingRectangleIncludingCurrent(float centerX, float centerY, float width, float height)
        {
            if (IsCollidingRectangle(centerX, centerY, width, height))
            {
                return true;
            }

            if (!_hasCurrentSegment)
            {
                return false;
            }

            return IsSegmentIntersectingRectangle(
                _startX,
                _startY,
                _currentX,
                _currentY,
                centerX,
                centerY,
                width,
                height);
        }

        private void AddSegment(float startX, float startY, float endX, float endY)
        {
            if (startX == endX && startY == endY)
            {
                return;
            }

            _segments.Add((startX, startY, endX, endY));
        }

        private JollySprite CreateSegment()
        {
            JollySprite sprite;

            if (_spritePool.Count > 0)
            {
                int lastIndex = _spritePool.Count - 1;
                sprite = _spritePool[lastIndex];
                _spritePool.RemoveAt(lastIndex);
            }
            else
            {
                sprite = _renderer.CreateSprite(_texture);
            }

            sprite.Name = "TronTrail";
            sprite.Position = new JollyVector2(_startX, _startY);
            sprite.Size = new JollyVector2(TrailWidth, TrailWidth);
            sprite.Opacity = 1.0f;
            sprite.Visible = true;
            sprite.Layer = 90;
            return sprite;
        }

        private void UpdateSegment(JollySprite sprite, float startX, float startY, float endX, float endY, TronDirection direction)
        {
            float x = (startX + endX) * 0.5f;
            float y = (startY + endY) * 0.5f;
            float width = MathF.Abs(endX - startX);
            float height = MathF.Abs(endY - startY);

            switch (direction)
            {
                case TronDirection.Left:
                case TronDirection.Right:
                    width += TrailOverlap;
                    break;

                case TronDirection.Up:
                case TronDirection.Down:
                    height += TrailOverlap;
                    break;
            }

            sprite.Position = new JollyVector2(x, y);
            sprite.Size = new JollyVector2(
                MathF.Max(width, TrailWidth),
                MathF.Max(height, TrailWidth));
        }

        private static void GetRearPoint(float x, float y, TronDirection direction, float width, float height, out float rearX, out float rearY)
        {
            rearX = x;
            rearY = y;

            switch (direction)
            {
                case TronDirection.Up:
                    rearY += height * 0.5f;
                    break;

                case TronDirection.Down:
                    rearY -= height * 0.5f;
                    break;

                case TronDirection.Left:
                    rearX += width * 0.5f;
                    break;

                case TronDirection.Right:
                    rearX -= width * 0.5f;
                    break;
            }
        }

        private static bool IsPointNearSegment(float x, float y, float radius, float x1, float y1, float x2, float y2)
        {
            float dx = x2 - x1;
            float dy = y2 - y1;
            float lengthSquared = dx * dx + dy * dy;

            if (lengthSquared <= 0.0f)
            {
                float distanceX = x - x1;
                float distanceY = y - y1;
                return distanceX * distanceX + distanceY * distanceY <= radius * radius;
            }

            float t = ((x - x1) * dx + (y - y1) * dy) / lengthSquared;
            t = MathF.Max(0.0f, MathF.Min(1.0f, t));

            float closestX = x1 + t * dx;
            float closestY = y1 + t * dy;
            float distanceToX = x - closestX;
            float distanceToY = y - closestY;

            return distanceToX * distanceToX + distanceToY * distanceToY <= radius * radius;
        }

        private static bool IsSegmentIntersectingRectangle(
            float x1,
            float y1,
            float x2,
            float y2,
            float centerX,
            float centerY,
            float width,
            float height)
        {
            float halfWidth = width * 0.5f + TrailWidth * 0.5f;
            float halfHeight = height * 0.5f + TrailWidth * 0.5f;

            float minX = centerX - halfWidth;
            float maxX = centerX + halfWidth;
            float minY = centerY - halfHeight;
            float maxY = centerY + halfHeight;

            if (x1 >= minX && x1 <= maxX && y1 >= minY && y1 <= maxY)
            {
                return true;
            }

            if (x2 >= minX && x2 <= maxX && y2 >= minY && y2 <= maxY)
            {
                return true;
            }

            float dx = x2 - x1;
            float dy = y2 - y1;

            float tMin = 0.0f;
            float tMax = 1.0f;

            if (!ClipSegment(-dx, x1 - minX, ref tMin, ref tMax))
            {
                return false;
            }

            if (!ClipSegment(dx, maxX - x1, ref tMin, ref tMax))
            {
                return false;
            }

            if (!ClipSegment(-dy, y1 - minY, ref tMin, ref tMax))
            {
                return false;
            }

            if (!ClipSegment(dy, maxY - y1, ref tMin, ref tMax))
            {
                return false;
            }

            return true;
        }

        private static bool ClipSegment(float p, float q, ref float tMin, ref float tMax)
        {
            if (p == 0.0f)
            {
                return q >= 0.0f;
            }

            float r = q / p;

            if (p < 0.0f)
            {
                if (r > tMax)
                {
                    return false;
                }

                if (r > tMin)
                {
                    tMin = r;
                }
            }
            else
            {
                if (r < tMin)
                {
                    return false;
                }

                if (r < tMax)
                {
                    tMax = r;
                }
            }

            return true;
        }

        private JollySprite CreateCorner(float x, float y)
        {
            JollySprite sprite;

            if (_spritePool.Count > 0)
            {
                int lastIndex = _spritePool.Count - 1;
                sprite = _spritePool[lastIndex];
                _spritePool.RemoveAt(lastIndex);
            }
            else
            {
                sprite = _renderer.CreateSprite(_texture);
            }

            sprite.Name = "TronTrailCorner";
            sprite.Position = new JollyVector2(x, y);
            sprite.Size = new JollyVector2(TrailWidth, TrailWidth);
            sprite.Opacity = 1.0f;
            sprite.Visible = true;
            sprite.Layer = 90;
            return sprite;
        }
    }
}