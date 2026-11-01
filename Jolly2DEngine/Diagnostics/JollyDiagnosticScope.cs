using System.Diagnostics;

namespace Jolly2DEngine.Diagnostics
{
    public struct JollyDiagnosticScope : IDisposable
    {
        private readonly JollyDiagnostics? _diagnostics;
        private readonly JollyDiagnosticScopeType _type;
        private readonly long _startTimestamp;

        internal JollyDiagnosticScope(JollyDiagnostics? diagnostics, JollyDiagnosticScopeType type)
        {
            _diagnostics = diagnostics;
            _type = type;
            _startTimestamp = diagnostics == null ? 0 : Stopwatch.GetTimestamp();
        }

        public void Dispose()
        {
            if (_diagnostics == null || _startTimestamp == 0)
            {
                return;
            }

            long elapsedTicks = Stopwatch.GetTimestamp() - _startTimestamp;
            double elapsedMilliseconds = elapsedTicks * 1000.0 / Stopwatch.Frequency;

            _diagnostics.RecordDiagnosticTime(_type, elapsedMilliseconds);
        }
    }

    internal enum JollyDiagnosticScopeType
    {
        Update,
        Render
    }
}