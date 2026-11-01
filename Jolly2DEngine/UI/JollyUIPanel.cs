using Jolly2DEngine.Math;

namespace Jolly2DEngine.UI
{
    public sealed class JollyUIPanel : JollyUIElement
    {
        public JollyColor Color { get; set; } = JollyColor.White;

        public JollyColor BackgroundColor
        {
            get { return Color; }
            set { Color = value; }
        }

        public JollyUILayout Layout { get; set; } = JollyUILayout.None;
        public float Spacing { get; set; }
        public float PaddingLeft { get; set; }
        public float PaddingTop { get; set; }
        public float PaddingRight { get; set; }
        public float PaddingBottom { get; set; }
        public bool StretchChildren { get; set; }
    }
}