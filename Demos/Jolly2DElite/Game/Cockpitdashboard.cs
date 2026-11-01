
using Jolly2DEngine.Assets;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.DirectX;
using System.Numerics;

namespace Jolly2DElite.Game
{
    internal sealed class CockpitDashboard
    {
        private readonly DirectXJollyRenderer _renderer;
        private readonly JollyAssetManager _assets;

        private readonly float _worldWidth;
        private readonly float _worldHeight;
        private readonly float _cockpitHeight;

        private JollyTexture? _pixelTexture;

        private JollySprite? _cockpitSprite;
        private readonly List<JollySprite> _sprites = [];
        private int _spriteIndex;

        private const float DashboardWidth = 256.0f;
        private const float DashboardHeight = 56.0f;

        public CockpitDashboard(DirectXJollyRenderer renderer, JollyAssetManager assets, float worldWidth, float worldHeight, float cockpitHeight)
        {
            ArgumentNullException.ThrowIfNull(renderer);
            ArgumentNullException.ThrowIfNull(assets);

            _renderer = renderer;
            _assets = assets;
            _worldWidth = worldWidth;
            _worldHeight = worldHeight;
            _cockpitHeight = cockpitHeight;
        }

        public void Initialise()
        {
            JollyTexture cockpitTexture = _assets!.LoadTexture("Cockpit.png");
            _cockpitSprite = _renderer.CreateSprite(cockpitTexture);
            _cockpitSprite.Position = new JollyVector2(_worldWidth * 0.5f, _worldHeight - _cockpitHeight * 0.5f);
            _cockpitSprite.Layer = 0;

            byte[] pixels = [255, 255, 255, 255];
            _pixelTexture = _renderer.CreateTextureFromBgraPixels(1, 1, pixels);
        }

        public void BeginFrame(List<JollySprite> sprites)
        {
            _spriteIndex = 0;

            foreach (JollySprite sprite in _sprites)
            {
                sprite.Visible = false;
            }
        }

        private void DrawDashboardRect(List<JollySprite> sprites, float x, float y, float width, float height, JollyColor color)
        {
            ArgumentNullException.ThrowIfNull(_pixelTexture);

            float scaleX = _worldWidth / DashboardWidth;
            float scaleY = _cockpitHeight / DashboardHeight;

            JollySprite sprite;

            if (_spriteIndex < _sprites.Count)
            {
                sprite = _sprites[_spriteIndex];
            }
            else
            {
                sprite = _renderer.CreateSprite(_pixelTexture);
                sprite.Layer = 1;
                _sprites.Add(sprite);
            }

            _spriteIndex++;

            float spriteWidth = width * scaleX;
            float spriteHeight = height * scaleY;

            sprite.Size = new JollyVector2(spriteWidth, spriteHeight);
            sprite.Position = new JollyVector2(x * scaleX + spriteWidth * 0.5f, _worldHeight - _cockpitHeight + y * scaleY + spriteHeight * 0.5f);

            sprite.Color = color;
            sprite.Visible = true;

            sprites.Add(sprite);
        }

        public void Draw(List<JollySprite> sprites, int speed, float roll, float pitch, int forwardShield, int aftShield, int energy, int fuel, int missiles, int laserTemperature, IReadOnlyList<WorldObject>? worldObjects = null, PlayerShip? player = null)
        {
            ArgumentNullException.ThrowIfNull(sprites);

            if (_cockpitSprite is not null)
            {
                sprites.Add(_cockpitSprite);
            }

            BeginFrame(sprites);

            // Draw the instrument bars and indicators here,
            // using the supplied current game values.
            DrawShieldBar(sprites, 16, 0, forwardShield);
            DrawShieldBar(sprites, 16, 1, aftShield);

            DrawFuelBar(sprites, 16, 2, fuel);

            DrawSpeedBar(sprites, 208, 0, speed);

            DrawRollIndicator(sprites, 208, 1, roll);
            DrawPitchIndicator(sprites, 208, 2, pitch);
            DrawScanner(sprites, worldObjects, player);

            DrawEnergyBanks(sprites, 218, 3, energy);

            DrawMissileIndicators(sprites, missiles);
            DrawTemperatureBar(sprites, 16, 4, laserTemperature);
        }

        private void DrawShieldBar(List<JollySprite> sprites, float x, int row, int shield)
        {
            int value = Math.Clamp(shield >> 4, 0, 16);
            JollyColor color = value >= 8 ? JollyColor.Red : JollyColor.Green;
            DrawDashboardRect(sprites, x, row * 8 + 2, value * 8, 3, color);
        }

        private void DrawFuelBar(List<JollySprite> sprites, float x, int row, int fuel)
        {
            int value = Math.Clamp(fuel >> 2, 0, 16);
            DrawDashboardRect(sprites, x, row * 8 + 2, value * 2, 3, JollyColor.Yellow);
        }

        private void DrawSpeedBar(List<JollySprite> sprites, float x, int row, int speed)
        {
            int value = Math.Clamp(speed >> 1, 0, 16);
            JollyColor color = value >= 8 ? JollyColor.Red : JollyColor.White;
            DrawDashboardRect(sprites, x, row * 8 + 2, value * 2, 3, color);
        }

        private void DrawRollIndicator(List<JollySprite> sprites, float x, int row, float roll)
        {
            float normalized = Math.Clamp(roll / MathF.PI, -1.0f, 1.0f);
            int value = Math.Clamp((int)MathF.Round((normalized + 1.0f) * 7.5f), 0, 15);

            DrawDashboardRect(sprites, x + value * 2.0f, row * 8 + 1, 2, 4, JollyColor.White);
            DrawDashboardRect(sprites, x + 15.0f, row * 8, 1, 6, JollyColor.Yellow);
        }

        private void DrawPitchIndicator(List<JollySprite> sprites, float x, int row, float pitch)
        {
            float normalized = Math.Clamp(pitch / (MathF.PI * 0.5f), -1.0f, 1.0f);
            int value = Math.Clamp((int)MathF.Round((normalized + 1.0f) * 7.5f), 0, 15);

            DrawDashboardRect(sprites, x + value * 2.0f, row * 8 + 1, 2, 4, JollyColor.White);
            DrawDashboardRect(sprites, x + 15.0f, row * 8, 1, 6, JollyColor.Yellow);
        }

        private void DrawScanner(List<JollySprite> sprites, IReadOnlyList<WorldObject>? worldObjects, PlayerShip? player)
        {
            const float scannerX = 80.0f;
            const float scannerY = 7.0f;
            const float scannerWidth = 96.0f;
            const float scannerHeight = 40.0f;
            const float scannerRange = 900.0f;

            DrawDashboardRect(sprites, scannerX, scannerY, scannerWidth, 1.0f, new JollyColor(40, 120, 40, 255));
            DrawDashboardRect(sprites, scannerX, scannerY + scannerHeight, scannerWidth, 1.0f, new JollyColor(40, 120, 40, 255));
            DrawDashboardRect(sprites, scannerX, scannerY, 1.0f, scannerHeight, new JollyColor(40, 120, 40, 255));
            DrawDashboardRect(sprites, scannerX + scannerWidth, scannerY, 1.0f, scannerHeight, new JollyColor(40, 120, 40, 255));
            DrawDashboardRect(sprites, scannerX + scannerWidth * 0.5f, scannerY, 1.0f, scannerHeight, new JollyColor(30, 70, 30, 255));
            DrawDashboardRect(sprites, scannerX, scannerY + scannerHeight * 0.5f, scannerWidth, 1.0f, new JollyColor(30, 70, 30, 255));

            if (worldObjects is null || player is null)
            {
                return;
            }

            foreach (WorldObject worldObject in worldObjects)
            {
                if (worldObject.Model is not (AdderModel or CobraMkIIIModel or AnacondaModel or AsteroidModel or CoriolisStationModel or DodecahedronStationModel))
                {
                    continue;
                }

                Vector3 relative = worldObject.Position - player.Position;
                float horizontal = Vector3.Dot(relative, player.Right);
                float depth = Vector3.Dot(relative, player.Forward);
                float vertical = Vector3.Dot(relative, player.Up);

                if (MathF.Abs(horizontal) > scannerRange || MathF.Abs(depth) > scannerRange)
                {
                    continue;
                }

                float dotX = scannerX + scannerWidth * 0.5f + horizontal / scannerRange * scannerWidth * 0.5f;
                float dotY = scannerY + scannerHeight * 0.5f - depth / scannerRange * scannerHeight * 0.5f - Math.Clamp(vertical / scannerRange, -0.25f, 0.25f) * scannerHeight;
                JollyColor dotColor = worldObject.Model is AsteroidModel ? JollyColor.Yellow : JollyColor.Red;
                DrawDashboardRect(sprites, dotX - 1.0f, dotY - 1.0f, 3.0f, 3.0f, dotColor);
            }
        }

        private void DrawEnergyBanks(List<JollySprite> sprites, float x, int row, int energy)
        {
            int remaining = Math.Clamp(energy, 0, 100) * 64 / 100;

            for (int bank = 3; bank >= 0; bank--)
            {
                int value = Math.Min(remaining, 16);
                remaining -= value;
                JollyColor color = value >= 12 ? JollyColor.Red : JollyColor.White;
                DrawDashboardRect(sprites, x, (row + bank) * 8, value * 2, 3, color);
            }
        }

        private void DrawMissileIndicators(List<JollySprite> sprites, int missiles)
        {
            int missileCount = Math.Clamp(missiles, 0, 4);

            for (int missile = 1; missile <= 4; missile++)
            {
                if (missile > missileCount)
                {
                    continue;
                }

                DrawDashboardRect(sprites, 48 - 8 * missile, 47, 6, 5, JollyColor.Yellow);
            }
        }

        private void DrawTemperatureBar(List<JollySprite> sprites, float x, int row, int temperature)
        {
            int value = Math.Clamp(temperature >> 4, 0, 16);
            JollyColor color = value >= 11 ? new JollyColor(255, 0, 0, 255) : JollyColor.White;
            DrawDashboardRect(sprites, x, row * 8 + 2, value * 2, 3, color);
        }
    }
}

