# `JollyAudioManager`

Sound-effect and music playback manager. Sound effects are cached and can overlap; music playback supports looping, seeking, progress reporting and an optional effects chain.

Namespace: `Jolly2DEngine.Audio`.

## Public API

```csharp
public sealed class JollyAudioManager : IDisposable
public float MasterVolume { get; set; }
public float MusicVolume { get; set; }
public float SfxVolume { get; set; }
public bool Muted { get; set; }
public double MusicProgress { get; }
public bool IsMusicPlaying { get; }
public string? CurrentMusic { get; }
public JollyAudioEffects? MusicEffects { get; }
public TimeSpan MusicPosition { get; }
public TimeSpan MusicDuration { get; }
public void LoadSound(string name, string path)
public void UnloadSound(string name)
public JollySoundPlayback PlaySound(string name)
public JollySoundPlaybackGroup PlaySound(string name, int count)
public void StopAllSounds()
public void PlayMusic(string path, bool loop = true, JollyAudioEffects? effects = null)
public void SeekMusic(TimeSpan position)
public void PauseMusic()
public void ResumeMusic()
public void StopMusic()
public void ClearSounds()
public void Dispose()
```

## Related types

- [Audio effects chain](Audio_JollyAudioEffects.md)
- [Audio subsystem guide](../AUDIO_SUBSYSTEM_GUIDE.md)

## Notes

`MusicProgress` provides progress as exposed by the manager; `MusicPosition` and `MusicDuration` expose the current playback position and total duration. `PlayMusic` can receive a `JollyAudioEffects` instance to process the music stream. Dispose the manager when its owner shuts down.
