using Jolly2DEngine.Graphics.Scene;
using Jolly2DEngine.Math;
using Jolly2DEngine.Save;

namespace Jolly2DNuGetTest.SaveTest
{
    public sealed class SaveLoadTestScene : JollyScene
    {
        public SaveLoadTestObject Player { get; }
        public SaveLoadTestObject Crate { get; }
        public int Score { get; set; }
        public int Level { get; set; } = 3;

        public SaveLoadTestScene()
        {
            Name = "Phase14SaveLoad";

            Player = new SaveLoadTestObject
            {
                SaveId = "player",
                Name = "Player",
                Tag = "Player",
                Position = new JollyVector2(120.0f, 180.0f),
                Scale = JollyVector2.One,
                Rotation = 12.0f,
                Visible = true,
                Active = true,
                CustomValue = 25
            };

            Crate = new SaveLoadTestObject
            {
                SaveId = "crate",
                Name = "Crate",
                Tag = "World",
                Position = new JollyVector2(400.0f, 300.0f),
                Scale = new JollyVector2(2.0f, 2.0f),
                Rotation = -8.0f,
                Visible = true,
                Active = true,
                CustomValue = 100
            };

            Add(Player);
            Add(Crate);
            Score = 1250;
        }

        public SaveLoadGameState CaptureState()
        {
            return new SaveLoadGameState
            {
                Score = Score,
                Level = Level,
                Scene = JollySceneSaveState.Capture(this),
                PlayerCustomValue = Player.CustomValue,
                CrateCustomValue = Crate.CustomValue
            };
        }

        public int ApplyState(SaveLoadGameState state)
        {
            ArgumentNullException.ThrowIfNull(state);

            Score = state.Score;
            Level = state.Level;
            Player.CustomValue = state.PlayerCustomValue;
            Crate.CustomValue = state.CrateCustomValue;
            return state.Scene.ApplyTo(this);
        }
    }

    public sealed class SaveLoadGameState
    {
        public int Score { get; set; }
        public int Level { get; set; }
        public int PlayerCustomValue { get; set; }
        public int CrateCustomValue { get; set; }
        public JollySceneSaveState Scene { get; set; } = new();
    }
}
