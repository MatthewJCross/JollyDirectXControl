using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;

namespace Jolly2DEngine.UI
{
    public enum JollyUIProgressBarOrientation
    {
        Horizontal,
        Vertical
    }

    public sealed class JollyUIProgressBar : JollyUIElement
    {
        private float _minimum;
        private float _maximum = 1.0f;
        private float _value;
        private JollyColor _backgroundColor = JollyColor.FromBytes(40, 40, 40, 255);
        private JollyColor _fillColor = JollyColor.FromBytes(60, 180, 80, 255);

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
            set { _value = System.Math.Clamp(value, _minimum, _maximum); }
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

        public JollyUIProgressBarOrientation Orientation { get; set; } = JollyUIProgressBarOrientation.Horizontal;

        public JollyColor BackgroundColor
        {
            get { return _backgroundColor; }
            set { _backgroundColor = value; }
        }

        public JollyColor FillColor
        {
            get { return _fillColor; }
            set { _fillColor = value; }
        }

        public JollyTexture? BackgroundTexture { get; set; }

        public JollyTexture? FillTexture { get; set; }

        private bool _showText;
        private string _text = string.Empty;
        private JollyColor _textColor = JollyColor.White;
        private float _fontSize = 16.0f;
        private bool _bold;

        public bool ShowText
        {
            get { return _showText; }
            set
            {
                if (_showText == value)
                {
                    return;
                }

                _showText = value;
                TextDirty = true;
            }
        }

        public string Text
        {
            get { return _text; }
            set
            {
                string newValue = value ?? string.Empty;

                if (_text == newValue)
                {
                    return;
                }

                _text = newValue;
                TextDirty = true;
            }
        }

        public JollyColor TextColor
        {
            get { return _textColor; }
            set
            {
                if (_textColor == value)
                {
                    return;
                }

                _textColor = value;
                TextDirty = true;
            }
        }

        public float FontSize
        {
            get { return _fontSize; }
            set
            {
                if (_fontSize == value)
                {
                    return;
                }

                _fontSize = value;
                TextDirty = true;
            }
        }

        public bool Bold
        {
            get { return _bold; }
            set
            {
                if (_bold == value)
                {
                    return;
                }

                _bold = value;
                TextDirty = true;
            }
        }

        public bool TextDirty { get; internal set; } = true;
    }
}