using Jolly2DEngine.Core;
using Jolly2DEngine.Graphics.Scene;

namespace Jolly2DEngine.Scene
{
    public sealed class JollySceneManager
    {
        private readonly Queue<JollyScene> _pendingScenes = new();

        public JollyScene? CurrentScene { get; private set; }
        public JollyScene? PreviousScene { get; private set; }
        public int EntityCount => CurrentScene?.EntityCount ?? 0;
        public bool IsChangingScene => _pendingScenes.Count > 0;
        public event Action<JollyScene?, JollyScene?>? SceneChanged;

        public void ChangeScene(JollyScene scene)
        {
            ArgumentNullException.ThrowIfNull(scene);

            if (ReferenceEquals(CurrentScene, scene) && _pendingScenes.Count == 0)
            {
                return;
            }

            _pendingScenes.Enqueue(scene);
        }

        public JollyGameObject? Find(string name)
        {
            return CurrentScene?.Find(name);
        }

        public T? Find<T>(string name) where T : JollyGameObject
        {
            return CurrentScene?.Find<T>(name);
        }

        public IReadOnlyList<T> FindAll<T>() where T : JollyGameObject
        {
            return CurrentScene?.FindAll<T>() ?? Array.Empty<T>();
        }

        public void Update(JollyGameTime gameTime)
        {
            ProcessPendingScene();
            CurrentScene?.Update(gameTime);
        }

        public void Draw()
        {
            CurrentScene?.Draw();
        }

        public void Shutdown()
        {
            CurrentScene?.OnExit();
            CurrentScene?.Shutdown();
            CurrentScene = null;
            PreviousScene = null;
            _pendingScenes.Clear();
        }

        private void ProcessPendingScene()
        {
            if (_pendingScenes.Count == 0)
            {
                return;
            }

            JollyScene nextScene = _pendingScenes.Dequeue();
            JollyScene? previousScene = CurrentScene;

            previousScene?.OnExit();
            previousScene?.Shutdown();

            PreviousScene = previousScene;
            CurrentScene = nextScene;

            CurrentScene.Initialize();
            CurrentScene.OnEnter();
            SceneChanged?.Invoke(previousScene, CurrentScene);
        }
    }
}
