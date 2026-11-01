using System.Diagnostics;

namespace Jolly2DEngine.Diagnostics
{
    public struct JollyProfileScope : IDisposable
    {
        private readonly JollyProfile? _profile;
        private readonly long _startTimestamp;

        internal JollyProfileScope(JollyProfile? profile)
        {
            _profile = profile;
            _startTimestamp = profile == null ? 0 : Stopwatch.GetTimestamp();
        }

        public void Dispose()
        {
            if (_profile == null || _startTimestamp == 0)
            {
                return;
            }

            long elapsedTicks = Stopwatch.GetTimestamp() - _startTimestamp;
            double elapsedMilliseconds = elapsedTicks * 1000.0 / Stopwatch.Frequency;

            JollyDiagnostics.RecordProfile(_profile, elapsedMilliseconds);
        }
    }
}