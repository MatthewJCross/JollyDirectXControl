using Jolly2DEngine.Math;

namespace Jolly2DEngine.UI
{
    public sealed class JollyUIText : JollyUIElement
    {
        private string _text = string.Empty;
        private string _fontFamily = "Arial";
        private float _fontSize = 20.0f;
        private JollyColor _color = JollyColor.White;
        private bool _ultraBold;
        private bool _bold;
        private bool _italic;

        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                string newValue = value ?? string.Empty;

                if (_text == newValue)
                {
                    return;
                }

                _text = newValue;
                Dirty = true;
            }
        }

        public string FontFamily
        {
            get
            {
                return _fontFamily;
            }
            set
            {
                string newValue = value ?? "Arial";

                if (_fontFamily == newValue)
                {
                    return;
                }

                _fontFamily = newValue;
                Dirty = true;
            }
        }

        public float FontSize
        {
            get
            {
                return _fontSize;
            }
            set
            {
                if (_fontSize == value)
                {
                    return;
                }

                _fontSize = value;
                Dirty = true;
            }
        }

        public JollyColor Color
        {
            get
            {
                return _color;
            }
            set
            {
                if (_color == value)
                {
                    return;
                }

                _color = value;
                Dirty = true;
            }
        }

        public bool UltraBold
        {
            get
            {
                return _ultraBold;
            }
            set
            {
                if (_ultraBold == value)
                {
                    return;
                }

                _ultraBold = value;
                Dirty = true;
            }
        }

        public bool Bold
        {
            get
            {
                return _bold;
            }
            set
            {
                if (_bold == value)
                {
                    return;
                }

                _bold = value;
                Dirty = true;
            }
        }

        public bool Italic
        {
            get
            {
                return _italic;
            }
            set
            {
                if (_italic == value)
                {
                    return;
                }

                _italic = value;
                Dirty = true;
            }
        }

        public bool Dirty { get; internal set; } = true;
    }
}