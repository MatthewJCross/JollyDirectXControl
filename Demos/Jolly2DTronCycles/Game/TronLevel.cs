namespace Jolly2DTronCycles.Game
{
    public sealed class TronLevel
    {
        public int Number { get; }
        public string Name { get; }
        public int AICount { get; }
        public float AISpeed { get; }
        public float AISlowSpeed { get; }
        public float AIBoostSpeed { get; }
        public float BrakeSpeed { get; }
        public float Aggression { get; }

        public TronLevel(int number, string name, int aiCount, float aiSpeed, float aiSlowSpeed, float aiBoostSpeed, float brakeSpeed, float aggression)
        {
            Number = number;
            Name = name;
            AICount = aiCount;
            AISpeed = aiSpeed;
            AISlowSpeed = aiSlowSpeed;
            AIBoostSpeed = aiBoostSpeed;
            BrakeSpeed = brakeSpeed;
            Aggression = aggression;
        }
    }
}