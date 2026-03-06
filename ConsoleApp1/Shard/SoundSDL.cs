/*
*
*   SDL_mixer-backed sound system that separates music and sound effects.
*   Supports MP3/WAV playback, track cleanup, and independent music/effects volume.
*
*/

using SDL;
using static SDL.SDL3;
using static SDL.SDL3_mixer;
using System;
using System.Collections.Generic;

namespace Shard
{
    unsafe class ActiveTrack
    {
        public MIX_Track* Track { get; set; }
        public string FilePath { get; set; }
        public bool IsMusic { get; set; }
        public bool Loop { get; set; }
    }

    public unsafe class SoundSDL : Sound
    {
        private static SDL_AudioDeviceID device = 0;

        private MIX_Mixer* mixer;
        private Dictionary<string, nint> cachedAudio;
        private HashSet<string> preloadedAudio;
        private List<ActiveTrack> activeEffects;
        private ActiveTrack currentMusic;
        private float musicVolume;
        private float effectsVolume;

        public SoundSDL()
        {
            cachedAudio = new Dictionary<string, nint>(StringComparer.OrdinalIgnoreCase);
            preloadedAudio = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            activeEffects = new List<ActiveTrack>();
            currentMusic = null;
            musicVolume = 1.0f;
            effectsVolume = 1.0f;

            SDL_Init(SDL_InitFlags.SDL_INIT_AUDIO);
            MIX_Init();
            ensureMixer();
        }

        public override float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = clampVolume(value);
                applyTrackVolumes();
            }
        }

        public override float EffectsVolume
        {
            get => effectsVolume;
            set
            {
                effectsVolume = clampVolume(value);
                applyTrackVolumes();
            }
        }

        public override void playSound(string file, bool loop = false)
        {
            if (ensureMixer() == false)
            {
                return;
            }

            string path = resolveFile(file);
            if (path == null)
            {
                return;
            }

            MIX_Audio* audio = loadAudio(path);
            if (audio == null)
            {
                return;
            }

            MIX_Track* track = MIX_CreateTrack(mixer);
            if (track == null)
            {
                Debug.getInstance().log("Sound warning: failed to create effect track. " + SDL_GetError(), Debug.DEBUG_LEVEL_WARNING);
                return;
            }

            MIX_SetTrackAudio(track, audio);
            MIX_SetTrackLoops(track, loop ? -1 : 0);
            MIX_SetTrackGain(track, EffectsVolume);

            if (!MIX_PlayTrack(track, 0))
            {
                Debug.getInstance().log("Sound warning: failed to play effect track. " + SDL_GetError(), Debug.DEBUG_LEVEL_WARNING);
                MIX_DestroyTrack(track);
                return;
            }

            activeEffects.Add(new ActiveTrack
            {
                Track = track,
                FilePath = path,
                IsMusic = false,
                Loop = loop
            });
        }

        public override void PlayMusic(string file, bool loop = true)
        {
            if (ensureMixer() == false)
            {
                return;
            }

            string path = resolveFile(file);
            if (path == null)
            {
                return;
            }

            MIX_Audio* audio = loadAudio(path);
            if (audio == null)
            {
                return;
            }

            StopMusic();
            currentMusic = startTrack(path, audio, true, loop, MusicVolume);
        }

        public override void StopMusic()
        {
            if (currentMusic == null || currentMusic.Track == null)
            {
                return;
            }

            MIX_StopTrack(currentMusic.Track, 0);
            MIX_DestroyTrack(currentMusic.Track);
            currentMusic = null;
        }

        public override void update()
        {
            cleanupCompletedEffects();

            if (currentMusic != null && currentMusic.Track != null && !MIX_TrackPlaying(currentMusic.Track))
            {
                string path = currentMusic.FilePath;
                bool shouldLoop = currentMusic.Loop;
                MIX_DestroyTrack(currentMusic.Track);
                currentMusic = null;

                if (shouldLoop)
                {
                    MIX_Audio* audio = loadAudio(path);
                    if (audio != null)
                    {
                        currentMusic = startTrack(path, audio, true, true, MusicVolume);
                    }
                }
            }
        }

        public override void stopAllSounds()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                stopAndDestroyTrack(activeEffects[i].Track);
            }

            activeEffects.Clear();
            StopMusic();
        }

        public override void shutdown()
        {
            stopAllSounds();

            if (mixer != null)
            {
                MIX_DestroyMixer(mixer);
                mixer = null;
            }

            if (device != 0)
            {
                SDL_CloseAudioDevice(device);
                device = 0;
            }

            cachedAudio.Clear();
            preloadedAudio.Clear();
        }

        public override void restart()
        {
            shutdown();

            SDL_Init(SDL_InitFlags.SDL_INIT_AUDIO);
            MIX_Init();
            ensureMixer();
        }

        public override void stopSound(string file)
        {
            string path = resolveFile(file);
            if (path == null)
            {
                return;
            }

            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (string.Equals(activeEffects[i].FilePath, path, StringComparison.OrdinalIgnoreCase))
                {
                    stopAndDestroyTrack(activeEffects[i].Track);
                    activeEffects.RemoveAt(i);
                }
            }

            if (currentMusic != null &&
                string.Equals(currentMusic.FilePath, path, StringComparison.OrdinalIgnoreCase))
            {
                StopMusic();
            }
        }

        public override void preloadSound(string file)
        {
            if (ensureMixer() == false)
            {
                return;
            }

            string path = resolveFile(file);
            if (path == null)
            {
                return;
            }

            if (loadAudio(path) != null)
            {
                preloadedAudio.Add(path);
            }
        }

        public override void unloadSound(string file)
        {
            string path = resolveFile(file);
            if (path == null)
            {
                return;
            }

            stopSound(file);
            destroyCachedAudio(path);
            preloadedAudio.Remove(path);
        }

        public override void clearPreloadedSounds()
        {
            List<string> toRemove = new List<string>(preloadedAudio);
            foreach (string path in toRemove)
            {
                stopSound(path);
                destroyCachedAudio(path);
            }

            preloadedAudio.Clear();
        }

        private bool ensureMixer()
        {
            if (device == 0)
            {
                device = SDL_OpenAudioDevice(SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK, null);
                if (device == 0)
                {
                    Debug.getInstance().log("Sound warning: failed to open audio device. " + SDL_GetError(), Debug.DEBUG_LEVEL_WARNING);
                    return false;
                }

                SDL_ResumeAudioDevice(device);
            }
            else if (mixer == null)
            {
                // Device exists but mixer was destroyed - need to reinitialize
                return createMixer();
            }

            if (mixer != null)
            {
                return true;
            }

            return createMixer();
        }

        private bool createMixer()
        {
            SDL_AudioSpec spec = new SDL_AudioSpec();
            if (!SDL_GetAudioDeviceFormat(device, &spec, null))
            {
                Debug.getInstance().log("Sound warning: failed to query audio device format. " + SDL_GetError(), Debug.DEBUG_LEVEL_WARNING);
                return false;
            }

            mixer = MIX_CreateMixerDevice(device, &spec);
            if (mixer == null)
            {
                Debug.getInstance().log("Sound warning: failed to create mixer. " + SDL_GetError(), Debug.DEBUG_LEVEL_WARNING);
                return false;
            }

            return true;
        }

        private MIX_Audio* loadAudio(string path)
        {
            if (cachedAudio.TryGetValue(path, out nint cached))
            {
                return (MIX_Audio*)cached;
            }

            MIX_Audio* audio = MIX_LoadAudio(mixer, path, true);
            if (audio == null)
            {
                Debug.getInstance().log("Sound warning: failed to load audio " + path + ". " + SDL_GetError(), Debug.DEBUG_LEVEL_WARNING);
                return null;
            }

            cachedAudio[path] = (nint)audio;
            return audio;
        }

        private void applyTrackVolumes()
        {
            if (currentMusic != null && currentMusic.Track != null)
            {
                MIX_SetTrackGain(currentMusic.Track, MusicVolume);
            }

            foreach (ActiveTrack effect in activeEffects)
            {
                if (effect.Track != null)
                {
                    MIX_SetTrackGain(effect.Track, EffectsVolume);
                }
            }
        }

        private void cleanupCompletedEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                ActiveTrack track = activeEffects[i];
                if (track.Track == null)
                {
                    activeEffects.RemoveAt(i);
                    continue;
                }

                if (!MIX_TrackPlaying(track.Track))
                {
                    MIX_DestroyTrack(track.Track);
                    activeEffects.RemoveAt(i);
                }
            }
        }

        private void stopAndDestroyTrack(MIX_Track* track)
        {
            if (track == null)
            {
                return;
            }

            MIX_StopTrack(track, 0);
            MIX_DestroyTrack(track);
        }

        private ActiveTrack startTrack(string path, MIX_Audio* audio, bool isMusic, bool loop, float gain)
        {
            MIX_Track* track = MIX_CreateTrack(mixer);
            if (track == null)
            {
                Debug.getInstance().log("Sound warning: failed to create " + (isMusic ? "music" : "effect") + " track. " + SDL_GetError(), Debug.DEBUG_LEVEL_WARNING);
                return null;
            }

            MIX_SetTrackAudio(track, audio);
            MIX_SetTrackLoops(track, loop ? -1 : 0);
            MIX_SetTrackGain(track, gain);

            if (!MIX_PlayTrack(track, 0))
            {
                Debug.getInstance().log("Sound warning: failed to play " + (isMusic ? "music" : "effect") + " track. " + SDL_GetError(), Debug.DEBUG_LEVEL_WARNING);
                MIX_DestroyTrack(track);
                return null;
            }

            return new ActiveTrack
            {
                Track = track,
                FilePath = path,
                IsMusic = isMusic,
                Loop = loop
            };
        }

        private void destroyCachedAudio(string path)
        {
            if (cachedAudio.TryGetValue(path, out nint audioPtr) == false)
            {
                return;
            }

            MIX_DestroyAudio((MIX_Audio*)audioPtr);
            cachedAudio.Remove(path);
        }

        private string resolveFile(string file)
        {
            if (string.IsNullOrWhiteSpace(file))
            {
                return null;
            }

            string path = Bootstrap.getAssetManager().getAssetPath(file);
            if (path != null)
            {
                return path;
            }

            return file;
        }

        private float clampVolume(float value)
        {
            return Math.Clamp(value, 0.0f, 1.0f);
        }
    }
}
