# JollyDrumMachine

A WPF sample-based drum sequencer built on Jolly2DEngine audio playback.

## Features

- 16-, 32- and 64-step patterns.
- Four editable pattern slots (A–D), with pattern copying and clearing.
- Per-step velocity (mouse wheel over an active step), per-track volume, pan, tune, attack, decay, mute and solo controls.
- Drag samples into the sequencer to add tracks, or drop a sample onto an existing track to replace its sound.
- Tempo control, adjustable swing and optional timing/velocity humanisation.
- Swing changes the spacing of alternating sixteenth-note steps while keeping each pair of steps on the same overall tempo grid. Humanisation adds small trigger timing and velocity variations.
- Song arrangement made from pattern slots, with repeat counts, section copy/paste/duplicate, removal and reordering.
- Offline arrangement export to stereo 44.1 kHz WAV or 192 kbps MP3.
- JSON project save/load including all four patterns, arrangement, swing and humanisation settings.
- Recent-project shortcut and unsaved-change indication.

## Audio export

WAV export renders the arrangement offline, mixing the source samples into a stereo PCM file. MP3 export first renders the same WAV mix and then encodes it using Windows Media Foundation. MP3 export therefore depends on the Windows Media Foundation components being available on the PC.

The offline renderer uses the samples referenced by each pattern. Keep those sample files in place when moving a project to another PC; project files store sample paths rather than embedding audio files.

## Building

Open `JollyDrumMachine.csproj` in Visual Studio on Windows with the .NET 10 Windows desktop workload installed. The project references Jolly2DEngine via a relative project reference, so keep the engine in the expected relative location or update that reference for your folder layout.

The source package has not been compiled in this environment because the .NET SDK is not available here. Build and test the project locally, especially MP3 encoding and resampling with your own sample formats.

## Additional editing and audio features

The current working branch also includes:

- Per-instrument pan, pitch/tune, attack and decay settings in an instrument settings window.
- Live playback processing for pan, tune, attack and decay. Processed samples are generated in the temporary directory and removed when their instrument audio resource is released.
- Matching pan, tune, attack and decay processing in offline WAV/MP3 arrangement export.
- Per-pattern solo/mute behaviour in playback and offline rendering.
- Arrangement section copy, paste, duplicate, remove and reorder operations.
- Undo/redo history for edits, with a bounded history stack.
- Pattern preset save/load using JSON files.
- Export progress status updates and a cancellation button. WAV rendering checks cancellation during rendering; MP3 encoding runs away from the UI thread.
- Removal of the redundant global sample cache in the window; sample audio is loaded on demand by the playback service.

### Audio processing notes

Pitch is implemented by resampling playback samples, which changes both pitch and duration. Attack is a short fade-in envelope. Decay is an exponential envelope; the default value bypasses decay shaping to preserve the original sample sound. Pan uses equal-power stereo gains. These controls affect both live playback and offline rendering.

The refactor keeps core editable state, pattern capture/cloning, project snapshots, arrangement edits, and bounded undo/redo history in `MainViewModel`. JSON project/preset/recent-project persistence is handled by `ProjectFileService`; pattern model conversions are handled by `PatternDataService`; offline WAV/MP3 export is handled by `ArrangementExportService`. `MainWindow` is now primarily the WPF adapter for dialogs, drag/drop, dynamic step-grid and instrument-row construction, visual playhead updates, and coordinating UI-specific playback transitions. Those dynamic WPF surfaces remain code-behind because the current grid is constructed programmatically rather than through item templates. A Windows build and listening test are still required before treating this as release-ready.
