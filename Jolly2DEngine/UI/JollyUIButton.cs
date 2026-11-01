using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;

namespace Jolly2DEngine.UI
{
    public sealed class JollyUIButton : JollyUIElement
    {
        public string Text { get; set; } = "Button";
        public JollyColor NormalColor { get; set; } = JollyColor.FromBytes(60, 60, 60, 255);
        public JollyColor HoverColor { get; set; } = JollyColor.FromBytes(80, 80, 80, 255);
        public JollyColor PressedColor { get; set; } = JollyColor.FromBytes(40, 40, 40, 255);
        public JollyColor DisabledColor { get; set; } = JollyColor.FromBytes(35, 35, 35, 255);
        public JollyColor TextColor { get; set; } = JollyColor.White;
        public float FontSize { get; set; } = 18.0f;
        public bool Bold { get; set; }
        public bool IsHovered { get; internal set; }
        public bool IsPressed { get; internal set; }

        public event Action<JollyUIButton>? Click;

        public JollyColor CurrentColor
        {
            get
            {
                if (!Enabled)
                {
                    return DisabledColor;
                }

                if (IsPressed)
                {
                    return PressedColor;
                }

                if (IsHovered)
                {
                    return HoverColor;
                }

                return NormalColor;
            }
        }
        public JollyTexture? NormalTexture { get; set; }
        public JollyTexture? HoverTexture { get; set; }
        public JollyTexture? PressedTexture { get; set; }
        public JollyTexture? DisabledTexture { get; set; }
        public JollyTexture? CurrentTexture
        {
            get
            {
                if (!Enabled)
                {
                    return DisabledTexture ?? NormalTexture;
                }

                if (IsPressed)
                {
                    return PressedTexture ?? HoverTexture ?? NormalTexture;
                }

                if (IsHovered)
                {
                    return HoverTexture ?? NormalTexture;
                }

                return NormalTexture;
            }
        }

        public JollyUI9Slice? NormalNineSlice { get; set; }
        public JollyUI9Slice? HoverNineSlice { get; set; }
        public JollyUI9Slice? PressedNineSlice { get; set; }
        public JollyUI9Slice? DisabledNineSlice { get; set; }

        public JollyUI9Slice? CurrentNineSlice
        {
            get
            {
                if (!Enabled)
                {
                    return DisabledNineSlice ?? NormalNineSlice;
                }

                if (IsPressed)
                {
                    return PressedNineSlice ?? HoverNineSlice ?? NormalNineSlice;
                }

                if (IsHovered)
                {
                    return HoverNineSlice ?? NormalNineSlice;
                }

                return NormalNineSlice;
            }
        }

        internal void RaiseClick()
        {
            if (!Enabled || !Visible)
            {
                return;
            }

            Click?.Invoke(this);
        }
    }
}