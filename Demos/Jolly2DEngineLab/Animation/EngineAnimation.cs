using Jolly2DEngine.Core;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Graphics.Scene;
using Jolly2DEngine.Math;
using Jolly2DEngine.Rendering.DirectX;
using System.Numerics;

namespace Jolly2DEngineLab.Animation;

/// <summary>
/// First mechanical animation for Jolly Engine Lab.
/// Add this to a WPF game project that references Jolly2DEngine.
/// </summary>
public sealed class EngineAnimation : JollyGame
{
    private const float VirtualWidth = 1280.0f;
    private const float VirtualHeight = 720.0f;
    private const float BaseZoom = 1.0f;
    
    private readonly DirectXJollyRenderer _renderer;
    private EngineAnimationScene? _scene;
    private volatile float _rpm = 100.0f;
    private volatile float _strokeMm = 56.0f;
    private volatile bool _paused;

    public EngineAnimation(DirectXControl animationView)
    {
        _renderer = new DirectXJollyRenderer(animationView);
        GameLoopFPS = 480.0;
        RenderFPS = 120.0;
        _renderer.ClearColor = System.Windows.Media.Color.FromRgb(12, 17, 24);
        _renderer.LineGlowEnabled = false;
        _renderer.Camera.SetViewport(VirtualWidth, VirtualHeight);
        _renderer.Camera.Position = new Vector2(VirtualWidth * 0.5f, VirtualHeight * 0.5f);
        _renderer.Camera.Zoom = BaseZoom;

        animationView.RenderSizeChanged += AnimationView_RenderSizeChanged;
    }

    private void AnimationView_RenderSizeChanged(float width, float height)
    {
        if (width <= 0.0f || height <= 0.0f)
        {
            return;
        }

        _renderer.Camera.SetViewport(width, height);

        UpdateCameraScale();
    }

    private void UpdateCameraScale()
    {
        float viewportWidth = _renderer.Camera.ViewportSize.X;
        float viewportHeight = _renderer.Camera.ViewportSize.Y;

        float scaleX = viewportWidth / VirtualWidth;
        float scaleY = viewportHeight / VirtualHeight;

        _renderer.Camera.Zoom = MathF.Min(scaleX, scaleY);
    }

    private volatile float _boreMm = 86.0f;

    public float BoreMm
    {
        get => _boreMm;
        set => _boreMm = Math.Clamp(value, 30.0f, 150.0f);
    }

    public float StrokeMm
    {
        get => _strokeMm;
        set => _strokeMm = System.Math.Clamp(value, 10.0f, 100.0f);
    }

    public float Rpm
    {
        get => _rpm;
        set => _rpm = System.Math.Clamp(value, 30.0f, 6000.0f);
    }

    public bool Paused
    {
        get => _paused;
        set => _paused = value;
    }

    public float CrankAngleDegrees => _scene?.CrankAngleDegrees ?? 0.0f;
    public float CrankRadiusMm => StrokeMm * 0.5f;

    protected override void OnInitialize()
    {
        _scene = new EngineAnimationScene(_renderer, () => Rpm, () => StrokeMm, () => BoreMm, () => Paused);
        Scenes.ChangeScene(_scene);
    }

    protected override void OnUpdate(JollyGameTime gameTime)
    {
        EngineAnimationScene? scene = _scene;

        if (scene == null)
        {
            return;
        }

        _renderer.SubmitFrame(new JollyRenderFrame(scene.GetSprites()));
    }

    protected override void OnShutdown()
    {
        _scene = null;
    }
}


internal sealed class EngineAnimationScene : JollyScene
{
    private const float CentreX = 640.0f;
    private const float CentreY = 550.0f;
    private const float PixelsPerMillimetre = 1.4f;

    private const float PistonHeight = 60.0f;
    private const float FlywheelDiameterToStrokeRatio = 2.5f;
    private const float CrankPinHoleDiameterToStrokeRatio = 0.28f;
    private const float CentreHoleDiameterToStrokeRatio = 0.32f;
    private const float CrankPinDistanceFromEdgeMm = 10.0f;
    private const float ConnectingRodLengthToStrokeRatio = 1.3f;
    private const float CylinderHeightToStrokeRatio = 1.8f;
    private const float CylinderWidthToStrokeRatio = 1.2f;
    private const float CylinderBottomClearanceMm = 5.0f;
    private const float CylinderHeadClearanceMm = 5.0f;
    private const float CylinderTopOffsetMm = 10.0f;
    private const int RodSegmentCount = 32;
    private const float RodBigEndWidth = 20.0f;
    private const float RodSmallEndWidth = 10.0f;
    private const float RodSegmentOverlap = 1.5f;

    private static readonly JollyColor TextureColour = JollyColor.FromBytes(255, 255, 255);

    private static readonly JollyColor BackgroundColour = JollyColor.FromBytes(12, 17, 24);

    private static readonly JollyColor RodColour = JollyColor.FromBytes(150, 165, 180);

    private readonly Func<float> _getRpm;
    private readonly Func<float> _getStrokeMm;
    private readonly Func<float> _getBoreMm;
    private readonly Func<bool> _getPaused;

    private readonly JollySpriteObject _flywheel;
    private readonly JollySpriteObject _centreHole;
    private readonly JollySpriteObject _crankPinHole;
    private readonly JollySpriteObject[] _connectingRodSegments;
    private readonly JollySpriteObject _connectingRodBigEnd;
    private readonly JollySpriteObject _connectingRodSmallEnd;
    private readonly JollySpriteObject _bigEndPinHole;
    private readonly JollySpriteObject _pistonPinHole;
    private readonly JollySpriteObject _piston;
    private readonly JollySpriteObject _pistonCrown;
    private readonly JollySpriteObject _cylinderBore;
    private readonly JollySpriteObject _cylinderLeftWall;
    private readonly JollySpriteObject _cylinderRightWall;
    private readonly JollySpriteObject _cylinderTop;
    private readonly JollySpriteObject _cylinderDome;
    private readonly JollySpriteObject _pistonDome;

    private float _angleDegrees;

    private float StrokeMm => Math.Clamp(_getStrokeMm(), 10.0f, 100.0f);
    private float BoreMm => Math.Clamp(_getBoreMm(), 30.0f, 150.0f);
    private float FlywheelDiameter => (StrokeMm + CrankPinDistanceFromEdgeMm * 2.0f) * PixelsPerMillimetre;
    private float CrankRadiusPixels => StrokeMm * 0.5f * PixelsPerMillimetre;
    private float CrankPinHoleDiameter => StrokeMm * CrankPinHoleDiameterToStrokeRatio * PixelsPerMillimetre;
    private float CentreHoleDiameter => StrokeMm * CentreHoleDiameterToStrokeRatio * PixelsPerMillimetre;

    private float PistonWidth => BoreMm * PixelsPerMillimetre;
    private float CylinderWidth => PistonWidth + CylinderBoreClearance;

    private float CylinderHeight => StrokeMm * CylinderHeightToStrokeRatio * PixelsPerMillimetre;
    private const float CylinderBoreClearance = 2.0f;
    private float FlywheelTopY => CentreY - FlywheelDiameter * 0.5f;

    private float CylinderBottomY => FlywheelTopY - CylinderBottomClearanceMm * PixelsPerMillimetre;
    private float CylinderTopY => CylinderBottomY - CylinderHeight;
    private float CylinderCentreY => (CylinderTopY + CylinderBottomY) * 0.5f;
    public float CrankAngleDegrees => _angleDegrees;

    public EngineAnimationScene(DirectXJollyRenderer renderer, Func<float> getRpm, Func<float> getStrokeMm, Func<float> getBoreMm, Func<bool> getPaused)
    {
        _getRpm = getRpm;
        _getStrokeMm = getStrokeMm;
        _getBoreMm = getBoreMm;
        _getPaused = getPaused;

        Name = "Flywheel and Tapered Connecting Rod";

        JollyTexture discTexture = renderer.CreateCircleTexture(100, 116, 130, 255, 256);
        JollyTexture holeTexture = renderer.CreateCircleTexture(12, 17, 24, 255, 64);
        JollyTexture rodTexture = renderer.CreateSolidTexture(150, 165, 180);
        JollyTexture rodEndTexture = renderer.CreateCircleTexture(150, 165, 180, 255, 64);
        JollyTexture pinHoleTexture = renderer.CreateCircleTexture(12, 17, 24, 255, 32);

        _flywheel = AddCircle(renderer, discTexture, "Solid flywheel", CentreX, CentreY, FlywheelDiameter, TextureColour, 1);

        _centreHole = AddCircle(renderer, holeTexture, "Centre shaft hole", CentreX, CentreY, CentreHoleDiameter, TextureColour, 2);
        _crankPinHole = AddCircle(renderer, holeTexture, "Flywheel crank pin recess", CentreX, CentreY, CrankPinHoleDiameter, TextureColour, 2);

        _connectingRodSegments = new JollySpriteObject[RodSegmentCount];

        for (int i = 0; i < RodSegmentCount; i++)
        {
            _connectingRodSegments[i] = AddRectangle(renderer, rodTexture, $"Connecting rod segment {i + 1}", CentreX, CentreY, RodSmallEndWidth, 1.0f, RodColour, 3);
        }

        _connectingRodBigEnd = AddCircle(renderer, rodEndTexture, "Connecting rod big end", CentreX, CentreY, 34.0f, TextureColour, 4);
        _connectingRodSmallEnd = AddCircle(renderer, rodEndTexture, "Connecting rod small end", CentreX, CentreY, 16.0f, TextureColour, 4);

        _bigEndPinHole = AddCircle(renderer, pinHoleTexture, "Connecting rod big-end pin hole", CentreX, CentreY, 16.0f, TextureColour, 5);
        _pistonPinHole = AddCircle(renderer, pinHoleTexture, "Piston pin hole", CentreX, CentreY, 7.0f, TextureColour, 5);

        JollyTexture cylinderWallTexture = renderer.CreateSolidTexture(85, 98, 110);
        JollyTexture cylinderBoreTexture = renderer.CreateSolidTexture(30, 38, 46);

        _cylinderBore = AddRectangle(renderer, cylinderBoreTexture, "Cylinder bore", CentreX, CylinderCentreY, CylinderWidth, CylinderHeight, TextureColour, 2);
        _cylinderLeftWall = AddRectangle(renderer, cylinderWallTexture, "Cylinder left wall", CentreX - CylinderWidth * 0.5f - 6.0f, CylinderCentreY, 12.0f, CylinderHeight, TextureColour, 3);
        _cylinderRightWall = AddRectangle(renderer, cylinderWallTexture, "Cylinder right wall", CentreX + CylinderWidth * 0.5f + 6.0f, CylinderCentreY, 12.0f, CylinderHeight, TextureColour, 3);

        JollyTexture cylinderHeadTexture = renderer.CreateTextureFromBgraPixels(106, 30, CreateCylinderHeadArchPixels(106, 30, 85, 98, 110));

        _cylinderDome = AddRectangle(renderer, cylinderHeadTexture, "Domed cylinder head", CentreX, CylinderTopY + 10, CylinderWidth + 24, 40.0f, TextureColour, 4);

        JollyTexture pistonTexture = renderer.CreateSolidTexture(175, 185, 195);
        JollyTexture pistonCrownTexture = renderer.CreateSolidTexture(115, 128, 140);

        _piston = AddRectangle(renderer, pistonTexture, "Piston body", CentreX, CentreY, 70.0f, 60.0f, TextureColour, 3);
        _pistonCrown = AddRectangle(renderer, pistonCrownTexture, "Piston crown", CentreX, CentreY, 70.0f, 8.0f, TextureColour, 4);

        UpdateCrankPinPosition();
    }



    private static byte[] CreateCylinderHeadArchPixels(int width, int height, byte red, byte green, byte blue)
    {
        byte[] pixels = new byte[checked(width * height * 4)];

        float centreX = (width - 1) * 0.5f;
        float halfWidth = centreX;
        float outerRise = height * 0.20f;
        float innerRise = MathF.Max(0.0f, outerRise - 5.0f);
        float wallThickness = 5.0f;

        for (int x = 0; x < width; x++)
        {
            float nx = (x - centreX) / halfWidth;
            float curve = MathF.Sqrt(MathF.Max(0.0f, 1.0f - nx * nx));

            float outerTop = outerRise * (1.0f - curve);
            float innerBottom = outerTop + wallThickness + innerRise * (1.0f - curve);

            for (int y = 0; y < height; y++)
            {
                if (y < outerTop || y > innerBottom)
                {
                    continue;
                }

                int index = (y * width + x) * 4;
                pixels[index] = blue;
                pixels[index + 1] = green;
                pixels[index + 2] = red;
                pixels[index + 3] = 255;
            }
        }

        return pixels;
    }

    public override void Update(JollyGameTime gameTime)
    {
        base.Update(gameTime);

        if (!_getPaused())
        {
            _angleDegrees = (_angleDegrees + _getRpm() * 6.0f * (float)gameTime.DeltaSeconds) % 360.0f;
        }

        UpdateCrankPinPosition();
    }

    private void UpdateCrankPinPosition()
    {
        float angleRadians = _angleDegrees * (MathF.PI / 180.0f);

        float crankRadius = CrankRadiusPixels;
        float crankX = CentreX + MathF.Sin(angleRadians) * crankRadius;
        float crankY = CentreY - MathF.Cos(angleRadians) * crankRadius;

        float rodLength = StrokeMm * ConnectingRodLengthToStrokeRatio * PixelsPerMillimetre;
        float horizontalOffset = crankX - CentreX;

        float verticalOffset = MathF.Sqrt(MathF.Max(0.0f, rodLength * rodLength - horizontalOffset * horizontalOffset));

        float pistonX = CentreX;
        float pistonY = crankY - verticalOffset;
        float wristPinOffset = 25.0f;
        float wristPinY = pistonY - wristPinOffset;

        float deltaX = pistonX - crankX;
        float deltaY = wristPinY - crankY;

        float actualRodLength = MathF.Sqrt(deltaX * deltaX + deltaY * deltaY);

        float rodRotation = MathF.Atan2(deltaY, deltaX) * (180.0f / MathF.PI) + 90.0f;
        float segmentLength = actualRodLength / RodSegmentCount + RodSegmentOverlap;

        for (int i = 0; i < RodSegmentCount; i++)
        {
            float t = (i + 0.5f) / RodSegmentCount;

            float segmentX = crankX + deltaX * t;
            float segmentY = crankY + deltaY * t;

            float segmentWidth = RodBigEndWidth + (RodSmallEndWidth - RodBigEndWidth) * t;

            JollySpriteObject segment = _connectingRodSegments[i];

            segment.Position = new JollyVector2(segmentX, segmentY);
            segment.Sprite.Width = segmentWidth;
            segment.Sprite.Height = segmentLength;
            segment.Sprite.Rotation = rodRotation;
        }

        _connectingRodBigEnd.Position = new JollyVector2(crankX, crankY);
        _connectingRodSmallEnd.Position = new JollyVector2(pistonX, wristPinY);

        _bigEndPinHole.Position = new JollyVector2(crankX, crankY);
        _pistonPinHole.Position = new JollyVector2(pistonX, wristPinY);

        _crankPinHole.Position = new JollyVector2(crankX, crankY);

        _piston.Position = new JollyVector2(pistonX, pistonY - 30.0f);
        _piston.Sprite.Width = PistonWidth;

        _pistonCrown.Position = new JollyVector2(pistonX, pistonY - 4.0f);
        _pistonCrown.Sprite.Width = PistonWidth;

        _pistonPinHole.Position = new JollyVector2(pistonX, wristPinY);

        UpdateEngineGeometry(); 
    }

    private void UpdateEngineGeometry()
    {
        float flywheelDiameter = FlywheelDiameter;

        _flywheel.Sprite.Width = flywheelDiameter;
        _flywheel.Sprite.Height = flywheelDiameter;

        _centreHole.Sprite.Width = CentreHoleDiameter;
        _centreHole.Sprite.Height = CentreHoleDiameter;

        _crankPinHole.Sprite.Width = CrankPinHoleDiameter;
        _crankPinHole.Sprite.Height = CrankPinHoleDiameter;

        UpdateCylinderPosition();
    }

    private void UpdateCylinderPosition()
    {
        float cylinderHeight = CylinderHeight;
        float cylinderWidth = CylinderWidth;
        float cylinderCentreY = CylinderCentreY;

        _cylinderBore.Position = new JollyVector2(CentreX, cylinderCentreY);
        _cylinderBore.Sprite.Width = cylinderWidth;
        _cylinderBore.Sprite.Height = cylinderHeight;

        _cylinderLeftWall.Position = new JollyVector2(CentreX - cylinderWidth * 0.5f - 6.0f, cylinderCentreY);
        _cylinderLeftWall.Sprite.Height = cylinderHeight;

        _cylinderRightWall.Position = new JollyVector2(CentreX + cylinderWidth * 0.5f + 6.0f, cylinderCentreY);
        _cylinderRightWall.Sprite.Height = cylinderHeight;

        _cylinderDome.Position = new JollyVector2(CentreX, CylinderTopY + 10.0f);
    }

    private JollySpriteObject AddCircle(DirectXJollyRenderer renderer, JollyTexture texture, string name, float centreX, float centreY, float diameter, JollyColor colour, int layer)
    {
        JollySprite sprite = renderer.CreateSprite(texture);

        sprite.Name = name;
        sprite.Width = diameter;
        sprite.Height = diameter;
        sprite.Color = colour;
        sprite.Layer = layer;

        JollySpriteObject gameObject = new(sprite)
        {
            Name = name,
            Position = new JollyVector2(centreX, centreY)
        };

        Add(gameObject);

        return gameObject;
    }

    private JollySpriteObject AddRectangle(DirectXJollyRenderer renderer, JollyTexture texture, string name, float centreX, float centreY, float width, float height, JollyColor colour, int layer)
    {
        JollySprite sprite = renderer.CreateSprite(texture);

        sprite.Name = name;
        sprite.Width = width;
        sprite.Height = height;
        sprite.Color = colour;
        sprite.Layer = layer;

        JollySpriteObject gameObject = new(sprite)
        {
            Name = name,
            Position = new JollyVector2(centreX, centreY)
        };

        Add(gameObject);

        return gameObject;
    }
}