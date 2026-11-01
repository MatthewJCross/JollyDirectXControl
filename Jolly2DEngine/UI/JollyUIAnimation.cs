using System.Numerics;

namespace Jolly2DEngine.UI
{
    public enum JollyUIAnimationProperty
    {
        Position,
        Size,
        Opacity
    }

    public sealed class JollyUIAnimation
    {
        private readonly Vector2 _startVector;
        private readonly Vector2 _endVector;
        private readonly float _startFloat;
        private readonly float _endFloat;
        private float _elapsed;

        public JollyUIElement Element { get; }
        public JollyUIAnimationProperty Property { get; }
        public float Duration { get; }
        public bool IsComplete { get; private set; }

        internal JollyUIAnimation(JollyUIElement element, JollyUIAnimationProperty property, Vector2 start, Vector2 end, float duration)
        {
            Element = element;
            Property = property;
            _startVector = start;
            _endVector = end;
            Duration = duration;
        }

        internal JollyUIAnimation(JollyUIElement element, JollyUIAnimationProperty property, float start, float end, float duration)
        {
            Element = element;
            Property = property;
            _startFloat = start;
            _endFloat = end;
            Duration = duration;
        }

        internal void Update(float deltaSeconds)
        {
            if (IsComplete)
            {
                return;
            }

            _elapsed += System.Math.Max(0.0f, deltaSeconds);

            float progress = Duration <= 0.0f ? 1.0f : System.Math.Clamp(_elapsed / Duration, 0.0f, 1.0f);
            float easedProgress = progress * progress * (3.0f - 2.0f * progress);

            switch (Property)
            {
                case JollyUIAnimationProperty.Position:
                    Element.Position = Vector2.Lerp(_startVector, _endVector, easedProgress);
                    break;

                case JollyUIAnimationProperty.Size:
                    Element.Size = Vector2.Lerp(_startVector, _endVector, easedProgress);
                    break;

                case JollyUIAnimationProperty.Opacity:
                    Element.Opacity = _startFloat + (_endFloat - _startFloat) * easedProgress;
                    break;
            }

            if (progress >= 1.0f)
            {
                IsComplete = true;
            }
        }
    }
}
