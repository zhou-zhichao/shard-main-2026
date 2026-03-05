/*
*
*   A refined sound system implementation using SDL3. Now supports looping, volume control and audio streaming.
*   @author Yuxi Guo
*   @version 1.1
*
*/

using SDL;
using static SDL.SDL3;
using System;
using System.Collections.Generic;

namespace Shard
{
    // a simple structure to hold preloaded audio data
    public unsafe struct PreloadedAudio
    {
        public byte* Buffer;
        public SDL_AudioSpec Spec;
        public uint Length;
    }

    // a simple structure to manage the audio stream
    public unsafe struct ActiveStream
    {
        public SDL_AudioStream* Stream;
        public byte* Buffer; // document buffer for release
        public uint Length; // length of the buffer
        public bool IsLooping;
        public string FilePath;
    }

    public unsafe class SoundSDL : Sound
    {
        private static SDL_AudioDeviceID _dev = 0;
        private readonly List<ActiveStream> _streams = new List<ActiveStream>();
        private readonly object _streamLock = new object();
        
        // preloaded audio cache: file path -> audio data
        private readonly Dictionary<string, PreloadedAudio> _preloadedSounds = new Dictionary<string, PreloadedAudio>();
        private readonly object _preloadLock = new object();

        public SoundSDL()
        {
            SDL_Init(SDL_InitFlags.SDL_INIT_AUDIO);
            
            // The default playback device is enabled only once during the initialization
            if (_dev == 0)
            {
                _dev = SDL_OpenAudioDevice(SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK, null);
                if (_dev == 0)
                {
                    _dev = SDL_OpenAudioDevice(SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK, null);
                    if (_dev != 0)
                    {
                        SDL_ResumeAudioDevice(_dev);
                        Debug.getInstance().log("Audio device opened successfully");
                    } 
                    else
                    {
                        Console.WriteLine("Failed to open audio device:"+ SDL_GetError());
                    }
                }
            }
        }

        public override void playSound(string file, bool loop = false)
        {
            if (_dev == 0)
            {
                Console.WriteLine("Audio device is not available, cannot play sound.");
                return;
            }

            SDL_AudioSpec spec;
            byte* buffer;
            uint length;

            file = Bootstrap.getAssetManager().getAssetPath(file);
            Console.WriteLine("Try to play sound: " + file);

            // Check if the sound is preloaded, use cached version if available
            bool usePreloaded = false;
            lock (_preloadLock)
            {
                if (_preloadedSounds.TryGetValue(file, out var preloaded))
                {
                    // Copy the preloaded data for playback (we can't reuse the same buffer directly
                    // because each stream needs its own buffer to manage)
                    buffer = (byte*)SDL_malloc(preloaded.Length);
                    if (buffer != null)
                    {
                        System.Buffer.MemoryCopy(preloaded.Buffer, buffer, preloaded.Length, preloaded.Length);
                        spec = preloaded.Spec;
                        length = preloaded.Length;
                        usePreloaded = true;
                    }
                }
            }

            if (!usePreloaded)
            {
                // Load from file if not preloaded
                if (!SDL_LoadWAV(file, &spec, &buffer, &length))
                {
                    Console.WriteLine("Failed to load WAV: " + SDL_GetError());
                    return;
                }
            }

            SDL_AudioStream* stream = SDL_CreateAudioStream(&spec, &spec);
            if (stream == null) {
                SDL_free(buffer);
                return;
            }
            
            // Initialize the audio stream with the volume
            SDL_SetAudioStreamGain(stream, this.Volume);
            SDL_BindAudioStream(_dev, stream);
            
            // First-time data population
            SDL_PutAudioStreamData(stream, (nint)buffer, (int)length);

            lock (_streamLock)
            {
                _streams.Add(new ActiveStream
                {
                    Stream = stream,
                    Buffer = buffer,
                    Length = length, // Store length for looping
                    IsLooping = loop,
                    FilePath = file
                });
            }
        }

        public override void update()
        {
            lock (_streamLock)
            {
                for (int i = _streams.Count - 1; i >= 0; i--)
                {
                    var active = _streams[i];

                    // Set the volume of the audio stream
                    SDL_SetAudioStreamGain(active.Stream, this.Volume);
                    // Check the number of pending bytes in the playback queue
                    if (SDL_GetAudioStreamQueued(active.Stream) == 0)
                    {
                        if (active.IsLooping)
                        {
                            // If looping is required, re-feed the buffered backup
                            SDL_PutAudioStreamData(active.Stream, (nint)active.Buffer, (int)active.Length);
                        }
                        else
                        {
                            // clean up
                            SDL_DestroyAudioStream(active.Stream);
                            SDL_free(active.Buffer);
                            _streams.RemoveAt(i);
                        }
                    }
                }
            }
        }

        public override void stopAllSounds()
        {
            lock (_streamLock)
            {
                for (int i = 0; i < _streams.Count; i++)
                {
                    var active = _streams[i];
                    SDL_DestroyAudioStream(active.Stream);
                    SDL_free(active.Buffer);
                }
                _streams.Clear();
            }
        }

        public override void stopSound(string file)
        {
            file = Bootstrap.getAssetManager().getAssetPath(file);

            lock (_streamLock)
            {
                for (int i = _streams.Count - 1; i >= 0; i--)
                {
                    var active = _streams[i];
                    if (string.Equals(active.FilePath, file, StringComparison.OrdinalIgnoreCase))
                    {
                        SDL_DestroyAudioStream(active.Stream);
                        SDL_free(active.Buffer);
                        _streams.RemoveAt(i);
                    }
                }
            }
        }

        public override void preloadSound(string file)
        {
            if (_dev == 0)
            {
                Console.WriteLine("Audio device is not available, cannot preload sound.");
                return;
            }

            file = Bootstrap.getAssetManager().getAssetPath(file);

            lock (_preloadLock)
            {
                if (_preloadedSounds.ContainsKey(file))
                {
                    Console.WriteLine("Sound already preloaded: " + file);
                    return;
                }
            }

            SDL_AudioSpec spec;
            byte* buffer;
            uint length;

            if (!SDL_LoadWAV(file, &spec, &buffer, &length))
            {
                Console.WriteLine("Failed to preload WAV: " + SDL_GetError());
                return;
            }

            lock (_preloadLock)
            {
                _preloadedSounds[file] = new PreloadedAudio
                {
                    Buffer = buffer,
                    Spec = spec,
                    Length = length
                };
            }
            Console.WriteLine("Preloaded sound: " + file);
        }

        public override void unloadSound(string file)
        {
            file = Bootstrap.getAssetManager().getAssetPath(file);

            lock (_preloadLock)
            {
                if (_preloadedSounds.TryGetValue(file, out var preloaded))
                {
                    SDL_free(preloaded.Buffer);
                    _preloadedSounds.Remove(file);
                    Console.WriteLine("Unloaded sound: " + file);
                }
            }
        }

        public override void clearPreloadedSounds()
        {
            lock (_preloadLock)
            {
                foreach (var kvp in _preloadedSounds)
                {
                    SDL_free(kvp.Value.Buffer);
                }
                _preloadedSounds.Clear();
                Console.WriteLine("Cleared all preloaded sounds");
            }
        }

    }
}
