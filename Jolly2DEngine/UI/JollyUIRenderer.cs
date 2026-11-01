using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;

namespace Jolly2DEngine.UI
{
    public sealed class JollyUIRenderer
    {
        private readonly JollyUIManager _uiManager;
        private readonly Func<JollyTexture, JollySprite> _spriteFactory;
        private readonly Func<byte, byte, byte, byte, JollyTexture> _textureFactory;
        private readonly Func<string, string, float, JollyColor, bool, bool, bool, JollyTexture> _textTextureFactory;
        private readonly Dictionary<JollyColor, JollyTexture> _textures = new();
        private readonly Dictionary<JollyUIText, JollyTexture> _textTextures = new();
        private readonly Dictionary<JollyUIText, JollySprite> _textSprites = new();
        private readonly List<JollySprite> _sprites = new();
        private readonly Dictionary<JollyUIButton, JollyTexture> _buttonTextTextures = new();
        private readonly Dictionary<JollyUIButton, JollySprite> _buttonTextSprites = new();
        private readonly Dictionary<JollyUIProgressBar, JollyTexture> _progressTextTextures = new();
        private readonly Dictionary<JollyUIProgressBar, JollySprite> _progressTextSprites = new();
        private readonly Dictionary<JollyUIProgressBar, string> _progressTextValues = new();

        public IReadOnlyList<JollySprite> Sprites => _sprites;

        public JollyUIRenderer(JollyUIManager uiManager, Func<JollyTexture, JollySprite> spriteFactory, Func<byte, byte, byte, byte, JollyTexture> textureFactory, Func<string, string, float, JollyColor, bool, bool, bool, JollyTexture> textTextureFactory)
        {
            ArgumentNullException.ThrowIfNull(uiManager);
            ArgumentNullException.ThrowIfNull(spriteFactory);
            ArgumentNullException.ThrowIfNull(textureFactory);
            ArgumentNullException.ThrowIfNull(textTextureFactory);

            _uiManager = uiManager;
            _spriteFactory = spriteFactory;
            _textureFactory = textureFactory;
            _textTextureFactory = textTextureFactory;
        }

        public void Update()
        {
            _sprites.Clear();

            foreach (JollyUIElement element in _uiManager.Elements)
            {
                AddElement(element);
            }

            foreach (JollyUIElement element in _uiManager.Elements)
            {
                AddProgressBarText(element);
            }
        }

        private void AddElement(JollyUIElement element)
        {
            if (!element.Visible || element.Opacity <= 0.0f)
            {
                return;
            }

            if (element is JollyUIPanel panel)
            {
                AddPanel(panel);
            }
            else if (element is JollyUIText text)
            {
                AddText(text);
            }
            else if (element is JollyUIButton button)
            {
                AddButton(button);
            }
            else if (element is JollyUIProgressBar progressBar)
            {
                AddProgressBar(progressBar);
            }
            else if (element is JollyUISlider slider)
            {
                AddSlider(slider);
            }

            foreach (JollyUIElement child in element.Children)
            {
                AddElement(child);
            }
        }

        private void AddPanel(JollyUIPanel panel)
        {
            if (panel.Width <= 0.0f || panel.Height <= 0.0f)
            {
                return;
            }

            JollyTexture texture = GetTexture(panel.Color);
            JollySprite sprite = _spriteFactory(texture);

            sprite.X = panel.AbsoluteX + panel.Width * 0.5f;
            sprite.Y = panel.AbsoluteY + panel.Height * 0.5f;
            sprite.Width = panel.Width;
            sprite.Height = panel.Height;
            sprite.Opacity = System.Math.Clamp(panel.Opacity, 0.0f, 1.0f);
            sprite.Layer = panel.Layer;
            sprite.SourceX = 0.0f;
            sprite.SourceY = 0.0f;
            sprite.SourceWidth = 1.0f;
            sprite.SourceHeight = 1.0f;
            sprite.Visible = true;

            _sprites.Add(sprite);
        }

        private void AddText(JollyUIText text)
        {
            if (string.IsNullOrEmpty(text.Text))
            {
                return;
            }

            bool textureChanged = false;

            if (!_textTextures.TryGetValue(text, out JollyTexture? texture) || text.Dirty)
            {
                texture = _textTextureFactory(text.Text, text.FontFamily, text.FontSize, text.Color, text.Bold, text.Italic, text.UltraBold);
                _textTextures[text] = texture;
                text.Dirty = false;
                textureChanged = true;
            }

            if (!_textSprites.TryGetValue(text, out JollySprite? sprite) || textureChanged)
            {
                sprite = _spriteFactory(texture);
                _textSprites[text] = sprite;
            }

            sprite.X = text.AbsoluteX + texture.Width * 0.5f;
            sprite.Y = text.AbsoluteY + texture.Height * 0.5f;
            sprite.Width = texture.Width;
            sprite.Height = texture.Height;
            sprite.Opacity = System.Math.Clamp(text.Opacity, 0.0f, 1.0f);
            sprite.Layer = text.Layer;
            sprite.SourceX = 0.0f;
            sprite.SourceY = 0.0f;
            sprite.SourceWidth = 1.0f;
            sprite.SourceHeight = 1.0f;
            sprite.Visible = true;

            _sprites.Add(sprite);
        }

        private void AddButton(JollyUIButton button)
        {
            if (button.Width <= 0.0f || button.Height <= 0.0f)
            {
                return;
            }

            JollyTexture? buttonTexture = button.CurrentTexture;
            JollyUI9Slice? buttonNineSlice = button.CurrentNineSlice;

            if (buttonNineSlice != null)
            {
                Add9Slice(buttonNineSlice, button.AbsoluteX, button.AbsoluteY, button.Width, button.Height, button.Opacity, button.Layer);
            }
            else
            {
                JollyTexture backgroundTexture = buttonTexture ?? GetTexture(button.CurrentColor);

                JollySprite backgroundSprite = _spriteFactory(backgroundTexture);

                backgroundSprite.X = button.AbsoluteX + button.Width * 0.5f;
                backgroundSprite.Y = button.AbsoluteY + button.Height * 0.5f;
                backgroundSprite.Width = button.Width;
                backgroundSprite.Height = button.Height;
                backgroundSprite.Opacity = System.Math.Clamp(button.Opacity, 0.0f, 1.0f);
                backgroundSprite.Layer = button.Layer;
                backgroundSprite.SourceX = 0.0f;
                backgroundSprite.SourceY = 0.0f;
                backgroundSprite.SourceWidth = 1.0f;
                backgroundSprite.SourceHeight = 1.0f;
                backgroundSprite.Visible = true;

                _sprites.Add(backgroundSprite);
            }

            string fontFamily = "Arial";
            float fontSize = button.FontSize;

            JollyTexture textTexture;

            if (!_buttonTextTextures.TryGetValue(button, out textTexture!))
            {
                textTexture = _textTextureFactory(button.Text, fontFamily, fontSize, button.TextColor, button.Bold, false, false);
                _buttonTextTextures[button] = textTexture;
            }

            if (!_buttonTextSprites.TryGetValue(button, out JollySprite? textSprite))
            {
                textSprite = _spriteFactory(textTexture);
                _buttonTextSprites[button] = textSprite;
            }

            textSprite.X = button.AbsoluteX + button.Width * 0.5f;
            textSprite.Y = button.AbsoluteY + button.Height * 0.5f;
            textSprite.Width = textTexture.Width;
            textSprite.Height = textTexture.Height;
            textSprite.Opacity = System.Math.Clamp(button.Opacity, 0.0f, 1.0f);
            textSprite.Layer = button.Layer + 1;
            textSprite.SourceX = 0.0f;
            textSprite.SourceY = 0.0f;
            textSprite.SourceWidth = 1.0f;
            textSprite.SourceHeight = 1.0f;
            textSprite.Visible = true;

            _sprites.Add(textSprite);
        }

        private void Add9Slice(JollyUI9Slice slice, float x, float y, float width, float height, float opacity, int layer)
        {
            JollyTexture texture = slice.Texture;

            float left = System.Math.Min(slice.Left, width * 0.5f);
            float right = System.Math.Min(slice.Right, width * 0.5f);
            float top = System.Math.Min(slice.Top, height * 0.5f);
            float bottom = System.Math.Min(slice.Bottom, height * 0.5f);

            float centreWidth = System.Math.Max(0.0f, width - left - right);
            float centreHeight = System.Math.Max(0.0f, height - top - bottom);

            float sourceLeft = left / texture.Width;
            float sourceRight = right / texture.Width;
            float sourceTop = top / texture.Height;
            float sourceBottom = bottom / texture.Height;

            float sourceCentreWidth = System.Math.Max(0.0f, 1.0f - sourceLeft - sourceRight);
            float sourceCentreHeight = System.Math.Max(0.0f, 1.0f - sourceTop - sourceBottom);

            Add9SlicePart(texture, x, y, left, top, 0.0f, 0.0f, sourceLeft, sourceTop, opacity, layer);
            Add9SlicePart(texture, x + left, y, centreWidth, top, sourceLeft, 0.0f, sourceCentreWidth, sourceTop, opacity, layer);
            Add9SlicePart(texture, x + left + centreWidth, y, right, top, sourceLeft + sourceCentreWidth, 0.0f, sourceRight, sourceTop, opacity, layer);

            Add9SlicePart(texture, x, y + top, left, centreHeight, 0.0f, sourceTop, sourceLeft, sourceCentreHeight, opacity, layer);
            Add9SlicePart(texture, x + left, y + top, centreWidth, centreHeight, sourceLeft, sourceTop, sourceCentreWidth, sourceCentreHeight, opacity, layer);
            Add9SlicePart(texture, x + left + centreWidth, y + top, right, centreHeight, sourceLeft + sourceCentreWidth, sourceTop, sourceRight, sourceCentreHeight, opacity, layer);

            Add9SlicePart(texture, x, y + top + centreHeight, left, bottom, 0.0f, sourceTop + sourceCentreHeight, sourceLeft, sourceBottom, opacity, layer);
            Add9SlicePart(texture, x + left, y + top + centreHeight, centreWidth, bottom, sourceLeft, sourceTop + sourceCentreHeight, sourceCentreWidth, sourceBottom, opacity, layer);
            Add9SlicePart(texture, x + left + centreWidth, y + top + centreHeight, right, bottom, sourceLeft + sourceCentreWidth, sourceTop + sourceCentreHeight, sourceRight, sourceBottom, opacity, layer);
        }

        private void Add9SlicePart(JollyTexture texture, float x, float y, float width, float height, float sourceX, float sourceY, float sourceWidth, float sourceHeight, float opacity, int layer)
        {
            if (width <= 0.0f || height <= 0.0f || sourceWidth <= 0.0f || sourceHeight <= 0.0f)
            {
                return;
            }

            JollySprite sprite = _spriteFactory(texture);

            sprite.X = x + width * 0.5f;
            sprite.Y = y + height * 0.5f;
            sprite.Width = width;
            sprite.Height = height;
            sprite.Opacity = System.Math.Clamp(opacity, 0.0f, 1.0f);
            sprite.Layer = layer;
            sprite.SourceX = sourceX;
            sprite.SourceY = sourceY;
            sprite.SourceWidth = sourceWidth;
            sprite.SourceHeight = sourceHeight;
            sprite.Visible = true;

            _sprites.Add(sprite);
        }

        private void AddProgressBar(JollyUIProgressBar progressBar)
        {
            if (progressBar.Width <= 0.0f || progressBar.Height <= 0.0f)
            {
                return;
            }

            JollyTexture backgroundTexture = progressBar.BackgroundTexture ?? GetTexture(progressBar.BackgroundColor);

            JollySprite backgroundSprite = _spriteFactory(backgroundTexture);

            backgroundSprite.X = progressBar.AbsoluteX + progressBar.Width * 0.5f;
            backgroundSprite.Y = progressBar.AbsoluteY + progressBar.Height * 0.5f;
            backgroundSprite.Width = progressBar.Width;
            backgroundSprite.Height = progressBar.Height;
            backgroundSprite.Opacity = System.Math.Clamp(progressBar.Opacity, 0.0f, 1.0f);
            backgroundSprite.Layer = progressBar.Layer;
            backgroundSprite.SourceX = 0.0f;
            backgroundSprite.SourceY = 0.0f;
            backgroundSprite.SourceWidth = 1.0f;
            backgroundSprite.SourceHeight = 1.0f;
            backgroundSprite.Visible = true;

            _sprites.Add(backgroundSprite);

            float percentage = progressBar.Percentage;

            if (percentage > 0.0f)
            {
                JollyTexture fillTexture = progressBar.FillTexture ?? GetTexture(progressBar.FillColor);

                JollySprite fillSprite = _spriteFactory(fillTexture);

                if (progressBar.Orientation == JollyUIProgressBarOrientation.Horizontal)
                {
                    float fillWidth = progressBar.Width * percentage;

                    fillSprite.X = progressBar.AbsoluteX + fillWidth * 0.5f;
                    fillSprite.Y = progressBar.AbsoluteY + progressBar.Height * 0.5f;
                    fillSprite.Width = fillWidth;
                    fillSprite.Height = progressBar.Height;
                }
                else
                {
                    float fillHeight = progressBar.Height * percentage;

                    fillSprite.X = progressBar.AbsoluteX + progressBar.Width * 0.5f;
                    fillSprite.Y = progressBar.AbsoluteY + progressBar.Height - fillHeight * 0.5f;
                    fillSprite.Width = progressBar.Width;
                    fillSprite.Height = fillHeight;
                }

                fillSprite.Opacity = System.Math.Clamp(progressBar.Opacity, 0.0f, 1.0f);
                fillSprite.Layer = progressBar.Layer + 1;
                fillSprite.SourceX = 0.0f;
                fillSprite.SourceY = 0.0f;
                fillSprite.SourceWidth = 1.0f;
                fillSprite.SourceHeight = 1.0f;
                fillSprite.Visible = true;

                _sprites.Add(fillSprite);
            }

        }

        private void AddProgressBarText(JollyUIElement element)
        {
            if (!element.Visible || element.Opacity <= 0.0f)
            {
                return;
            }

            if (element is JollyUIProgressBar progressBar && progressBar.ShowText)
            {
                string text = string.IsNullOrEmpty(progressBar.Text) ? $"{progressBar.Percentage * 100.0f:0}%" : progressBar.Text;

                if (!_progressTextTextures.TryGetValue(progressBar, out JollyTexture? textTexture) || !_progressTextValues.TryGetValue(progressBar, out string? cachedText) || cachedText != text)
                {
                    textTexture = _textTextureFactory(text, "Arial", progressBar.FontSize, progressBar.TextColor, progressBar.Bold, false, false);
                    _progressTextTextures[progressBar] = textTexture;
                    _progressTextValues[progressBar] = text;
                    _progressTextSprites.Remove(progressBar);
                }

                if (!_progressTextSprites.TryGetValue(progressBar, out JollySprite? textSprite))
                {
                    textSprite = _spriteFactory(textTexture);
                    _progressTextSprites[progressBar] = textSprite;
                }

                textSprite.X = progressBar.AbsoluteX + progressBar.Width * 0.5f;
                textSprite.Y = progressBar.AbsoluteY + progressBar.Height * 0.5f;
                textSprite.Width = textTexture.Width;
                textSprite.Height = textTexture.Height;
                textSprite.Opacity = System.Math.Clamp(progressBar.Opacity, 0.0f, 1.0f);
                textSprite.Layer = progressBar.Layer + 2;
                textSprite.SourceX = 0.0f;
                textSprite.SourceY = 0.0f;
                textSprite.SourceWidth = 1.0f;
                textSprite.SourceHeight = 1.0f;
                textSprite.Visible = true;

                _sprites.Add(textSprite);
            }

            foreach (JollyUIElement child in element.Children)
            {
                AddProgressBarText(child);
            }
        }

        private void AddSlider(JollyUISlider slider)
        {
            if (slider.Width <= 0.0f || slider.Height <= 0.0f)
            {
                return;
            }

            JollyTexture trackTexture = slider.TrackTexture ?? GetTexture(slider.TrackColor);

            JollySprite trackSprite = _spriteFactory(trackTexture);

            if (slider.Orientation == JollyUISliderOrientation.Horizontal)
            {
                trackSprite.X = slider.AbsoluteX + slider.Width * 0.5f;
                trackSprite.Y = slider.AbsoluteY + slider.Height * 0.5f;
                trackSprite.Width = slider.Width;
                trackSprite.Height = slider.TrackThickness;
            }
            else
            {
                trackSprite.X = slider.AbsoluteX + slider.Width * 0.5f;
                trackSprite.Y = slider.AbsoluteY + slider.Height * 0.5f;
                trackSprite.Width = slider.TrackThickness;
                trackSprite.Height = slider.Height;
            }

            trackSprite.Opacity = System.Math.Clamp(slider.Opacity, 0.0f, 1.0f);
            trackSprite.Layer = slider.Layer;
            trackSprite.SourceX = 0.0f;
            trackSprite.SourceY = 0.0f;
            trackSprite.SourceWidth = 1.0f;
            trackSprite.SourceHeight = 1.0f;
            trackSprite.Visible = true;

            _sprites.Add(trackSprite);

            float percentage = slider.Percentage;

            if (percentage > 0.0f)
            {
                JollyTexture fillTexture = slider.FillTexture ?? GetTexture(slider.FillColor);

                JollySprite fillSprite = _spriteFactory(fillTexture);

                if (slider.Orientation == JollyUISliderOrientation.Horizontal)
                {
                    float fillWidth = slider.Width * percentage;

                    fillSprite.X = slider.AbsoluteX + fillWidth * 0.5f;
                    fillSprite.Y = slider.AbsoluteY + slider.Height * 0.5f;
                    fillSprite.Width = fillWidth;
                    fillSprite.Height = slider.TrackThickness;
                }
                else
                {
                    float fillHeight = slider.Height * percentage;

                    fillSprite.X = slider.AbsoluteX + slider.Width * 0.5f;
                    fillSprite.Y = slider.AbsoluteY + slider.Height - fillHeight * 0.5f;
                    fillSprite.Width = slider.TrackThickness;
                    fillSprite.Height = fillHeight;
                }

                fillSprite.Opacity = System.Math.Clamp(slider.Opacity, 0.0f, 1.0f);
                fillSprite.Layer = slider.Layer + 1;
                fillSprite.SourceX = 0.0f;
                fillSprite.SourceY = 0.0f;
                fillSprite.SourceWidth = 1.0f;
                fillSprite.SourceHeight = 1.0f;
                fillSprite.Visible = true;

                _sprites.Add(fillSprite);
            }

            JollyTexture handleTexture = slider.HandleTexture ?? GetTexture(slider.HandleColor);

            JollySprite handleSprite = _spriteFactory(handleTexture);

            if (slider.Orientation == JollyUISliderOrientation.Horizontal)
            {
                float usableWidth = System.Math.Max(0.0f, slider.Width - slider.HandleSize);
                handleSprite.X = slider.AbsoluteX + slider.HandleSize * 0.5f + usableWidth * percentage;
                handleSprite.Y = slider.AbsoluteY + slider.Height * 0.5f;
            }
            else
            {
                float usableHeight = System.Math.Max(0.0f, slider.Height - slider.HandleSize);

                handleSprite.X = slider.AbsoluteX + slider.Width * 0.5f;
                handleSprite.Y = slider.AbsoluteY + slider.Height - slider.HandleSize * 0.5f - usableHeight * percentage;
            }

            handleSprite.Width = slider.HandleSize;
            handleSprite.Height = slider.HandleSize;
            handleSprite.Opacity = System.Math.Clamp(slider.Opacity, 0.0f, 1.0f);
            handleSprite.Layer = slider.Layer + 2;
            handleSprite.SourceX = 0.0f;
            handleSprite.SourceY = 0.0f;
            handleSprite.SourceWidth = 1.0f;
            handleSprite.SourceHeight = 1.0f;
            handleSprite.Visible = true;

            _sprites.Add(handleSprite);
        }

        private JollyTexture GetTexture(JollyColor color)
        {
            if (_textures.TryGetValue(color, out JollyTexture? texture))
            {
                return texture;
            }

            byte red = (byte)(color.R * 255.0f);
            byte green = (byte)(color.G * 255.0f);
            byte blue = (byte)(color.B * 255.0f);
            byte alpha = (byte)(color.A * 255.0f);

            texture = _textureFactory(red, green, blue, alpha);
            _textures.Add(color, texture);

            return texture;
        }

        public void Clear()
        {
            _sprites.Clear();
            _textures.Clear();
            _textTextures.Clear();
            _textSprites.Clear();
            _buttonTextTextures.Clear();
            _buttonTextSprites.Clear();
            _progressTextTextures.Clear();
            _progressTextSprites.Clear();
            _progressTextValues.Clear();
        }
    }
}