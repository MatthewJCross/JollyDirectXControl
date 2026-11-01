using System.Numerics;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;

namespace Jolly2DEngine.UI
{
    public enum JollyUISliderOrientation
    {
        Horizontal,
        Vertical
    }

    public sealed class JollyUISlider : JollyUIElement
    {
        private float _minimum;
        private float _maximum = 1.0f;
        private float _value;
        private bool _dragging;

        public JollyUISliderOrientation Orientation { get; set; } = JollyUISliderOrientation.Horizontal;

        public float Minimum
        {
            get { return _minimum; }
            set
            {
                _minimum = value;

                if (_maximum < _minimum)
                {
                    _maximum = _minimum;
                }

                Value = _value;
            }
        }

        public float Maximum
        {
            get { return _maximum; }
            set
            {
                if (value < _minimum)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), "Maximum cannot be less than Minimum.");
                }

                _maximum = value;
                Value = _value;
            }
        }

        public float Value
        {
            get { return _value; }
            set
            {
                float newValue = System.Math.Clamp(value, _minimum, _maximum);

                if (_value == newValue)
                {
                    return;
                }

                _value = newValue;
                ValueChanged?.Invoke(this, _value);
            }
        }

        public float Percentage
        {
            get
            {
                if (_maximum <= _minimum)
                {
                    return 0.0f;
                }

                return (_value - _minimum) / (_maximum - _minimum);
            }
        }

        public JollyColor TrackColor { get; set; } = JollyColor.FromBytes(50, 50, 50, 255);

        public JollyColor FillColor { get; set; } = JollyColor.FromBytes(80, 160, 240, 255);

        public JollyColor HandleColor { get; set; } = JollyColor.White;

        public float TrackThickness { get; set; } = 8.0f;

        public float HandleSize { get; set; } = 20.0f;

        public JollyTexture? TrackTexture { get; set; }

        public JollyTexture? FillTexture { get; set; }

        public JollyTexture? HandleTexture { get; set; }

        public bool IsDragging
        {
            get { return _dragging; }
        }

        public event Action<JollyUISlider, float>? ValueChanged;

        internal void MouseDown(Vector2 position)
        {
            if (!Visible || !Enabled)
            {
                return;
            }

            if (!ContainsPoint(position))
            {
                return;
            }

            _dragging = true;
            UpdateValueFromMouse(position);
        }

        internal void MouseMove(Vector2 position)
        {
            if (!_dragging)
            {
                return;
            }

            UpdateValueFromMouse(position);
        }

        internal void MouseUp()
        {
            _dragging = false;
        }

        private void UpdateValueFromMouse(Vector2 position)
        {
            float percentage;

            if (Orientation == JollyUISliderOrientation.Horizontal)
            {
                float usableWidth = System.Math.Max(1.0f, Width - HandleSize);
                float start = AbsoluteX + HandleSize * 0.5f;
                percentage = (position.X - start) / usableWidth;
            }
            else
            {
                float usableHeight = System.Math.Max(1.0f, Height - HandleSize);
                float start = AbsoluteY + HandleSize * 0.5f;
                percentage = 1.0f - ((position.Y - start) / usableHeight);
            }

            percentage = System.Math.Clamp(percentage, 0.0f, 1.0f);

            Value = _minimum + (_maximum - _minimum) * percentage;
        }
    }
}