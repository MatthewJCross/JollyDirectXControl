using System.Globalization;

namespace Jolly2DElite.Game
{
    public sealed record CommanderSave(string Path, string Commander, string Label, DateTimeOffset Saved, byte[] Data)
    {
        public long Cash => (uint)(Data[9] << 24 | Data[10] << 16 | Data[11] << 8 | Data[12]);
        public int Galaxy => Data[15] + 1;
        public int KillTally => Data[71] | Data[72] << 8;
        public string SystemName => CommanderStore.SystemNameAt(Data.AsSpan(3, 6), Data[1], Data[2]);
        public string SavedText => Saved.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        public string SavedDateText => Saved.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
