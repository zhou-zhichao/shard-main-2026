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
        public int Id { get; set; }
        public MIX_Track* Track { get; set; }
        public string FilePath { get; set; }
        public bool IsMusic { get; set; }
        public bool Loop { get; set; }
        public float Pan { get; set; }  // -1.0 = left, 0 = center, 1.0 = right
        public AudioGroup Group { get; set; }  // Associated audio group
        public float BaseVolume { get; set; }  // Base volume for spatial audio
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
        private float masterVolume;
        private bool isPaused;
        private Dictionary<string, AudioGroup> audioGroups;
        private int nextTrackId;
        private int nextGroupId;

        public SoundSDL()
        {
            cachedAudio = new Dictionary<string, nint>(StringComparer.OrdinalIgnoreCase);
            preloadedAudio = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            activeEffects = new List<ActiveTrack>();
            currentMusic = null;
            musicVolume = 1.0f;
            effectsVolume = 1.0f;
            masterVolume = 1.0f;
            isPaused = false;
            audioGroups = new Dictionary<string, AudioGroup>(StringComparer.OrdinalIgnoreCase);
            nextTrackId = 0;
            nextGroupId = 0;

            SDL_Init(SDL_InitFlags.SDL_INIT_AUDIO);
            MIX_Init();
            ensureMixer();

            // Create default audio groups
            createDefaultGroups();
        }

        private void createDefaultGroups()
        {
            // Create default groups: Music, Effects, Ambient, UI, Voice
            createGroup("Music");
            createGroup("Effects");
            createGroup("Ambient");
            createGroup("UI");
            createGroup("Voice");
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

        public override float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = clampVolume(value);
                applyMasterVolume();
            }
        }

        public override void PauseAll()
        {
            if (!isPaused && mixer != null)
            {
                MIX_PauseAllTracks(mixer);
                isPaused = true;
            }
        }

        public override void ResumeAll()
        {
            if (isPaused && mixer != null)
            {
                MIX_ResumeAllTracks(mixer);
                isPaused = false;
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
                Loop = loop,
                Pan = 0.0f
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

            if (currentMusic != null && currentMusic.Track != null && !MIX_TrackPlaying(currentMusic.Track) && !isPaused)
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
            isPaused = false;
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

        public override int playSoundWithHandle(string file, float volume, bool loop = false)
        {
            if (ensureMixer() == false)
            {
                return -1;
            }

            string path = resolveFile(file);
            if (path == null)
            {
                return -1;
            }

            MIX_Audio* audio = loadAudio(path);
            if (audio == null)
            {
                return -1;
            }

            MIX_Track* track = MIX_CreateTrack(mixer);
            if (track == null)
            {
                Debug.getInstance().log("Sound warning: failed to create spatial track. " + SDL_GetError(), Debug.DEBUG_LEVEL_WARNING);
                return -1;
            }

            MIX_SetTrackAudio(track, audio);
            MIX_SetTrackLoops(track, loop ? -1 : 0);
            MIX_SetTrackGain(track, volume * effectsVolume * masterVolume);

            if (!MIX_PlayTrack(track, 0))
            {
                Debug.getInstance().log("Sound warning: failed to play spatial track. " + SDL_GetError(), Debug.DEBUG_LEVEL_WARNING);
                MIX_DestroyTrack(track);
                return -1;
            }

            int trackId = nextTrackId++;
            activeEffects.Add(new ActiveTrack
            {
                Id = trackId,
                Track = track,
                FilePath = path,
                IsMusic = false,
                Loop = loop,
                Pan = 0.0f,
                BaseVolume = volume
            });

            return trackId;
        }

        public override void setTrackVolume(int trackHandle, float volume)
        {
            if (trackHandle < 0)
            {
                return;
            }

            foreach (ActiveTrack effect in activeEffects)
            {
                if (effect.Id == trackHandle && effect.Track != null)
                {
                    float finalVolume = volume * effect.BaseVolume * effectsVolume * masterVolume;
                    MIX_SetTrackGain(effect.Track, finalVolume);
                    break;
                }
            }
        }

        public override void stopTrack(int trackHandle)
        {
            if (trackHandle < 0)
            {
                return;
            }

            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i].Id == trackHandle)
                {
                    stopAndDestroyTrack(activeEffects[i].Track);
                    activeEffects.RemoveAt(i);
                    break;
                }
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

            applyMasterVolume();
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

        private void applyMasterVolume()
        {
            if (mixer != null)
            {
                MIX_SetMixerGain(mixer, masterVolume);
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

                // Only cleanup tracks that are not set to loop
                if (!track.Loop && !MIX_TrackPlaying(track.Track))
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
                Loop = loop,
                Pan = 0.0f
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

        private float clampPan(float value)
        {
            return Math.Clamp(value, -1.0f, 1.0f);
        }

        public override void setSoundPan(string file, float pan)
        {
            string path = resolveFile(file);
            if (path == null)
            {
                return;
            }

            float clampedPan = clampPan(pan);

            // Check active effects
            foreach (ActiveTrack effect in activeEffects)
            {
                if (string.Equals(effect.FilePath, path, StringComparison.OrdinalIgnoreCase) && effect.Track != null)
                {
                    effect.Pan = clampedPan;
                    applyTrackPan(effect.Track, clampedPan);
                    return;
                }
            }

            // Check current music
            if (currentMusic != null &&
                string.Equals(currentMusic.FilePath, path, StringComparison.OrdinalIgnoreCase) &&
                currentMusic.Track != null)
            {
                currentMusic.Pan = clampedPan;
                applyTrackPan(currentMusic.Track, clampedPan);
            }
        }

        public override void clearSoundPan(string file)
        {
            setSoundPan(file, 0.0f);
        }

        private void applyTrackPan(MIX_Track* track, float pan)
        {
            if (track == null)
            {
                return;
            }

            // Convert -1.0 to 1.0 range to left/right gains
            // pan = -1.0 -> left gain = 1.0, right gain = 0.0 (full left)
            // pan = 0.0 -> left gain = 1.0, right gain = 1.0 (center)
            // pan = 1.0 -> left gain = 0.0, right gain = 1.0 (full right)
            float leftGain = Math.Clamp(1.0f - pan, 0.0f, 1.0f);
            float rightGain = Math.Clamp(1.0f + pan, 0.0f, 1.0f);

            MIX_StereoGains gains = new MIX_StereoGains
            {
                left = leftGain,
                right = rightGain
            };

            if (!MIX_SetTrackStereo(track, &gains))
            {
                Debug.getInstance().log("Sound warning: failed to set stereo panning. " + SDL_GetError(), Debug.DEBUG_LEVEL_WARNING);
            }
        }

        public override AudioGroup createGroup(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.getInstance().log("Sound warning: cannot create group with empty name", Debug.DEBUG_LEVEL_WARNING);
                return null;
            }

            if (audioGroups.ContainsKey(name))
            {
                Debug.getInstance().log("Sound warning: group '" + name + "' already exists", Debug.DEBUG_LEVEL_WARNING);
                return audioGroups[name];
            }

            // Create audio group (SDL3_mixer groups are used for post-processing, not pause/resume)
            AudioGroup audioGroup = new AudioGroup(name, 1.0f);
            audioGroups[name] = audioGroup;

            return audioGroup;
        }

        public override AudioGroup getGroup(string name)
        {
            if (audioGroups.TryGetValue(name, out AudioGroup group))
            {
                return group;
            }
            return null;
        }

        public override void playSound(string file, string groupName, bool loop = false)
        {
            playSound(file, groupName, 1.0f, loop);
        }

        public override void playSound(string file, string groupName, float volume, bool loop = false)
        {
            AudioGroup group = getGroup(groupName);
            if (group == null)
            {
                // Auto-create group if doesn't exist
                group = createGroup(groupName);
                if (group == null)
                {
                    // Fallback to Effects group
                    group = getGroup("Effects");
                    if (group == null)
                    {
                        return;
                    }
                }
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

            // Create track
            MIX_Track* track = MIX_CreateTrack(mixer);
            if (track == null)
            {
                Debug.getInstance().log("Sound warning: failed to create track for " + path + ". " + SDL_GetError(), Debug.DEBUG_LEVEL_WARNING);
                return;
            }

            MIX_SetTrackAudio(track, audio);
            MIX_SetTrackLoops(track, loop ? -1 : 0);

            // Calculate and apply volume (group volume * effects volume * master volume)
            // If group is paused, volume is 0
            float finalVolume = (group.Paused ? 0.0f : volume * group.Volume * effectsVolume * masterVolume);
            MIX_SetTrackGain(track, finalVolume);

            // Play the track
            if (!MIX_PlayTrack(track, 0))
            {
                Debug.getInstance().log("Sound warning: failed to play effect track. " + SDL_GetError(), Debug.DEBUG_LEVEL_WARNING);
                MIX_DestroyTrack(track);
                return;
            }

            // Add to active effects
            activeEffects.Add(new ActiveTrack
            {
                Track = track,
                FilePath = path,
                IsMusic = false,
                Loop = loop,
                Pan = 0.0f,
                Group = group
            });
        }

        public override void pauseGroup(string groupName)
        {
            AudioGroup group = getGroup(groupName);
            if (group == null)
            {
                return;
            }

            group.Paused = true;

            // Set all tracks in this group to volume 0
            foreach (ActiveTrack effect in activeEffects)
            {
                if (effect.Group != null && string.Equals(effect.Group.Name, groupName, StringComparison.OrdinalIgnoreCase) && effect.Track != null)
                {
                    MIX_SetTrackGain(effect.Track, 0.0f);
                }
            }
        }

        public override void resumeGroup(string groupName)
        {
            AudioGroup group = getGroup(groupName);
            if (group == null)
            {
                return;
            }

            group.Paused = false;

            // Restore volume for all tracks in this group
            foreach (ActiveTrack effect in activeEffects)
            {
                if (effect.Group != null && string.Equals(effect.Group.Name, groupName, StringComparison.OrdinalIgnoreCase) && effect.Track != null)
                {
                    float finalVolume = effect.Group.Volume * effectsVolume * masterVolume;
                    MIX_SetTrackGain(effect.Track, finalVolume);
                }
            }
        }

        public override void stopGroup(string groupName)
        {
            AudioGroup group = getGroup(groupName);
            if (group == null)
            {
                return;
            }

            // Stop all tracks in this group
            List<ActiveTrack> toRemove = new List<ActiveTrack>();
            foreach (ActiveTrack effect in activeEffects)
            {
                if (effect.Group != null && string.Equals(effect.Group.Name, groupName, StringComparison.OrdinalIgnoreCase) && effect.Track != null)
                {
                    MIX_StopTrack(effect.Track, 0);
                    MIX_DestroyTrack(effect.Track);
                    toRemove.Add(effect);
                }
            }

            foreach (ActiveTrack effect in toRemove)
            {
                activeEffects.Remove(effect);
            }
        }
    }
}
