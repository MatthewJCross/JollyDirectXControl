using Jolly2DEngine.Core;
using Jolly2DEngine.Graphics.Scene;

namespace Jolly2DEngine.Save
{
    public sealed class JollySceneSaveState
    {
        public string SceneName { get; set; } = string.Empty;
        public List<JollyGameObjectSaveState> Objects { get; set; } = new();

        public static JollySceneSaveState Capture(JollyScene scene)
        {
            ArgumentNullException.ThrowIfNull(scene);

            JollySceneSaveState state = new()
            {
                SceneName = scene.Name
            };

            foreach (var gameObject in scene.Objects)
            {
                state.Objects.Add(JollyGameObjectSaveState.Capture(gameObject));
            }

            return state;
        }

        public int ApplyTo(JollyScene scene)
        {
            ArgumentNullException.ThrowIfNull(scene);

            int restoredCount = 0;

            foreach (JollyGameObjectSaveState objectState in Objects)
            {
                JollyGameObject? gameObject = scene.FindBySaveId(objectState.Id);

                if (gameObject == null && !string.IsNullOrWhiteSpace(objectState.Name))
                {
                    gameObject = scene.Find(objectState.Name);
                }

                if (gameObject == null)
                {
                    continue;
                }

                objectState.ApplyTo(gameObject);
                restoredCount++;
            }

            return restoredCount;
        }
    }
}
