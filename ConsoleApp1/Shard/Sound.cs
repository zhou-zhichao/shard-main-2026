/*
*
*   This class is the base class for all sound systems. It provides a common interface for playing sounds and updating the sound system.
*   @author Yuxi Guo
*   @version 1.1
*   
*/

namespace Shard
{
    // Audio Group class for managing collections of sounds
    public class AudioGroup
    {
        public string Name { get; internal set; }
        public float Volume { get; set; }
        public bool Paused { get; internal set; }

        public AudioGroup(string name)
        {
            Name = name;
            Volume = 1.0f;
            Paused = false;
        }

        internal AudioGroup(string name, float volume) : this(name)
        {
            Volume = volume;
        }
    }

    abstract public class Sound
    {
        public virtual float Volume
        {
            get => EffectsVolume;
            set => EffectsVolume = value;
        }

        public abstract float MusicVolume { get; set; }
        public abstract float EffectsVolume { get; set; }

        // Master volume controls the overall output gain of the mixer
        public virtual float MasterVolume
        {
            get => 1.0f;
            set { }
        }

        // Pause all currently playing sounds
        abstract public void PauseAll();

        // Resume all paused sounds
        abstract public void ResumeAll();

        // Set stereo panning for a sound (2D games)
        // pan: -1.0 = full left, 0 = center, 1.0 = full right
        abstract public void setSoundPan(string file, float pan);

        // Clear stereo panning, restore to default
        abstract public void clearSoundPan(string file);

        // Audio Group Management
        // Create a new audio group with the specified name
        abstract public AudioGroup createGroup(string name);

        // Get an existing audio group by name
        abstract public AudioGroup getGroup(string name);

        // Play a sound and assign it to a specific group
        abstract public void playSound(string file, string groupName, bool loop = false);

        // Play a sound with specific group and volume
        abstract public void playSound(string file, string groupName, float volume, bool loop = false);

        // Pause all sounds in a specific group
        abstract public void pauseGroup(string groupName);

        // Resume all sounds in a specific group
        abstract public void resumeGroup(string groupName);

        // Stop all sounds in a specific group
        abstract public void stopGroup(string groupName);

        // add loop to support BGM
        abstract public void playSound(string file, bool loop = false);
        abstract public void PlayMusic(string file, bool loop = true);
        abstract public void StopMusic();

        // add per frame update method to clean up resources that have finished playback
        abstract public void update();

        // stop all currently playing sounds
        abstract public void stopAllSounds();

        // stop all sounds started from the specified file
        abstract public void stopSound(string file);

        // Play a sound and return its handle for spatial audio control
        // Returns track handle >= 0 on success, -1 on failure
        abstract public int playSoundWithHandle(string file, float volume, bool loop = false);

        // Set volume for a specific track by handle
        abstract public void setTrackVolume(int trackHandle, float volume);

        // Stop a specific track by handle
        abstract public void stopTrack(int trackHandle);

        // preload a sound file into memory for faster playback
        abstract public void preloadSound(string file);

        // unload a preloaded sound from memory
        abstract public void unloadSound(string file);

        // unload all preloaded sounds from memory
        abstract public void clearPreloadedSounds();

        // shutdown the sound system completely
        abstract public void shutdown();

        // restart the sound system after shutdown
        abstract public void restart();
    }
}
