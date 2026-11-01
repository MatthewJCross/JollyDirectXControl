namespace Jolly2DEngine.Diagnostics
{
    public sealed class JollyProfile
    {
        internal JollyProfile(string name)
        {
            Name = name;
        }

        public string Name { get; }

        internal long CallCount;

        internal double LastMilliseconds;

        internal double TotalMilliseconds;

        internal double MaximumMilliseconds;
    }
}