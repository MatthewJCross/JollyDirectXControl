using Jolly2DEngine.Core;
using Jolly2DEngine.Graphics;
using Jolly2DEngine.Graphics.Scene;
using Jolly2DEngine.Math;
using Jolly2DEngine.Physics;
using Jolly2DEngine.Rendering;
using Jolly2DEngine.Rendering.DirectX;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Jolly2DEditor
{
    public partial class MainWindow : Window
    {
        private sealed class EditorScene : JollyScene
        {
        }

        private enum EditorTool
        {
            Select,
            Move,
            Rotate,
            Scale
        }

        private EditorTool _currentTool = EditorTool.Select;
        private EditorTool _activeGizmoTool = EditorTool.Select;

        private readonly EditorScene _scene;
        private readonly DirectXJollyRenderer _renderer;
        private readonly DispatcherTimer _renderTimer;

        private JollyGameObject? _selectedObject;
        private bool _updatingInspector;

        private System.Windows.Point _lastMousePosition;
        private bool _viewportMouseDown;

        private JollyGameObject? _dragObject;
        private System.Numerics.Vector2 _dragStartWorldPosition;
        private JollyVector2 _dragStartObjectPosition;
        private bool _draggingObject;

        private bool _gizmoDragging;
        private System.Numerics.Vector2 _gizmoStartWorldPosition;
        private JollyVector2 _gizmoStartObjectPosition;
        private JollyVector2 _gizmoStartScale;
        private float _gizmoStartRotation;
        private float _gizmoStartAngle;
        private float _gizmoStartDistance;

        private JollyTexture? _editorGridTexture;
        private JollyTexture? _editorSelectionTexture;

        private JollyTexture? _editorMoveGizmoTexture;
        private JollyTexture? _editorRotateGizmoTexture;
        private JollyTexture? _editorScaleGizmoTexture;

        private readonly List<JollySprite> _editorGridSprites = [];
        private readonly List<JollySprite> _editorSelectionSprites = [];
        private readonly List<JollySprite> _editorGizmoSprites = [];
        private readonly List<JollySprite> _editorOverlaySprites = [];

        private bool _gridVisible = true;

        private const float EditorGridSpacing = 32.0f;
        private const int EditorGridLayer = -100000;
        private const int EditorSelectionLayer = 100000;

        private bool _collisionEditing;
        private readonly List<JollySprite> _editorCollisionSprites = [];
        private JollyTexture? _editorCollisionTexture;
        private const int EditorCollisionLayer = 110000;

        private bool _collisionDragging;
        private int _collisionHandle = -1;
        private float _collisionStartWidth;
        private float _collisionStartHeight;
        private JollyVector2 _collisionStartScale;
        private float _collisionHandleSize = 10.0f;

        private bool _circleCollisionDragging;
        private float _circleCollisionStartRadius;
        private JollyVector2 _circleCollisionStartScale;

        public MainWindow()
        {
            InitializeComponent();

            _scene = new EditorScene
            {
                Name = "Untitled"
            };

            _renderer = new DirectXJollyRenderer(SceneView);
            _renderer.Camera.Position = System.Numerics.Vector2.Zero;
            _renderer.Camera.Zoom = 1.0f;
            _renderer.Camera.Rotation = 0.0f;

            SceneView.RenderSizeChanged += SceneView_RenderSizeChanged;
            SceneView.MouseScreenPositionChanged += SceneView_MouseScreenPositionChanged;
            SceneView.MouseButtonDown += SceneView_MouseButtonDown;
            SceneView.MouseButtonUp += SceneView_MouseButtonUp;

            _renderTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };

            _renderTimer.Tick += RenderTimer_Tick;

            SceneStatus.Content = $"Scene: {_scene.Name}";

            UpdateViewportStatus();
            PopulateSceneTree();
            ShowNoSelection();
        }

        private void EnsureEditorTextures()
        {
            if (_editorGridTexture == null)
            {
                _editorGridTexture = _renderer.CreateSolidTexture(80, 80, 80, 255);
            }

            if (_editorSelectionTexture == null)
            {
                _editorSelectionTexture = _renderer.CreateSolidTexture(255, 220, 0, 255);
            }
        }

        private void EnsureEditorGizmoTextures()
        {
            if (_editorMoveGizmoTexture == null)
            {
                _editorMoveGizmoTexture = _renderer.CreateSolidTexture(255, 70, 70, 255);
            }

            if (_editorRotateGizmoTexture == null)
            {
                _editorRotateGizmoTexture = _renderer.CreateSolidTexture(0, 120, 255, 255);
            }

            if (_editorScaleGizmoTexture == null)
            {
                _editorScaleGizmoTexture = _renderer.CreateSolidTexture(255, 210, 40, 255);
            }
        }

        private void EnsureSelectionSprites()
        {
            EnsureEditorTextures();

            while (_editorSelectionSprites.Count < 4)
            {
                JollySprite sprite = _renderer.CreateSprite(_editorSelectionTexture!);

                sprite.Layer = EditorSelectionLayer;
                sprite.Visible = false;
                sprite.Opacity = 1.0f;

                _editorSelectionSprites.Add(sprite);
            }
        }

        private void EnsureGridSprites(int requiredCount)
        {
            EnsureEditorTextures();

            while (_editorGridSprites.Count < requiredCount)
            {
                JollySprite sprite = _renderer.CreateSprite(_editorGridTexture!);

                sprite.Layer = EditorGridLayer;
                sprite.Visible = false;

                _editorGridSprites.Add(sprite);
            }
        }

        private void EnsureGizmoSprites(int requiredCount)
        {
            EnsureEditorGizmoTextures();

            while (_editorGizmoSprites.Count < requiredCount)
            {
                JollySprite sprite;

                if (_editorGizmoSprites.Count < 4)
                {
                    sprite = _renderer.CreateSprite(_editorMoveGizmoTexture!);
                }
                else if (_editorGizmoSprites.Count == 4)
                {
                    sprite = _renderer.CreateSprite(_editorRotateGizmoTexture!);
                }
                else if (_editorGizmoSprites.Count == 5)
                {
                    sprite = _renderer.CreateSprite(_editorScaleGizmoTexture!);
                }
                else
                {
                    sprite = _renderer.CreateSprite(_editorRotateGizmoTexture!);
                }

                sprite.Layer = EditorSelectionLayer + 20;
                sprite.Visible = false;
                sprite.Opacity = 1.0f;

                _editorGizmoSprites.Add(sprite);
            }
        }

        private void EnsureCollisionSprites(int requiredCount)
        {
            if (_editorCollisionTexture == null)
            {
                _editorCollisionTexture = _renderer.CreateSolidTexture(255, 0, 0, 255);
            }

            while (_editorCollisionSprites.Count < requiredCount)
            {
                JollySprite sprite = _renderer.CreateSprite(_editorCollisionTexture);

                sprite.Layer = EditorCollisionLayer;
                sprite.Visible = false;
                sprite.Opacity = 1.0f;

                _editorCollisionSprites.Add(sprite);
            }
        }

        private void HideGridSprites()
        {
            for (int i = 0; i < _editorGridSprites.Count; i++)
            {
                _editorGridSprites[i].Visible = false;
            }
        }

        private void HideSelectionSprites()
        {
            for (int i = 0; i < _editorSelectionSprites.Count; i++)
            {
                _editorSelectionSprites[i].Visible = false;
            }
        }

        private void HideGizmoSprites()
        {
            for (int i = 0; i < _editorGizmoSprites.Count; i++)
            {
                _editorGizmoSprites[i].Visible = false;
            }
        }

        private void HideCollisionSprites()
        {
            foreach (JollySprite sprite in _editorCollisionSprites)
            {
                sprite.Visible = false;
            }
        }

        private void Grid_Click(object sender, RoutedEventArgs e)
        {
            _gridVisible = !_gridVisible;
            GridButton.Content = _gridVisible ? "Grid: On" : "Grid: Off";
            ReadyStatus.Content = _gridVisible ? "Grid enabled" : "Grid disabled";
        }

        private void BuildEditorOverlay()
        {
            EnsureEditorTextures();

            _editorOverlaySprites.Clear();

            if (_gridVisible)
            {
                BuildGridOverlay();
            }
            else
            {
                HideGridSprites();
            }

            BuildSelectionOverlay();
            BuildTransformGizmo();
            BuildCollisionOverlay();

            _editorOverlaySprites.AddRange(_editorGridSprites);
            _editorOverlaySprites.AddRange(_editorSelectionSprites);
            _editorOverlaySprites.AddRange(_editorGizmoSprites);
            _editorOverlaySprites.AddRange(_editorCollisionSprites);
        }

        private void BuildGridOverlay()
        {
            System.Numerics.Vector2 viewport = _renderer.Camera.ViewportSize;

            if (viewport.X <= 0.0f || viewport.Y <= 0.0f)
            {
                HideGridSprites();
                return;
            }

            System.Numerics.Vector2 topLeft = _renderer.Camera.ScreenToWorld(System.Numerics.Vector2.Zero);
            System.Numerics.Vector2 topRight = _renderer.Camera.ScreenToWorld(new System.Numerics.Vector2(viewport.X, 0.0f));
            System.Numerics.Vector2 bottomLeft = _renderer.Camera.ScreenToWorld(new System.Numerics.Vector2(0.0f, viewport.Y));
            System.Numerics.Vector2 bottomRight = _renderer.Camera.ScreenToWorld(viewport);

            float minX = MathF.Min(MathF.Min(topLeft.X, topRight.X), MathF.Min(bottomLeft.X, bottomRight.X));

            float maxX = MathF.Max(
                MathF.Max(topLeft.X, topRight.X),
                MathF.Max(bottomLeft.X, bottomRight.X));

            float minY = MathF.Min(
                MathF.Min(topLeft.Y, topRight.Y),
                MathF.Min(bottomLeft.Y, bottomRight.Y));

            float maxY = MathF.Max(
                MathF.Max(topLeft.Y, topRight.Y),
                MathF.Max(bottomLeft.Y, bottomRight.Y));

            float startX = MathF.Floor(minX / EditorGridSpacing) * EditorGridSpacing;
            float startY = MathF.Floor(minY / EditorGridSpacing) * EditorGridSpacing;

            int verticalCount = Math.Max(
                0,
                (int)MathF.Ceiling((maxX - startX) / EditorGridSpacing) + 1);

            int horizontalCount = Math.Max(
                0,
                (int)MathF.Ceiling((maxY - startY) / EditorGridSpacing) + 1);

            EnsureGridSprites(verticalCount + horizontalCount);

            float width = maxX - minX;
            float height = maxY - minY;
            float centreX = (minX + maxX) * 0.5f;
            float centreY = (minY + maxY) * 0.5f;

            int spriteIndex = 0;

            for (int i = 0; i < verticalCount; i++)
            {
                float x = startX + i * EditorGridSpacing;

                JollySprite line = _editorGridSprites[spriteIndex++];

                line.Position = new JollyVector2(x, centreY);
                line.Size = new JollyVector2(1.0f, height);
                line.Rotation = 0.0f;
                line.Layer = EditorGridLayer;
                line.Opacity = 0.35f;
                line.Visible = true;
            }

            for (int i = 0; i < horizontalCount; i++)
            {
                float y = startY + i * EditorGridSpacing;

                JollySprite line = _editorGridSprites[spriteIndex++];

                line.Position = new JollyVector2(centreX, y);
                line.Size = new JollyVector2(width, 1.0f);
                line.Rotation = 0.0f;
                line.Layer = EditorGridLayer;
                line.Opacity = 0.35f;
                line.Visible = true;
            }

            for (int i = spriteIndex; i < _editorGridSprites.Count; i++)
            {
                _editorGridSprites[i].Visible = false;
            }
        }

        private void BuildSelectionOverlay()
        {
            EnsureSelectionSprites();

            if (_selectedObject is not JollySpriteObject spriteObject)
            {
                HideSelectionSprites();
                return;
            }

            JollySprite sprite = spriteObject.Sprite;

            if (!sprite.Visible)
            {
                HideSelectionSprites();
                return;
            }

            float width = MathF.Abs(sprite.Width * sprite.Scale.X);
            float height = MathF.Abs(sprite.Height * sprite.Scale.Y);

            if (width <= 0.0f || height <= 0.0f)
            {
                HideSelectionSprites();
                return;
            }

            float thickness = MathF.Max(2.0f / MathF.Max(0.01f, _renderer.Camera.Zoom), 1.0f);
            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;
            float rotationRadians = sprite.Rotation * MathF.PI / 180.0f;
            System.Numerics.Matrix3x2 rotationMatrix = System.Numerics.Matrix3x2.CreateRotation(rotationRadians);
            System.Numerics.Vector2 topOffset = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(0.0f, -halfHeight), rotationMatrix);
            System.Numerics.Vector2 bottomOffset = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(0.0f, halfHeight), rotationMatrix);
            System.Numerics.Vector2 leftOffset = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(-halfWidth, 0.0f), rotationMatrix);
            System.Numerics.Vector2 rightOffset = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(halfWidth, 0.0f), rotationMatrix);

            JollySprite top = _editorSelectionSprites[0];
            top.Position = new JollyVector2(sprite.Position.X + topOffset.X, sprite.Position.Y + topOffset.Y);
            top.Size = new JollyVector2(width, thickness);
            top.Rotation = sprite.Rotation;
            top.Scale = JollyVector2.One;
            top.Layer = EditorSelectionLayer;
            top.Opacity = 1.0f;
            top.Visible = true;

            JollySprite bottom = _editorSelectionSprites[1];
            bottom.Position = new JollyVector2(sprite.Position.X + bottomOffset.X, sprite.Position.Y + bottomOffset.Y);
            bottom.Size = new JollyVector2(width, thickness);
            bottom.Rotation = sprite.Rotation;
            bottom.Scale = JollyVector2.One;
            bottom.Layer = EditorSelectionLayer;
            bottom.Opacity = 1.0f;
            bottom.Visible = true;

            JollySprite left = _editorSelectionSprites[2];
            left.Position = new JollyVector2(sprite.Position.X + leftOffset.X, sprite.Position.Y + leftOffset.Y);
            left.Size = new JollyVector2(thickness, height);
            left.Rotation = sprite.Rotation;
            left.Scale = JollyVector2.One;
            left.Layer = EditorSelectionLayer;
            left.Opacity = 1.0f;
            left.Visible = true;

            JollySprite right = _editorSelectionSprites[3];
            right.Position = new JollyVector2(sprite.Position.X + rightOffset.X, sprite.Position.Y + rightOffset.Y);
            right.Size = new JollyVector2(thickness, height);
            right.Rotation = sprite.Rotation;
            right.Scale = JollyVector2.One;
            right.Layer = EditorSelectionLayer;
            right.Opacity = 1.0f;
            right.Visible = true;
        }

        private void BuildCollisionOverlay()
        {
            HideCollisionSprites();

            if (!_collisionEditing)
            {
                return;
            }

            if (_selectedObject is not JollySpriteObject spriteObject)
            {
                return;
            }

            if (!spriteObject.Active || spriteObject.IsDestroyed || !spriteObject.Visible)
            {
                return;
            }

            JollyCollider? collider = spriteObject.Collider;

            if (collider == null || !collider.Enabled)
            {
                return;
            }

            switch (collider.Shape)
            {
                case JollyColliderShape.Rectangle:
                    BuildRectangleCollisionOverlay(spriteObject);
                    break;

                case JollyColliderShape.Circle:
                    BuildCircleCollisionOverlay(spriteObject);
                    break;

                case JollyColliderShape.Pixel:
                    BuildPixelCollisionOverlay(spriteObject);
                    break;
            }
        }

        private void BuildTransformGizmo()
        {
            HideGizmoSprites();

            if (_selectedObject is not JollySpriteObject spriteObject)
            {
                return;
            }

            if (!spriteObject.Active || spriteObject.IsDestroyed || !spriteObject.Visible)
            {
                return;
            }

            switch (_currentTool)
            {
                case EditorTool.Move:
                    BuildMoveGizmo(spriteObject);
                    break;

                case EditorTool.Rotate:
                    BuildRotateGizmo(spriteObject);
                    break;

                case EditorTool.Scale:
                    BuildScaleGizmo(spriteObject);
                    break;

                case EditorTool.Select:
                    BuildRotateGizmo(spriteObject);
                    BuildScaleGizmo(spriteObject);
                    break;
            }
        }

        private void BuildMoveGizmo(JollySpriteObject spriteObject)
        {
            EnsureGizmoSprites(4);

            JollySprite sprite = spriteObject.Sprite;

            float width = MathF.Abs(sprite.Width * sprite.Scale.X);
            float height = MathF.Abs(sprite.Height * sprite.Scale.Y);

            float length = MathF.Max(
                50.0f,
                MathF.Max(width, height) * 0.5f + 40.0f);

            float thickness = MathF.Max(
                4.0f / MathF.Max(0.01f, _renderer.Camera.Zoom),
                2.0f);

            JollySprite horizontal = _editorGizmoSprites[0];

            horizontal.Position = new JollyVector2(
                sprite.Position.X + length * 0.5f,
                sprite.Position.Y);

            horizontal.Size = new JollyVector2(length, thickness);
            horizontal.Rotation = 0.0f;
            horizontal.Opacity = 1.0f;
            horizontal.Layer = EditorSelectionLayer + 10;
            horizontal.Visible = true;

            JollySprite vertical = _editorGizmoSprites[1];

            vertical.Position = new JollyVector2(
                sprite.Position.X,
                sprite.Position.Y - length * 0.5f);

            vertical.Size = new JollyVector2(thickness, length);
            vertical.Rotation = 0.0f;
            vertical.Opacity = 1.0f;
            vertical.Layer = EditorSelectionLayer + 10;
            vertical.Visible = true;

            JollySprite horizontalHandle = _editorGizmoSprites[2];

            horizontalHandle.Position = new JollyVector2(
                sprite.Position.X + length,
                sprite.Position.Y);

            horizontalHandle.Size = new JollyVector2(20.0f, 20.0f);
            horizontalHandle.Rotation = 0.0f;
            horizontalHandle.Opacity = 1.0f;
            horizontalHandle.Layer = EditorSelectionLayer + 11;
            horizontalHandle.Visible = true;

            JollySprite verticalHandle = _editorGizmoSprites[3];

            verticalHandle.Position = new JollyVector2(
                sprite.Position.X,
                sprite.Position.Y - length);

            verticalHandle.Size = new JollyVector2(20.0f, 20.0f);
            verticalHandle.Rotation = 0.0f;
            verticalHandle.Opacity = 1.0f;
            verticalHandle.Layer = EditorSelectionLayer + 11;
            verticalHandle.Visible = true;
        }

        private void BuildRotateGizmo(JollySpriteObject spriteObject)
        {
            EnsureGizmoSprites(7);
            JollySprite sprite = spriteObject.Sprite;
            float width = MathF.Abs(sprite.Width * sprite.Scale.X);
            float height = MathF.Abs(sprite.Height * sprite.Scale.Y);
            float radius = MathF.Max(50.0f, MathF.Max(width, height) * 0.5f + 35.0f);
            float thickness = MathF.Max(3.0f / MathF.Max(0.01f, _renderer.Camera.Zoom), 2.0f);
            JollySprite line = _editorGizmoSprites[4];
            line.Position = new JollyVector2(sprite.Position.X, sprite.Position.Y - radius * 0.5f);
            line.Size = new JollyVector2(thickness, radius);
            line.Scale = JollyVector2.One;
            line.Rotation = 0.0f;
            line.Opacity = 1.0f;
            line.Layer = EditorSelectionLayer + 10;
            line.Visible = true;
            JollySprite handle = _editorGizmoSprites[6];
            handle.Position = new JollyVector2(sprite.Position.X, sprite.Position.Y - radius);
            handle.Size = new JollyVector2(18.0f, 18.0f);
            handle.Scale = JollyVector2.One;
            handle.Rotation = 0.0f;
            handle.Opacity = 1.0f;
            handle.Layer = EditorSelectionLayer + 11;
            handle.Visible = true;
        }

        private void BuildScaleGizmo(JollySpriteObject spriteObject)
        {
            EnsureGizmoSprites(6);

            JollySprite sprite = spriteObject.Sprite;

            float width = MathF.Abs(sprite.Width * sprite.Scale.X);
            float height = MathF.Abs(sprite.Height * sprite.Scale.Y);

            if (width <= 0.0f || height <= 0.0f)
            {
                return;
            }

            float rotationRadians = sprite.Rotation * MathF.PI / 180.0f;
            System.Numerics.Matrix3x2 rotation = System.Numerics.Matrix3x2.CreateRotation(rotationRadians);
            System.Numerics.Vector2 offset = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(width * 0.5f, height * 0.5f), rotation);
            JollySprite handle = _editorGizmoSprites[5];
            handle.Position = new JollyVector2(sprite.Position.X + offset.X, sprite.Position.Y + offset.Y);
            handle.Size = new JollyVector2(6.0f, 6.0f);
            handle.Scale = JollyVector2.One;
            handle.Rotation = sprite.Rotation;
            handle.Opacity = 1.0f;
            handle.Layer = EditorSelectionLayer + 50;
            handle.Visible = true;
        }

        private void BuildRectangleCollisionOverlay(JollySpriteObject spriteObject)
        {
            JollyCollider? collider = spriteObject.Collider;

            if (collider == null)
            {
                return;
            }

            EnsureCollisionSprites(8);

            float scaleX = MathF.Abs(spriteObject.Scale.X);
            float scaleY = MathF.Abs(spriteObject.Scale.Y);

            float width = MathF.Abs(collider.Width * scaleX);
            float height = MathF.Abs(collider.Height * scaleY);

            if (width <= 0.0f || height <= 0.0f)
            {
                return;
            }

            float thickness = MathF.Max(3.0f / MathF.Max(0.01f, _renderer.Camera.Zoom), 2.0f);
            float handleSize = MathF.Max(_collisionHandleSize / MathF.Max(0.01f, _renderer.Camera.Zoom), 6.0f);

            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            float rotationRadians = spriteObject.Rotation * MathF.PI / 180.0f;

            System.Numerics.Matrix3x2 rotation = System.Numerics.Matrix3x2.CreateRotation(rotationRadians);

            System.Numerics.Vector2 topLeftOffset = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(-halfWidth, -halfHeight), rotation);
            System.Numerics.Vector2 topRightOffset = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(halfWidth, -halfHeight), rotation);
            System.Numerics.Vector2 bottomLeftOffset = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(-halfWidth, halfHeight), rotation);
            System.Numerics.Vector2 bottomRightOffset = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(halfWidth, halfHeight), rotation);

            System.Numerics.Vector2 topOffset = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(0.0f, -halfHeight), rotation);
            System.Numerics.Vector2 bottomOffset = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(0.0f, halfHeight), rotation);
            System.Numerics.Vector2 leftOffset = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(-halfWidth, 0.0f), rotation);
            System.Numerics.Vector2 rightOffset = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(halfWidth, 0.0f), rotation);

            JollySprite top = _editorCollisionSprites[0];
            top.Position = new JollyVector2(spriteObject.Position.X + topOffset.X, spriteObject.Position.Y + topOffset.Y);
            top.Size = new JollyVector2(width, thickness);
            top.Rotation = spriteObject.Rotation;
            top.Scale = JollyVector2.One;
            top.Layer = EditorCollisionLayer;
            top.Visible = true;

            JollySprite bottom = _editorCollisionSprites[1];
            bottom.Position = new JollyVector2(spriteObject.Position.X + bottomOffset.X, spriteObject.Position.Y + bottomOffset.Y);
            bottom.Size = new JollyVector2(width, thickness);
            bottom.Rotation = spriteObject.Rotation;
            bottom.Scale = JollyVector2.One;
            bottom.Layer = EditorCollisionLayer;
            bottom.Visible = true;

            JollySprite left = _editorCollisionSprites[2];
            left.Position = new JollyVector2(spriteObject.Position.X + leftOffset.X, spriteObject.Position.Y + leftOffset.Y);
            left.Size = new JollyVector2(thickness, height);
            left.Rotation = spriteObject.Rotation;
            left.Scale = JollyVector2.One;
            left.Layer = EditorCollisionLayer;
            left.Visible = true;

            JollySprite right = _editorCollisionSprites[3];
            right.Position = new JollyVector2(spriteObject.Position.X + rightOffset.X, spriteObject.Position.Y + rightOffset.Y);
            right.Size = new JollyVector2(thickness, height);
            right.Rotation = spriteObject.Rotation;
            right.Scale = JollyVector2.One;
            right.Layer = EditorCollisionLayer;
            right.Visible = true;

            JollySprite topLeft = _editorCollisionSprites[4];
            topLeft.Position = new JollyVector2(spriteObject.Position.X + topLeftOffset.X, spriteObject.Position.Y + topLeftOffset.Y);
            topLeft.Size = new JollyVector2(handleSize, handleSize);
            topLeft.Rotation = 0.0f;
            topLeft.Scale = JollyVector2.One;
            topLeft.Layer = EditorCollisionLayer + 1;
            topLeft.Visible = true;

            JollySprite topRight = _editorCollisionSprites[5];
            topRight.Position = new JollyVector2(spriteObject.Position.X + topRightOffset.X, spriteObject.Position.Y + topRightOffset.Y);
            topRight.Size = new JollyVector2(handleSize, handleSize);
            topRight.Rotation = 0.0f;
            topRight.Scale = JollyVector2.One;
            topRight.Layer = EditorCollisionLayer + 1;
            topRight.Visible = true;

            JollySprite bottomLeft = _editorCollisionSprites[6];
            bottomLeft.Position = new JollyVector2(spriteObject.Position.X + bottomLeftOffset.X, spriteObject.Position.Y + bottomLeftOffset.Y);
            bottomLeft.Size = new JollyVector2(handleSize, handleSize);
            bottomLeft.Rotation = 0.0f;
            bottomLeft.Scale = JollyVector2.One;
            bottomLeft.Layer = EditorCollisionLayer + 1;
            bottomLeft.Visible = true;

            JollySprite bottomRight = _editorCollisionSprites[7];
            bottomRight.Position = new JollyVector2(spriteObject.Position.X + bottomRightOffset.X, spriteObject.Position.Y + bottomRightOffset.Y);
            bottomRight.Size = new JollyVector2(handleSize, handleSize);
            bottomRight.Rotation = 0.0f;
            bottomRight.Scale = JollyVector2.One;
            bottomRight.Layer = EditorCollisionLayer + 1;
            bottomRight.Visible = true;
        }

        private void BuildCircleCollisionOverlay(JollySpriteObject spriteObject)
        {
            JollyCollider? collider = spriteObject.Collider;

            if (collider == null)
            {
                return;
            }

            const int segmentCount = 32;
            int handleIndex = segmentCount;

            EnsureCollisionSprites(segmentCount + 1);

            float scale = MathF.Max(MathF.Abs(spriteObject.Scale.X), MathF.Abs(spriteObject.Scale.Y));
            float radius = MathF.Abs(collider.Radius * scale);

            if (radius <= 0.0f)
            {
                return;
            }

            float thickness = MathF.Max(3.0f / MathF.Max(0.01f, _renderer.Camera.Zoom), 2.0f);
            float handleSize = MathF.Max(10.0f / MathF.Max(0.01f, _renderer.Camera.Zoom), 6.0f);

            for (int i = 0; i < segmentCount; i++)
            {
                float startAngle = i * MathF.PI * 2.0f / segmentCount;
                float endAngle = (i + 1) * MathF.PI * 2.0f / segmentCount;

                System.Numerics.Vector2 start = new(MathF.Cos(startAngle) * radius, MathF.Sin(startAngle) * radius);
                System.Numerics.Vector2 end = new(MathF.Cos(endAngle) * radius, MathF.Sin(endAngle) * radius);

                System.Numerics.Vector2 midpoint = (start + end) * 0.5f;
                float length = System.Numerics.Vector2.Distance(start, end);
                float rotation = MathF.Atan2(end.Y - start.Y, end.X - start.X) * 180.0f / MathF.PI;

                JollySprite line = _editorCollisionSprites[i];

                line.Position = new JollyVector2(spriteObject.Position.X + midpoint.X, spriteObject.Position.Y + midpoint.Y);

                line.Size = new JollyVector2(length, thickness);
                line.Rotation = rotation;
                line.Scale = JollyVector2.One;
                line.Layer = EditorCollisionLayer;
                line.Opacity = 1.0f;
                line.Visible = true;
            }

            JollySprite handle = _editorCollisionSprites[handleIndex];

            float handleAngle = spriteObject.Rotation * MathF.PI / 180.0f;
            System.Numerics.Vector2 handleOffset = new(MathF.Cos(handleAngle) * radius, MathF.Sin(handleAngle) * radius);
            handle.Position = new JollyVector2(spriteObject.Position.X + handleOffset.X, spriteObject.Position.Y + handleOffset.Y);

            handle.Size = new JollyVector2(handleSize, handleSize);
            handle.Rotation = 0.0f;
            handle.Scale = JollyVector2.One;
            handle.Layer = EditorCollisionLayer + 1;
            handle.Opacity = 1.0f;
            handle.Visible = true;
        }

        private void BuildPixelCollisionOverlay(JollySpriteObject spriteObject)
        {
            JollyCollider? collider = spriteObject.Collider;

            if (collider == null)
            {
                return;
            }

            JollySprite sprite = spriteObject.Sprite;
            JollyTexture texture = sprite.Texture;

            int sourceX = Math.Clamp((int)MathF.Round(sprite.SourceX * texture.Width), 0, texture.Width - 1);
            int sourceY = Math.Clamp((int)MathF.Round(sprite.SourceY * texture.Height), 0, texture.Height - 1);
            int sourceWidth = Math.Clamp((int)MathF.Round(sprite.SourceWidth * texture.Width), 1, texture.Width - sourceX);
            int sourceHeight = Math.Clamp((int)MathF.Round(sprite.SourceHeight * texture.Height), 1, texture.Height - sourceY);
            float scaleX = MathF.Abs(sprite.Scale.X);
            float scaleY = MathF.Abs(sprite.Scale.Y);

            if (scaleX <= 0.0f || scaleY <= 0.0f)
            {
                return;
            }

            float pixelWidth = sprite.Width / sourceWidth * scaleX;
            float pixelHeight = sprite.Height / sourceHeight * scaleY;

            if (pixelWidth <= 0.0f || pixelHeight <= 0.0f)
            {
                return;
            }

            float threshold = collider.AlphaThreshold;

            float rotationRadians = sprite.Rotation * MathF.PI / 180.0f;
            System.Numerics.Matrix3x2 rotation = System.Numerics.Matrix3x2.CreateRotation(rotationRadians);
            int requiredSprites = 0;

            for (int y = 0; y < sourceHeight; y++)
            {
                int x = 0;

                while (x < sourceWidth)
                {
                    while (x < sourceWidth && sprite.Texture.GetAlpha(sourceX + x, sourceY + y) < threshold)
                    {
                        x++;
                    }

                    if (x >= sourceWidth)
                    {
                        break;
                    }

                    int runStart = x;
                    while (x < sourceWidth && sprite.Texture.GetAlpha(sourceX + x, sourceY + y) >= threshold)
                    {
                        x++;
                    }

                    int runWidth = x - runStart;
                    if (runWidth > 0)
                    {
                        requiredSprites++;
                    }
                }
            }

            if (requiredSprites == 0)
            {
                return;
            }

            EnsureCollisionSprites(requiredSprites);

            int spriteIndex = 0;

            float scaledWidth = sprite.Width * scaleX;
            float scaledHeight = sprite.Height * scaleY;

            for (int y = 0; y < sourceHeight; y++)
            {
                int x = 0;
                while (x < sourceWidth)
                {
                    while (x < sourceWidth && sprite.Texture.GetAlpha(sourceX + x, sourceY + y) < threshold)
                    {
                        x++;
                    }

                    if (x >= sourceWidth)
                    {
                        break;
                    }

                    int runStart = x;
                    while (x < sourceWidth && sprite.Texture.GetAlpha(sourceX + x, sourceY + y) >= threshold)
                    {
                        x++;
                    }

                    int runWidth = x - runStart;
                    if (runWidth <= 0)
                    {
                        continue;
                    }

                    float localX = -scaledWidth * 0.5f + (runStart + runWidth * 0.5f) * pixelWidth;
                    float localY = -scaledHeight * 0.5f + (y + 0.5f) * pixelHeight;
                    System.Numerics.Vector2 localPosition = new(localX, localY);
                    System.Numerics.Vector2 worldOffset = System.Numerics.Vector2.Transform(localPosition, rotation);

                    JollySprite overlaySprite = _editorCollisionSprites[spriteIndex++];

                    overlaySprite.Position = new JollyVector2(sprite.Position.X + worldOffset.X, sprite.Position.Y + worldOffset.Y);
                    overlaySprite.Size = new JollyVector2(runWidth * pixelWidth, pixelHeight);

                    overlaySprite.Scale = JollyVector2.One;
                    overlaySprite.Rotation = sprite.Rotation;
                    overlaySprite.Layer = EditorCollisionLayer;
                    overlaySprite.Opacity = 0.35f;
                    overlaySprite.Visible = true;
                }
            }

            for (int i = spriteIndex; i < _editorCollisionSprites.Count; i++)
            {
                _editorCollisionSprites[i].Visible = false;
            }
        }

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            _renderTimer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            _renderTimer.Stop();
            _scene.Shutdown();
            base.OnClosed(e);
        }

        private void RenderTimer_Tick(object? sender, EventArgs e)
        {
            if (!SceneView.IsDirectXReady)
            {
                return;
            }

            BuildEditorOverlay();
            List<JollySprite> sprites = [.. _scene.GetSprites(), .. _editorOverlaySprites];
            JollyRenderFrame frame = new(sprites);
            _renderer.SubmitFrame(frame);
        }

        private void SceneView_RenderSizeChanged(float width, float height)
        {
            _renderer.Resize(width, height);
            UpdateViewportStatus();
        }

        private void SceneView_MouseScreenPositionChanged(object? sender, System.Windows.Point position)
        {
            _lastMousePosition = position;
            System.Numerics.Vector2 worldPosition = _renderer.Camera.ScreenToWorld(new System.Numerics.Vector2((float)position.X, (float)position.Y));
            PositionStatus.Content = $"World: {worldPosition.X:0.00}, {worldPosition.Y:0.00}";

            if (_circleCollisionDragging && _selectedObject is JollySpriteObject circleCollisionObject)
            {
                UpdateCircleCollisionDrag(circleCollisionObject, worldPosition);
                return;
            }

            if (_collisionDragging && _selectedObject is JollySpriteObject collisionObject)
            {
                UpdateCollisionDrag(collisionObject, worldPosition);
                return;
            }

            if (_gizmoDragging && _selectedObject is JollySpriteObject gizmoObject)
            {
                UpdateGizmoDrag(gizmoObject, worldPosition);
                return;
            }

            if (!_viewportMouseDown || _dragObject == null)
            {
                return;
            }

            System.Numerics.Vector2 delta = worldPosition - _dragStartWorldPosition;
            if (!_draggingObject && delta.LengthSquared() < 1.0f)
            {
                return;
            }

            _draggingObject = true;
            _dragObject.Position = new JollyVector2(_dragStartObjectPosition.X + delta.X, _dragStartObjectPosition.Y + delta.Y);
            SynchronizeSprite(_dragObject);
            BuildEditorOverlay();
        }

        private void SceneView_MouseButtonDown()
        {
            _viewportMouseDown = true;
            _draggingObject = false;
            _dragObject = null;
            _gizmoDragging = false;

            if (!SceneView.IsDirectXReady)
            {
                return;
            }

            System.Numerics.Vector2 worldPosition = _renderer.Camera.ScreenToWorld(new System.Numerics.Vector2((float)_lastMousePosition.X, (float)_lastMousePosition.Y));

            JollyGameObject? clickedObject = FindObjectAtWorldPosition(worldPosition);

            if (clickedObject != null && !ReferenceEquals(clickedObject, _selectedObject))
            {
                SelectObject(clickedObject);
            }

            if (_selectedObject is not JollySpriteObject spriteObject)
            {
                if (clickedObject == null)
                {
                    SelectObject(null);
                }

                return;
            }

            if (!spriteObject.Active || spriteObject.IsDestroyed || !spriteObject.Visible)
            {
                return;
            }

            if (TryBeginCircleCollisionDrag(spriteObject, worldPosition))
            {
                return;
            }

            if (TryBeginCollisionDrag(spriteObject, worldPosition))
            {
                return;
            }

            if (TryBeginGizmoDrag(spriteObject, worldPosition))
            {
                return;
            }

            if (IsPointInsideObject(spriteObject, worldPosition))
            {
                _dragObject = spriteObject;
                _dragStartWorldPosition = worldPosition;
                _dragStartObjectPosition = spriteObject.Position;
                return;
            }

            if (clickedObject == null)
            {
                SelectObject(null);
            }
        }

        private void SceneView_MouseButtonUp()
        {
            if (!_viewportMouseDown)
            {
                return;
            }

            _viewportMouseDown = false;

            if (_circleCollisionDragging)
            {
                _circleCollisionDragging = false;

                if (_selectedObject != null)
                {
                    BuildInspector(_selectedObject);
                }

                BuildEditorOverlay();
                return;
            }

            if (_collisionDragging)
            {
                _collisionDragging = false;
                _collisionHandle = -1;

                if (_selectedObject != null)
                {
                    BuildInspector(_selectedObject);
                }

                BuildEditorOverlay();
                return;
            }

            if (_gizmoDragging)
            {
                _gizmoDragging = false;
                _activeGizmoTool = EditorTool.Select;

                if (_selectedObject != null)
                {
                    BuildInspector(_selectedObject);
                }

                BuildEditorOverlay();
                return;
            }

            if (_draggingObject)
            {
                _dragObject = null;
                _draggingObject = false;

                if (_selectedObject != null)
                {
                    BuildInspector(_selectedObject);
                }

                BuildEditorOverlay();
                return;
            }

            _dragObject = null;
        }

        private int HitTestCollisionHandle(JollySpriteObject spriteObject, System.Numerics.Vector2 worldPosition)
        {
            JollyCollider? collider = spriteObject.Collider;

            if (collider == null || collider.Shape != JollyColliderShape.Rectangle)
            {
                return -1;
            }

            float scaleX = MathF.Abs(spriteObject.Scale.X);
            float scaleY = MathF.Abs(spriteObject.Scale.Y);

            float width = MathF.Abs(collider.Width * scaleX);
            float height = MathF.Abs(collider.Height * scaleY);

            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            float rotationRadians = spriteObject.Rotation * MathF.PI / 180.0f;

            System.Numerics.Matrix3x2 rotation = System.Numerics.Matrix3x2.CreateRotation(rotationRadians);

            System.Numerics.Vector2[] offsets =
            [
                new System.Numerics.Vector2(-halfWidth, -halfHeight),
                new System.Numerics.Vector2(halfWidth, -halfHeight),
                new System.Numerics.Vector2(-halfWidth, halfHeight),
                new System.Numerics.Vector2(halfWidth, halfHeight)
            ];

            float hitRadius = 18.0f / MathF.Max(0.01f, _renderer.Camera.Zoom);

            for (int i = 0; i < offsets.Length; i++)
            {
                System.Numerics.Vector2 offset = System.Numerics.Vector2.Transform(offsets[i], rotation);
                System.Numerics.Vector2 handlePosition = new(spriteObject.Position.X + offset.X, spriteObject.Position.Y + offset.Y);

                if (System.Numerics.Vector2.Distance(worldPosition, handlePosition) <= hitRadius)
                {
                    return i;
                }
            }

            return -1;
        }

        private bool HitTestCircleCollisionHandle(JollySpriteObject spriteObject, System.Numerics.Vector2 worldPosition)
        {
            JollyCollider? collider = spriteObject.Collider;

            if (collider == null || collider.Shape != JollyColliderShape.Circle)
            {
                return false;
            }

            float scale = MathF.Max(MathF.Abs(spriteObject.Scale.X), MathF.Abs(spriteObject.Scale.Y));
            float radius = MathF.Abs(collider.Radius * scale);

            if (radius <= 0.0f)
            {
                return false;
            }

            float rotationRadians = spriteObject.Rotation * MathF.PI / 180.0f;

            System.Numerics.Vector2 handleOffset = new(MathF.Cos(rotationRadians) * radius, MathF.Sin(rotationRadians) * radius);
            System.Numerics.Vector2 handlePosition = new(spriteObject.Position.X + handleOffset.X, spriteObject.Position.Y + handleOffset.Y);

            float hitRadius = 18.0f / MathF.Max(0.01f, _renderer.Camera.Zoom);

            return System.Numerics.Vector2.Distance(worldPosition, handlePosition) <= hitRadius;
        }

        private void UpdateCollisionDrag(JollySpriteObject spriteObject, System.Numerics.Vector2 worldPosition)
        {
            JollyCollider? collider = spriteObject.Collider;

            if (collider == null)
            {
                return;
            }

            System.Numerics.Vector2 fromObject = worldPosition - new System.Numerics.Vector2(spriteObject.Position.X, spriteObject.Position.Y);

            float rotationRadians = spriteObject.Rotation * MathF.PI / 180.0f;

            System.Numerics.Matrix3x2 inverseRotation = System.Numerics.Matrix3x2.CreateRotation(-rotationRadians);

            System.Numerics.Vector2 localPosition = System.Numerics.Vector2.Transform(fromObject, inverseRotation);

            float scaleX = MathF.Max(0.0001f, MathF.Abs(_collisionStartScale.X));
            float scaleY = MathF.Max(0.0001f, MathF.Abs(_collisionStartScale.Y));

            float newWidth = _collisionStartWidth;
            float newHeight = _collisionStartHeight;

            switch (_collisionHandle)
            {
                case 0:
                    newWidth = MathF.Max(1.0f, -localPosition.X * 2.0f / scaleX);
                    newHeight = MathF.Max(1.0f, -localPosition.Y * 2.0f / scaleY);
                    break;

                case 1:
                    newWidth = MathF.Max(1.0f, localPosition.X * 2.0f / scaleX);
                    newHeight = MathF.Max(1.0f, -localPosition.Y * 2.0f / scaleY);
                    break;

                case 2:
                    newWidth = MathF.Max(1.0f, -localPosition.X * 2.0f / scaleX);
                    newHeight = MathF.Max(1.0f, localPosition.Y * 2.0f / scaleY);
                    break;

                case 3:
                    newWidth = MathF.Max(1.0f, localPosition.X * 2.0f / scaleX);
                    newHeight = MathF.Max(1.0f, localPosition.Y * 2.0f / scaleY);
                    break;
            }

            collider.Width = newWidth;
            collider.Height = newHeight;

            BuildEditorOverlay();
        }

        private void UpdateCircleCollisionDrag(JollySpriteObject spriteObject, System.Numerics.Vector2 worldPosition)
        {
            JollyCollider? collider = spriteObject.Collider;

            if (collider == null)
            {
                return;
            }

            System.Numerics.Vector2 fromObject = worldPosition - new System.Numerics.Vector2(spriteObject.Position.X, spriteObject.Position.Y);
            float distance = fromObject.Length();
            float scale = MathF.Max(MathF.Abs(_circleCollisionStartScale.X), MathF.Abs(_circleCollisionStartScale.Y));
            scale = MathF.Max(0.0001f, scale);
            collider.Radius = MathF.Max(1.0f, distance / scale);
            BuildEditorOverlay();
        }

        private JollyGameObject? FindObjectAtWorldPosition(System.Numerics.Vector2 worldPosition)
        {
            for (int i = _scene.Objects.Count - 1; i >= 0; i--)
            {
                JollyGameObject gameObject = _scene.Objects[i];

                if (!gameObject.Active ||
                    !gameObject.Visible ||
                    gameObject.IsDestroyed)
                {
                    continue;
                }

                if (gameObject is not JollySpriteObject spriteObject)
                {
                    continue;
                }

                if (IsPointInsideObject(spriteObject, worldPosition))
                {
                    return gameObject;
                }
            }

            return null;
        }

        private bool TryBeginGizmoDrag(JollySpriteObject spriteObject, System.Numerics.Vector2 worldPosition)
        {
            JollySprite sprite = spriteObject.Sprite;
            float width = MathF.Abs(sprite.Width * sprite.Scale.X);
            float height = MathF.Abs(sprite.Height * sprite.Scale.Y);
            float handleRadius = 22.0f / MathF.Max(0.01f, _renderer.Camera.Zoom);

            if (_currentTool == EditorTool.Scale || _currentTool == EditorTool.Select)
            {
                System.Numerics.Matrix3x2 rotation = System.Numerics.Matrix3x2.CreateRotation(sprite.Rotation * MathF.PI / 180.0f);
                System.Numerics.Vector2 offset = System.Numerics.Vector2.Transform(new System.Numerics.Vector2(width * 0.5f, height * 0.5f), rotation);
                System.Numerics.Vector2 scaleHandle = new(sprite.Position.X + offset.X, sprite.Position.Y + offset.Y);

                if (System.Numerics.Vector2.Distance(worldPosition, scaleHandle) <= handleRadius)
                {
                    _activeGizmoTool = EditorTool.Scale;
                    BeginGizmoDrag(spriteObject, worldPosition);
                    return true;
                }
            }

            if (_currentTool == EditorTool.Rotate || _currentTool == EditorTool.Select)
            {
                float radius = MathF.Max(50.0f, MathF.Max(width, height) * 0.5f + 35.0f);
                System.Numerics.Vector2 rotateHandle = new(sprite.Position.X, sprite.Position.Y - radius);

                if (System.Numerics.Vector2.Distance(worldPosition, rotateHandle) <= handleRadius)
                {
                    _activeGizmoTool = EditorTool.Rotate;
                    BeginGizmoDrag(spriteObject, worldPosition);
                    return true;
                }
            }

            if (_currentTool == EditorTool.Move)
            {
                float length = MathF.Max(50.0f, MathF.Max(width, height) * 0.5f + 40.0f);
                System.Numerics.Vector2 xHandle = new(sprite.Position.X + length, sprite.Position.Y);
                System.Numerics.Vector2 yHandle = new(sprite.Position.X, sprite.Position.Y - length);

                if (System.Numerics.Vector2.Distance(worldPosition, xHandle) <= handleRadius || System.Numerics.Vector2.Distance(worldPosition, yHandle) <= handleRadius)
                {
                    _activeGizmoTool = EditorTool.Move;
                    BeginGizmoDrag(spriteObject, worldPosition);
                    return true;
                }
            }

            return false;
        }

        private bool TryBeginCollisionDrag(JollySpriteObject spriteObject, System.Numerics.Vector2 worldPosition)
        {
            if (!_collisionEditing)
            {
                return false;
            }

            JollyCollider? collider = spriteObject.Collider;

            if (collider == null || !collider.Enabled)
            {
                return false;
            }

            if (collider.Shape != JollyColliderShape.Rectangle)
            {
                return false;
            }

            int handle = HitTestCollisionHandle(spriteObject, worldPosition);

            if (handle < 0)
            {
                return false;
            }

            _collisionDragging = true;
            _collisionHandle = handle;
            _collisionStartWidth = collider.Width;
            _collisionStartHeight = collider.Height;
            _collisionStartScale = spriteObject.Scale;

            return true;
        }

        private bool TryBeginCircleCollisionDrag(JollySpriteObject spriteObject, System.Numerics.Vector2 worldPosition)
        {
            if (!_collisionEditing)
            {
                return false;
            }

            JollyCollider? collider = spriteObject.Collider;

            if (collider == null || !collider.Enabled)
            {
                return false;
            }

            if (collider.Shape != JollyColliderShape.Circle)
            {
                return false;
            }

            if (!HitTestCircleCollisionHandle(spriteObject, worldPosition))
            {
                return false;
            }

            _circleCollisionDragging = true;
            _circleCollisionStartRadius = collider.Radius;
            _circleCollisionStartScale = spriteObject.Scale;

            return true;
        }

        private void BeginGizmoDrag(JollySpriteObject spriteObject, System.Numerics.Vector2 worldPosition)
        {
            JollySprite sprite = spriteObject.Sprite;

            _gizmoDragging = true;

            _gizmoStartWorldPosition = worldPosition;
            _gizmoStartObjectPosition = spriteObject.Position;
            _gizmoStartScale = spriteObject.Scale;
            _gizmoStartRotation = spriteObject.Rotation;

            System.Numerics.Vector2 fromObject = worldPosition - new System.Numerics.Vector2(sprite.Position.X, sprite.Position.Y);
            _gizmoStartAngle = MathF.Atan2(fromObject.Y, fromObject.X);
            _gizmoStartDistance = MathF.Max(0.001f, fromObject.Length());
        }

        private void UpdateGizmoDrag(JollySpriteObject spriteObject, System.Numerics.Vector2 worldPosition)
        {
            switch (_activeGizmoTool)
            {
                case EditorTool.Move:
                    System.Numerics.Vector2 delta = worldPosition - _gizmoStartWorldPosition;
                    spriteObject.Position = new JollyVector2(_gizmoStartObjectPosition.X + delta.X, _gizmoStartObjectPosition.Y + delta.Y);
                    SynchronizeSprite(spriteObject);
                    break;

                case EditorTool.Rotate:
                    System.Numerics.Vector2 fromRotateObject = worldPosition - new System.Numerics.Vector2(_gizmoStartObjectPosition.X, _gizmoStartObjectPosition.Y);
                    float currentAngle = MathF.Atan2(fromRotateObject.Y, fromRotateObject.X);
                    float angleDelta = currentAngle - _gizmoStartAngle;

                    while (angleDelta > MathF.PI)
                    {
                        angleDelta -= MathF.PI * 2.0f;
                    }

                    while (angleDelta < -MathF.PI)
                    {
                        angleDelta += MathF.PI * 2.0f;
                    }

                    const float rotationSpeed = 10.0f;
                    spriteObject.Rotation = _gizmoStartRotation + angleDelta * rotationSpeed;
                    SynchronizeSprite(spriteObject);
                    break;

                case EditorTool.Scale:
                    System.Numerics.Vector2 fromScaleObject = worldPosition - new System.Numerics.Vector2(_gizmoStartObjectPosition.X, _gizmoStartObjectPosition.Y);

                    float rotationRadians = _gizmoStartRotation * MathF.PI / 180.0f;
                    System.Numerics.Matrix3x2 inverseRotation = System.Numerics.Matrix3x2.CreateRotation(-rotationRadians);
                    System.Numerics.Vector2 localPosition = System.Numerics.Vector2.Transform(fromScaleObject, inverseRotation);

                    float originalWidth = MathF.Abs(spriteObject.Sprite.Width * _gizmoStartScale.X);
                    float originalHeight = MathF.Abs(spriteObject.Sprite.Height * _gizmoStartScale.Y);

                    if (originalWidth <= 0.0f || originalHeight <= 0.0f)
                    {
                        break;
                    }

                    float newWidth = MathF.Max(1.0f, localPosition.X * 2.0f);
                    float newHeight = MathF.Max(1.0f, localPosition.Y * 2.0f);

                    float scaleX = newWidth / MathF.Abs(spriteObject.Sprite.Width);
                    float scaleY = newHeight / MathF.Abs(spriteObject.Sprite.Height);

                    spriteObject.Scale = new JollyVector2(MathF.CopySign(scaleX, _gizmoStartScale.X), MathF.CopySign(scaleY, _gizmoStartScale.Y));

                    SynchronizeSprite(spriteObject);
                    BuildEditorOverlay();
                    break;
            }

            BuildEditorOverlay();
        }

        private static bool IsPointInsideObject(JollyGameObject gameObject, System.Numerics.Vector2 worldPosition)
        {
            if (gameObject is not JollySpriteObject spriteObject)
            {
                return false;
            }

            JollySprite sprite = spriteObject.Sprite;
            float width = MathF.Abs(sprite.Width * sprite.Scale.X);
            float height = MathF.Abs(sprite.Height * sprite.Scale.Y);

            if (width <= 0.0f || height <= 0.0f)
            {
                return false;
            }

            System.Numerics.Vector2 localPosition = worldPosition - new System.Numerics.Vector2(sprite.Position.X, sprite.Position.Y);
            System.Numerics.Matrix3x2 inverseRotation = System.Numerics.Matrix3x2.CreateRotation(-sprite.Rotation);
            localPosition = System.Numerics.Vector2.Transform(localPosition, inverseRotation);

            float halfWidth = width * 0.5f;
            float halfHeight = height * 0.5f;

            return localPosition.X >= -halfWidth && localPosition.X <= halfWidth && localPosition.Y >= -halfHeight && localPosition.Y <= halfHeight;
        }

        private void UpdateViewportStatus()
        {
            System.Numerics.Vector2 viewport = _renderer.Camera.ViewportSize;
            ViewportStatus.Content = $"Viewport: {(int)viewport.X} x {(int)viewport.Y}";
        }

        private void PopulateSceneTree()
        {
            SceneTree.Items.Clear();

            TreeViewItem root = new()
            {
                Header = _scene.Name,
                IsExpanded = true,
                IsSelected = false
            };

            foreach (JollyGameObject gameObject in _scene.Objects)
            {
                TreeViewItem item = new()
                {
                    Header = GetObjectDisplayName(gameObject),
                    Tag = gameObject
                };

                root.Items.Add(item);
            }

            SceneTree.Items.Add(root);
        }

        private static string GetObjectDisplayName(JollyGameObject gameObject)
        {
            if (!string.IsNullOrWhiteSpace(gameObject.Name))
            {
                return gameObject.Name;
            }

            return gameObject.GetType().Name;
        }

        private void SceneTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_updatingInspector)
            {
                return;
            }

            if (e.NewValue is TreeViewItem item && item.Tag is JollyGameObject gameObject)
            {
                SelectObject(gameObject);
            }
        }

        private void SelectObject(JollyGameObject? gameObject)
        {
            _selectedObject = gameObject;

            _gizmoDragging = false;
            _dragObject = null;
            _draggingObject = false;

            _updatingInspector = true;

            try
            {
                InspectorPanel.Children.Clear();

                if (gameObject == null)
                {
                    SelectionStatus.Content = "Selection: None";
                    return;
                }

                TreeViewItem? item = FindTreeItem(SceneTree, gameObject);

                if (item != null)
                {
                    item.IsSelected = true;
                    item.BringIntoView();
                }

                BuildInspector(gameObject);
                SelectionStatus.Content = $"Selection: {gameObject.Name}";
            }
            finally
            {
                _updatingInspector = false;
            }

            BuildEditorOverlay();
        }

        private static TreeViewItem? FindTreeItem(ItemsControl parent, JollyGameObject gameObject)
        {
            foreach (object item in parent.Items)
            {
                if (item is not TreeViewItem treeItem)
                {
                    continue;
                }

                if (ReferenceEquals(treeItem.Tag, gameObject))
                {
                    return treeItem;
                }

                TreeViewItem? child = FindTreeItem(treeItem, gameObject);

                if (child != null)
                {
                    return child;
                }
            }

            return null;
        }

        private void ShowNoSelection()
        {
            InspectorPanel.Children.Clear();
            InspectorPanel.Children.Add(new TextBlock
            {
                Text = "No object selected",
                Foreground =
                    new SolidColorBrush(System.Windows.Media.Color.FromRgb(136, 136, 136)), Margin = new System.Windows.Thickness(0, 4, 0, 0) 
            });
        }

        private void BuildInspector(JollyGameObject gameObject)
        {
            _updatingInspector = true;

            try
            {
                InspectorPanel.Children.Clear();

                AddSectionHeader("Object");

                AddTextProperty("Name", gameObject.Name, value =>
                {
                    gameObject.Name = value;
                    RefreshSelectedTreeItem();
                });

                AddTextProperty("Tag", gameObject.Tag, value =>
                {
                    gameObject.Tag = value;
                });

                AddBoolProperty("Active", gameObject.Active, value =>
                {
                    gameObject.Active = value;
                });

                AddBoolProperty("Visible", gameObject.Visible, value =>
                {
                    gameObject.Visible = value;

                    if (gameObject is JollySpriteObject spriteObject)
                    {
                        spriteObject.Sprite.Visible = value;
                    }
                });

                AddSectionHeader("Transform");

                AddFloatProperty("Position X", gameObject.Position.X, value =>
                {
                    gameObject.Position = new JollyVector2(value, gameObject.Position.Y);
                    SynchronizeSprite(gameObject);
                    BuildEditorOverlay();
                });

                AddFloatProperty("Position Y", gameObject.Position.Y, value =>
                {
                    gameObject.Position = new JollyVector2(gameObject.Position.X, value);
                    SynchronizeSprite(gameObject);
                    BuildEditorOverlay();
                });

                AddFloatProperty("Rotation", gameObject.Rotation, value =>
                {
                    gameObject.Rotation = value;
                    SynchronizeSprite(gameObject);
                    BuildEditorOverlay();
                });

                AddFloatProperty("Scale X", gameObject.Scale.X, value =>
                {
                    gameObject.Scale = new JollyVector2(value, gameObject.Scale.Y);
                    SynchronizeSprite(gameObject);
                    BuildEditorOverlay();
                });

                AddFloatProperty("Scale Y", gameObject.Scale.Y, value =>
                {
                    gameObject.Scale = new JollyVector2(gameObject.Scale.X, value);
                    SynchronizeSprite(gameObject);
                    BuildEditorOverlay();
                });

                if (gameObject is JollySpriteObject spriteObject)
                {
                    BuildSpriteInspector(spriteObject);
                    BuildCollisionInspector(spriteObject);
                }

                AddSectionHeader("Physics");

                AddBoolProperty("Use Physics", gameObject.UsePhysics, value =>
                {
                    gameObject.UsePhysics = value;
                });

                AddBoolProperty("Use Gravity", gameObject.UseGravity, value =>
                {
                    gameObject.UseGravity = value;
                });

                AddFloatProperty("Drag", gameObject.Drag, value =>
                {
                    gameObject.Drag = value;
                });

                AddFloatProperty("Ground Friction", gameObject.GroundFriction, value =>
                {
                    gameObject.GroundFriction = value;
                });

                AddFloatProperty("Air Friction", gameObject.AirFriction, value =>
                {
                    gameObject.AirFriction = value;
                });

                AddFloatProperty("Restitution", gameObject.Restitution, value =>
                {
                    gameObject.Restitution = value;
                });
            }
            finally
            {
                _updatingInspector = false;
            }
        }

        private void BuildSpriteInspector(JollySpriteObject spriteObject)
        {
            JollySprite sprite = spriteObject.Sprite;
            AddSectionHeader("Sprite");

            AddFloatProperty("Width", sprite.Width, value =>
            {
                sprite.Width = value;
                BuildEditorOverlay();
            });

            AddFloatProperty("Height", sprite.Height, value =>
            {
                sprite.Height = value;
                BuildEditorOverlay();
            });

            AddFloatProperty("Opacity", sprite.Opacity, value =>
            {
                sprite.Opacity = value;
            });

            AddIntProperty("Layer", sprite.Layer, value =>
            {
                sprite.Layer = value;
            });

            AddBoolProperty("Flip Horizontal", sprite.FlipHorizontal, value =>
            {
                sprite.FlipHorizontal = value;
            });

            AddBoolProperty("Flip Vertical", sprite.FlipVertical, value =>
            {
                sprite.FlipVertical = value;
            });

            AddTextProperty("Collision Mode", sprite.CollisionMode.ToString(), _ => { });

            JollyCollider? collider = spriteObject.Collider;
            if (collider == null)
            {
                return;
            }

            AddSectionHeader("Collider");

            AddTextProperty("Shape", collider.Shape.ToString(), _ => { });

            AddBoolProperty("Enabled", collider.Enabled, value =>
            {
                collider.Enabled = value;
            });

            AddBoolProperty("Trigger", collider.IsTrigger, value =>
            {
                collider.IsTrigger = value;
            });

            AddIntProperty("Alpha Threshold", collider.AlphaThreshold, value =>
            {
                if (value >= 0 &&
                    value <= byte.MaxValue)
                {
                    collider.AlphaThreshold =
                        (byte)value;
                }
            });
        }

        private void BuildCollisionInspector(JollySpriteObject spriteObject)
        {
            JollyCollider? collider = spriteObject.Collider;

            AddSectionHeader("Collision");

            if (collider == null)
            {
                AddBoolProperty("Enabled", true, value =>
                {
                    if (value)
                    {
                        spriteObject.SetRectangleCollider(spriteObject.Sprite.Width, spriteObject.Sprite.Height);
                        BuildInspector(spriteObject);
                        BuildEditorOverlay();
                    }
                });

                AddComboProperty("Shape", "None", Enum.GetNames<JollyColliderShape>(), value =>
                {
                    switch (value)
                    {
                        case nameof(JollyColliderShape.Rectangle):
                            spriteObject.SetRectangleCollider(spriteObject.Sprite.Width, spriteObject.Sprite.Height);
                            break;

                        case nameof(JollyColliderShape.Circle):
                            spriteObject.SetCircleCollider(MathF.Max(1.0f, MathF.Min(spriteObject.Sprite.Width, spriteObject.Sprite.Height) * 0.5f));
                            break;

                        case nameof(JollyColliderShape.Pixel):
                            spriteObject.SetPixelCollider();
                            break;

                        case nameof(JollyColliderShape.None):
                            return;
                    }

                    BuildInspector(spriteObject);
                    BuildEditorOverlay();
                });

                return;
            }

            AddBoolProperty("Enabled", collider.Enabled, value =>
            {
                collider.Enabled = value;
                BuildEditorOverlay();
            });

            AddComboProperty("Shape", collider.Shape.ToString(), Enum.GetNames<JollyColliderShape>(), value =>
            {
                switch (value)
                {
                    case nameof(JollyColliderShape.None):
                        collider.Shape = JollyColliderShape.None;
                        break;

                    case nameof(JollyColliderShape.Rectangle):
                        if (collider.Shape != JollyColliderShape.Rectangle)
                        {
                            float width = collider.Width > 0.0f ? collider.Width : spriteObject.Sprite.Width;
                            float height = collider.Height > 0.0f ? collider.Height : spriteObject.Sprite.Height;
                            spriteObject.SetRectangleCollider(width, height);
                        }
                        break;

                    case nameof(JollyColliderShape.Circle):
                        if (collider.Shape != JollyColliderShape.Circle)
                        {
                            float radius = collider.Radius > 0.0f ? collider.Radius : MathF.Min(spriteObject.Sprite.Width, spriteObject.Sprite.Height) * 0.5f;
                            spriteObject.SetCircleCollider(MathF.Max(1.0f, radius));
                        }
                        break;

                    case nameof(JollyColliderShape.Pixel):
                        if (collider.Shape != JollyColliderShape.Pixel)
                        {
                            spriteObject.SetPixelCollider(collider.AlphaThreshold);
                        }
                        break;
                }

                BuildInspector(spriteObject);
                BuildEditorOverlay();
            });

            collider = spriteObject.Collider;

            if (collider == null)
            {
                return;
            }

            AddBoolProperty("Trigger", collider.IsTrigger, value =>
            {
                collider.IsTrigger = value;
                BuildEditorOverlay();
            });

            AddComboProperty("Layer", collider.Layer.ToString(), Enum.GetNames<JollyCollisionLayer>().Where(x => x != nameof(JollyCollisionLayer.None) && x != nameof(JollyCollisionLayer.All)).ToArray(), value =>
            {
                if (Enum.TryParse<JollyCollisionLayer>(value, out JollyCollisionLayer layer))
                {
                    collider.Layer = layer;
                }
            });

            AddMaskProperty("Mask", collider);
            
            AddComboProperty("Mask", collider.Mask.ToString(), Enum.GetNames<JollyCollisionLayer>(), value =>
            {
                if (Enum.TryParse<JollyCollisionLayer>(value, out JollyCollisionLayer mask))
                {
                    collider.Mask = mask;
                }
            });

            switch (collider.Shape)
            {
                case JollyColliderShape.Rectangle:
                    AddFloatProperty("Width", collider.Width, value =>
                    {
                        collider.Width = MathF.Max(1.0f, value);
                        BuildEditorOverlay();
                    });

                    AddFloatProperty("Height", collider.Height, value =>
                    {
                        collider.Height = MathF.Max(1.0f, value);
                        BuildEditorOverlay();
                    });
                    break;

                case JollyColliderShape.Circle:
                    AddFloatProperty("Radius", collider.Radius, value =>
                    {
                        collider.Radius = MathF.Max(1.0f, value);
                        BuildEditorOverlay();
                    });
                    break;

                case JollyColliderShape.Pixel:
                    AddFloatProperty("Alpha Threshold", collider.AlphaThreshold, value =>
                    {
                        collider.AlphaThreshold = (byte)Math.Clamp((int)value, 0, 255);
                        BuildEditorOverlay();
                    });
                    break;
            }
        }

        private void AddSectionHeader(string text)
        {
            Border border = new()
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(45, 45, 45)), Padding = new System.Windows.Thickness(6, 5, 6, 5), Margin = new System.Windows.Thickness(0, 12, 0, 6)
            };

            border.Child = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White
            };

            InspectorPanel.Children.Add(border);
        }

        private void AddTextProperty(string name, string value, Action<string> changed)
        {
            Grid grid = CreatePropertyGrid(name);

            System.Windows.Controls.TextBox textBox = new()
            {
                Text = value ?? string.Empty,
                VerticalContentAlignment =
                    VerticalAlignment.Center
            };

            textBox.LostFocus += (_, _) =>
            {
                if (_updatingInspector)
                {
                    return;
                }

                changed(textBox.Text);
            };

            textBox.KeyDown += (_, e) =>
            {
                if (e.Key ==
                    System.Windows.Input.Key.Enter)
                {
                    changed(textBox.Text);
                    e.Handled = true;
                }
            };

            Grid.SetColumn(textBox, 1);
            grid.Children.Add(textBox);
            InspectorPanel.Children.Add(grid);
        }

        private void AddFloatProperty(string name, float value, Action<float> changed)
        {
            Grid grid = CreatePropertyGrid(name);

            System.Windows.Controls.TextBox textBox = new()
            {
                Text = value.ToString("0.###", CultureInfo.InvariantCulture), VerticalContentAlignment = VerticalAlignment.Center
            };

            textBox.LostFocus += (_, _) =>
            {
                ApplyFloat(textBox, changed);
            };

            textBox.KeyDown += (_, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    ApplyFloat(textBox, changed);
                    e.Handled = true;
                }
            };

            Grid.SetColumn(textBox, 1);
            grid.Children.Add(textBox);
            InspectorPanel.Children.Add(grid);
        }

        private void AddIntProperty(string name, int value, Action<int> changed)
        {
            Grid grid = CreatePropertyGrid(name);

            System.Windows.Controls.TextBox textBox = new()
            {
                Text = value.ToString(CultureInfo.InvariantCulture), VerticalContentAlignment = VerticalAlignment.Center
            };

            textBox.LostFocus += (_, _) =>
            {
                ApplyInt(textBox, changed);
            };

            textBox.KeyDown += (_, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    ApplyInt(textBox, changed);
                    e.Handled = true;
                }
            };

            Grid.SetColumn(textBox, 1);
            grid.Children.Add(textBox);
            InspectorPanel.Children.Add(grid);
        }

        private void AddBoolProperty(string name, bool value, Action<bool> changed)
        {
            Grid grid = CreatePropertyGrid(name);

            System.Windows.Controls.CheckBox checkBox = new()
            {
                IsChecked = value,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Center
            };

            checkBox.Checked += (_, _) =>
            {
                if (!_updatingInspector)
                {
                    changed(true);
                }
            };

            checkBox.Unchecked += (_, _) =>
            {
                if (!_updatingInspector)
                {
                    changed(false);
                }
            };

            Grid.SetColumn(checkBox, 1);
            grid.Children.Add(checkBox);
            InspectorPanel.Children.Add(grid);
        }

        private void AddComboProperty(string name, string value, IEnumerable<string> options, Action<string> changed)
        {
            List<string> optionList = options.ToList();

            StackPanel panel = new()
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                Margin = new Thickness(0, 2, 0, 2)
            };

            TextBlock label = new()
            {
                Text = name,
                Width = 120,
                VerticalAlignment = VerticalAlignment.Center
            };

            System.Windows.Controls.ComboBox comboBox = new()
            {
                Width = 150,
                ItemsSource = optionList
            };

            int selectedIndex = optionList.IndexOf(value);

            if (selectedIndex >= 0)
            {
                comboBox.SelectedIndex = selectedIndex;
            }

            comboBox.SelectionChanged += (_, _) =>
            {
                if (_updatingInspector)
                {
                    return;
                }

                if (comboBox.SelectedIndex < 0)
                {
                    return;
                }

                changed(optionList[comboBox.SelectedIndex]);
            };

            panel.Children.Add(label);
            panel.Children.Add(comboBox);

            InspectorPanel.Children.Add(panel);
        }

        private void AddMaskProperty(string name, JollyCollider collider)
        {
            StackPanel section = new StackPanel();
            TextBlock label = new TextBlock();
            label.Text = name;
            label.Margin = new Thickness(0, 4, 0, 2);
            section.Children.Add(label);

            System.Windows.Controls.CheckBox allCheckBox = new System.Windows.Controls.CheckBox();
            allCheckBox.Content = "All";
            allCheckBox.IsChecked = collider.Mask == JollyCollisionLayer.All;
            allCheckBox.Margin = new Thickness(4, 2, 0, 2);
            section.Children.Add(allCheckBox);

            System.Windows.Controls.CheckBox noneCheckBox = new System.Windows.Controls.CheckBox();
            noneCheckBox.Content = "None";
            noneCheckBox.IsChecked = collider.Mask == JollyCollisionLayer.None;
            noneCheckBox.Margin = new Thickness(4, 2, 0, 2);
            section.Children.Add(noneCheckBox);

            foreach (JollyCollisionLayer layer in Enum.GetValues<JollyCollisionLayer>())
            {
                if (layer == JollyCollisionLayer.None || layer == JollyCollisionLayer.All)
                {
                    continue;
                }

                System.Windows.Controls.CheckBox checkBox = new System.Windows.Controls.CheckBox();
                checkBox.Content = layer.ToString();
                checkBox.IsChecked = (collider.Mask & layer) == layer;
                checkBox.Margin = new Thickness(4, 2, 0, 2);
                checkBox.Tag = layer;
                checkBox.Checked += (_, _) => UpdateMaskFromCheckBoxes(collider, section);
                checkBox.Unchecked += (_, _) => UpdateMaskFromCheckBoxes(collider, section);
                section.Children.Add(checkBox);
            }

            allCheckBox.Checked += (_, _) =>
            {
                collider.Mask = JollyCollisionLayer.All;
                foreach (object child in section.Children)
                {
                    if (child is System.Windows.Controls.CheckBox checkBox && checkBox != allCheckBox)
                    {
                        checkBox.IsChecked = true;
                    }
                }
            };

            allCheckBox.Unchecked += (_, _) =>
            {
                if (collider.Mask == JollyCollisionLayer.All)
                {
                    collider.Mask = JollyCollisionLayer.None;
                }
            };

            noneCheckBox.Checked += (_, _) =>
            {
                collider.Mask = JollyCollisionLayer.None;
                foreach (object child in section.Children)
                {
                    if (child is System.Windows.Controls.CheckBox checkBox && checkBox != noneCheckBox)
                    {
                        checkBox.IsChecked = false;
                    }
                }
            };

            noneCheckBox.Unchecked += (_, _) =>
            {
                if (collider.Mask == JollyCollisionLayer.None)
                {
                    collider.Mask = JollyCollisionLayer.None;
                }
            };

            InspectorPanel.Children.Add(section);
        }

        private void UpdateMaskFromCheckBoxes(JollyCollider collider, StackPanel section)
        {
            JollyCollisionLayer mask = JollyCollisionLayer.None;

            foreach (object child in section.Children)
            {
                if (child is not System.Windows.Controls.CheckBox checkBox || checkBox.Tag is not JollyCollisionLayer layer)
                {
                    continue;
                }

                if (checkBox.IsChecked == true)
                {
                    mask |= layer;
                }
            }

            collider.Mask = mask;
        }

        private static Grid CreatePropertyGrid(string name)
        {
            Grid grid = new()
            {
                Margin = new Thickness(0, 2, 0, 2)
            };

            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(135)
            });

            grid.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star) 
            });

            TextBlock label = new()
            {
                Text = name,
                VerticalAlignment = VerticalAlignment.Center, 
                Foreground = new SolidColorBrush(System.Windows.Media.Color.FromRgb(205, 205, 205))
            };

            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            return grid;
        }

        private static void ApplyFloat(System.Windows.Controls.TextBox textBox, Action<float> changed)
        {
            if (!float.TryParse(textBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                return;
            }

            if (!float.IsFinite(value))
            {
                return;
            }

            changed(value);
        }

        private static void ApplyInt(System.Windows.Controls.TextBox textBox, Action<int> changed)
        {
            if (!int.TryParse(textBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value))
            {
                return;
            }

            changed(value);
        }

        private static void SynchronizeSprite(JollyGameObject gameObject)
        {
            if (gameObject is not JollySpriteObject spriteObject)
            {
                return;
            }

            spriteObject.Sprite.Position = gameObject.Position;
            spriteObject.Sprite.Scale = gameObject.Scale;
            spriteObject.Sprite.Rotation = gameObject.Rotation;
            spriteObject.Sprite.Visible = gameObject.Visible;
        }

        private void RefreshSelectedTreeItem()
        {
            if (_selectedObject == null)
            {
                return;
            }

            foreach (object item in SceneTree.Items)
            {
                if (item is not TreeViewItem root)
                {
                    continue;
                }

                foreach (object child in root.Items)
                {
                    if (child is TreeViewItem treeItem && ReferenceEquals(treeItem.Tag, _selectedObject))
                    {
                        treeItem.Header = GetObjectDisplayName(_selectedObject);
                        return;
                    }
                }
            }
        }

        private void NewScene_Click(object sender, RoutedEventArgs e)
        {
            _scene.Shutdown();

            _selectedObject = null;
            _dragObject = null;
            _gizmoDragging = false;
            _draggingObject = false;

            SceneStatus.Content = $"Scene: {_scene.Name}";

            PopulateSceneTree();
            ShowNoSelection();

            SelectionStatus.Content = "No object selected";
            ReadyStatus.Content = "New scene";

            BuildEditorOverlay();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedObject == null)
            {
                return;
            }

            JollyGameObject objectToDelete = _selectedObject;
            _scene.Remove(objectToDelete);

            _selectedObject = null;
            _dragObject = null;
            _gizmoDragging = false;
            _draggingObject = false;

            PopulateSceneTree();
            ShowNoSelection();

            SelectionStatus.Content = "No object selected";
            ReadyStatus.Content = "Object deleted";

            BuildEditorOverlay();
        }

        private void SelectTool_Click(object sender, RoutedEventArgs e)
        {
            SetEditorTool(EditorTool.Select);
        }

        private void MoveTool_Click(object sender, RoutedEventArgs e)
        {
            SetEditorTool(EditorTool.Move);
        }

        private void RotateTool_Click(object sender, RoutedEventArgs e)
        {
            SetEditorTool(EditorTool.Rotate);
        }

        private void ScaleTool_Click(object sender, RoutedEventArgs e)
        {
            SetEditorTool(EditorTool.Scale);
        }

        private void SetEditorTool(EditorTool tool)
        {
            _currentTool = tool;
            _activeGizmoTool = EditorTool.Select;

            _gizmoDragging = false;
            _dragObject = null;
            _draggingObject = false;

            switch (tool)
            {
                case EditorTool.Select:
                    ReadyStatus.Content = "Select tool";
                    break;

                case EditorTool.Move:
                    ReadyStatus.Content = "Move tool";
                    break;

                case EditorTool.Rotate:
                    ReadyStatus.Content = "Rotate tool";
                    break;

                case EditorTool.Scale:
                    ReadyStatus.Content = "Scale tool";
                    break;
            }

            BuildEditorOverlay();
        }

        private void GridMenuItem_Click(object sender, RoutedEventArgs e)
        {
            GridOverlay.Visibility = GridMenuItem.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ResetCamera_Click(object sender, RoutedEventArgs e)
        {
            _renderer.Camera.Position = System.Numerics.Vector2.Zero;
            _renderer.Camera.Zoom = 1.0f;
            _renderer.Camera.Rotation = 0.0f;
            ReadyStatus.Content = "Camera reset";
            BuildEditorOverlay();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddObject_Click(object sender, RoutedEventArgs e)
        {
            EditorObject gameObject = new()
            {
                Name = $"Object {_scene.EntityCount + 1}",
                Position = new JollyVector2(0.0f, 0.0f),
                Scale = JollyVector2.One,
                Rotation = 0.0f,
                Active = true,
                Visible = true
            };

            _scene.Add(gameObject);

            PopulateSceneTree();
            SelectObject(gameObject);

            ReadyStatus.Content = $"Created {gameObject.Name}";
        }

        private void AddSprite_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new()
            {
                Title = "Select Sprite Texture",
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                JollySprite sprite = _renderer.CreateSprite(dialog.FileName);

                sprite.Name = Path.GetFileNameWithoutExtension(dialog.FileName);
                sprite.Position = new JollyVector2(0.0f, 0.0f);
                sprite.Visible = true;
                sprite.Opacity = 1.0f;
                sprite.Layer = 0;

                JollySpriteObject spriteObject = new(sprite)
                {
                    Name = sprite.Name,
                    Position = new JollyVector2(0.0f, 0.0f),
                    Scale = JollyVector2.One,
                    Rotation = 0.0f,
                    Active = true,
                    Visible = true
                };

                _scene.Add(spriteObject);

                PopulateSceneTree();
                SelectObject(spriteObject);
                ReadyStatus.Content = $"Created {spriteObject.Name}";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(this, ex.Message, "Unable to Load Sprite", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CollisionTool_Click(object sender, RoutedEventArgs e)
        {
            _collisionEditing = !_collisionEditing;

            if (_collisionEditing && _selectedObject is JollySpriteObject spriteObject)
            {
                if (spriteObject.Collider == null)
                {
                    spriteObject.SetRectangleCollider(spriteObject.Sprite.Width, spriteObject.Sprite.Height);
                }

                spriteObject.Collider.Enabled = true;
            }

            ReadyStatus.Content = _collisionEditing ? "Collision editor" : "Select tool";

            BuildEditorOverlay();
        }
    }
}