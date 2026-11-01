namespace Jolly2DEngine.Assets;

public sealed class JollyAssetHandle<T> : IDisposable where T : class
{
    private Action? _release;

    internal JollyAssetHandle(T asset, Action release)
    {
        Asset = asset;
        _release = release;
    }

    public T Asset { get; }

    public bool IsReleased => _release == null;

    public void Dispose()
    {
        Action? release = Interlocked.Exchange(ref _release, null);
        release?.Invoke();
    }
}
