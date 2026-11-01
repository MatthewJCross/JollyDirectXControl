using System.Numerics;

namespace Jolly2DEngine.UI
{
    public sealed class JollyUIManager
    {
        private JollyUIElement? _pressedElement;
        private JollyUIElement? _hoveredElement;

        private JollyUISlider? _draggedSlider;
        private readonly List<JollyUIAnimation> _animations = new();
        private readonly List<JollyUIElement> _elements = new();

        public IReadOnlyList<JollyUIElement> Elements => _elements;
        public System.Numerics.Vector2 MousePosition { get; private set; }
        public JollyUIElement? HoveredElement => _hoveredElement;
        public JollyUIElement? PressedElement => _pressedElement; public float ScreenWidth { get; private set; }
        public float ScreenHeight { get; private set; }

        public void SetScreenSize(float width, float height)
        {
            if (width < 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height < 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            ScreenWidth = width;
            ScreenHeight = height;
        }

        public void Add(JollyUIElement element)
        {
            ArgumentNullException.ThrowIfNull(element);

            if (!_elements.Contains(element))
            {
                _elements.Add(element);
            }
        }

        public bool Remove(JollyUIElement element)
        {
            ArgumentNullException.ThrowIfNull(element);

            _animations.RemoveAll(animation => ReferenceEquals(animation.Element, element));

            return _elements.Remove(element);
        }

        public void Clear()
        {
            _animations.Clear();
            _elements.Clear();
        }

        public JollyUIElement? HitTest(Vector2 point)
        {
            JollyUIElement? result = null;
            int highestLayer = int.MinValue;

            foreach (JollyUIElement element in _elements)
            {
                JollyUIElement? hit = HitTestElement(element, point);

                if (hit == null)
                {
                    continue;
                }

                if (hit.Layer >= highestLayer)
                {
                    highestLayer = hit.Layer;
                    result = hit;
                }
            }

            return result;
        }

        private JollyUIElement? HitTestElement(JollyUIElement element, Vector2 point)
        {
            if (!element.Visible || !element.Enabled)
            {
                return null;
            }

            JollyUIElement? result = null;
            int highestLayer = int.MinValue;

            foreach (JollyUIElement child in element.Children)
            {
                JollyUIElement? hit = HitTestElement(child, point);

                if (hit == null)
                {
                    continue;
                }

                if (hit.Layer >= highestLayer)
                {
                    highestLayer = hit.Layer;
                    result = hit;
                }
            }

            if (result != null)
            {
                return result;
            }

            return element.ContainsPoint(point) ? element : null;
        }

        public void MouseMove(float x, float y)
        {
            MousePosition = new System.Numerics.Vector2(x, y);

            if (_draggedSlider != null)
            {
                _draggedSlider.MouseMove(MousePosition);
                return;
            }

            JollyUIElement? element = HitTest(MousePosition);

            if (element is JollyUIButton button && !button.Enabled)
            {
                element = null;
            }

            if (_hoveredElement == element)
            {
                return;
            }

            if (_hoveredElement is JollyUIButton oldButton)
            {
                oldButton.IsHovered = false;
            }

            _hoveredElement = element;

            if (_hoveredElement is JollyUIButton newButton)
            {
                newButton.IsHovered = true;
            }
        }

        public void MouseDown()
        {
            JollyUIElement? element = HitTest(MousePosition);

            if (element is JollyUISlider slider)
            {
                _draggedSlider = slider;
                slider.MouseDown(MousePosition);
                return;
            }

            if (element is JollyUIButton button && !button.Enabled)
            {
                return;
            }

            _pressedElement = element;

            if (_pressedElement is JollyUIButton pressedButton)
            {
                pressedButton.IsPressed = true;
            }
        }

        public void MouseUp()
        {
            if (_draggedSlider != null)
            {
                _draggedSlider.MouseUp();
                _draggedSlider = null;
                return;
            }

            JollyUIElement? pressedElement = _pressedElement;
            JollyUIElement? releasedElement = HitTest(MousePosition);

            _pressedElement = null;

            if (pressedElement is JollyUIButton pressedButton)
            {
                pressedButton.IsPressed = false;

                if (ReferenceEquals(pressedElement, releasedElement))
                {
                    pressedButton.RaiseClick();
                }
            }
        }

        public JollyUIAnimation AnimatePosition(JollyUIElement element, Vector2 target, float duration)
        {
            ArgumentNullException.ThrowIfNull(element);

            JollyUIAnimation animation = new(element, JollyUIAnimationProperty.Position, element.Position, target, duration);
            _animations.Add(animation);
            return animation;
        }

        public JollyUIAnimation AnimateSize(JollyUIElement element, Vector2 target, float duration)
        {
            ArgumentNullException.ThrowIfNull(element);

            JollyUIAnimation animation = new(element, JollyUIAnimationProperty.Size, element.Size, target, duration);
            _animations.Add(animation);
            return animation;
        }

        public JollyUIAnimation AnimateOpacity(JollyUIElement element, float target, float duration)
        {
            ArgumentNullException.ThrowIfNull(element);

            JollyUIAnimation animation = new(element, JollyUIAnimationProperty.Opacity, element.Opacity, System.Math.Clamp(target, 0.0f, 1.0f), duration);
            _animations.Add(animation);
            return animation;
        }

        public void Update(float deltaSeconds)
        {
            for (int i = _animations.Count - 1; i >= 0; i--)
            {
                JollyUIAnimation animation = _animations[i];
                animation.Update(deltaSeconds);

                if (animation.IsComplete)
                {
                    _animations.RemoveAt(i);
                }
            }
        }

        public void UpdateLayout()
        {
            foreach (JollyUIElement element in _elements)
            {
                LayoutElement(element, ScreenWidth, ScreenHeight);
            }
        }

        private void LayoutElement(JollyUIElement element, float parentWidth, float parentHeight)
        {
            float width = element.Width;
            float height = element.Height;

            switch (element.Anchor)
            {
                case JollyUIAnchor.TopLeft:
                    element.X = element.MarginLeft;
                    element.Y = element.MarginTop;
                    break;

                case JollyUIAnchor.Top:
                    element.X = (parentWidth - width) * 0.5f + element.MarginLeft - element.MarginRight;
                    element.Y = element.MarginTop;
                    break;

                case JollyUIAnchor.TopRight:
                    element.X = parentWidth - width - element.MarginRight;
                    element.Y = element.MarginTop;
                    break;

                case JollyUIAnchor.Left:
                    element.X = element.MarginLeft;
                    element.Y = (parentHeight - height) * 0.5f + element.MarginTop - element.MarginBottom;
                    break;

                case JollyUIAnchor.Center:
                    element.X = (parentWidth - width) * 0.5f + element.MarginLeft - element.MarginRight;
                    element.Y = (parentHeight - height) * 0.5f + element.MarginTop - element.MarginBottom;
                    break;

                case JollyUIAnchor.Right:
                    element.X = parentWidth - width - element.MarginRight;
                    element.Y = (parentHeight - height) * 0.5f + element.MarginTop - element.MarginBottom;
                    break;

                case JollyUIAnchor.BottomLeft:
                    element.X = element.MarginLeft;
                    element.Y = parentHeight - height - element.MarginBottom;
                    break;

                case JollyUIAnchor.Bottom:
                    element.X = (parentWidth - width) * 0.5f + element.MarginLeft - element.MarginRight;
                    element.Y = parentHeight - height - element.MarginBottom;
                    break;

                case JollyUIAnchor.BottomRight:
                    element.X = parentWidth - width - element.MarginRight;
                    element.Y = parentHeight - height - element.MarginBottom;
                    break;

                case JollyUIAnchor.Stretch:
                    element.X = element.MarginLeft;
                    element.Y = element.MarginTop;
                    element.Width = System.Math.Max(0.0f, parentWidth - element.MarginLeft - element.MarginRight);
                    element.Height = System.Math.Max(0.0f, parentHeight - element.MarginTop - element.MarginBottom);
                    break;
            }

            if (element is JollyUIPanel panel && panel.Layout != JollyUILayout.None)
            {
                LayoutPanelChildren(panel);

                foreach (JollyUIElement child in panel.Children)
                {
                    LayoutElementWithoutAnchor(child);
                }

                return;
            }

            foreach (JollyUIElement child in element.Children)
            {
                LayoutElement(child, element.Width, element.Height);
            }
        }

        private void LayoutElementWithoutAnchor(JollyUIElement element)
        {
            if (element is JollyUIPanel panel && panel.Layout != JollyUILayout.None)
            {
                LayoutPanelChildren(panel);

                foreach (JollyUIElement child in panel.Children)
                {
                    LayoutElementWithoutAnchor(child);
                }

                return;
            }

            foreach (JollyUIElement child in element.Children)
            {
                LayoutElement(child, element.Width, element.Height);
            }
        }

        private void LayoutPanelChildren(JollyUIPanel panel)
        {
            float availableWidth = System.Math.Max(0.0f, panel.Width - panel.PaddingLeft - panel.PaddingRight);
            float availableHeight = System.Math.Max(0.0f, panel.Height - panel.PaddingTop - panel.PaddingBottom);

            float offset = 0.0f;

            foreach (JollyUIElement child in panel.Children)
            {
                if (!child.Visible)
                {
                    continue;
                }

                if (panel.Layout == JollyUILayout.Vertical)
                {
                    child.X = panel.PaddingLeft + child.MarginLeft;
                    child.Y = panel.PaddingTop + offset + child.MarginTop;

                    if (panel.StretchChildren)
                    {
                        child.Width = System.Math.Max(0.0f, availableWidth - child.MarginLeft - child.MarginRight);
                    }

                    offset += child.MarginTop + child.Height + child.MarginBottom + panel.Spacing;
                }
                else if (panel.Layout == JollyUILayout.Horizontal)
                {
                    child.X = panel.PaddingLeft + offset + child.MarginLeft;
                    child.Y = panel.PaddingTop + child.MarginTop;

                    if (panel.StretchChildren)
                    {
                        child.Height = System.Math.Max(0.0f, availableHeight - child.MarginTop - child.MarginBottom);
                    }

                    offset += child.MarginLeft + child.Width + child.MarginRight + panel.Spacing;
                }
            }
        }
    }
}