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
    // a simple structure to manage the audio stream
    public unsafe struct ActiveStream
    {
        public SDL_AudioStream* Stream;
        public byte* Buffer; // document buffer for release
        public uint Length; // length of the buffer
        public bool IsLooping;
    }

    public unsafe class SoundSDL : Sound
    {
        private static SDL_AudioDeviceID _dev = 0;
        private List<ActiveStream> _streams = new List<ActiveStream>();

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
            SDL_AudioSpec spec;
            byte* buffer;
            uint length;

            file = Bootstrap.getAssetManager().getAssetPath(file);
            Console.WriteLine("Try to play sound: " + file);

            if (!SDL_LoadWAV(file, &spec, &buffer, &length)) return;

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

            _streams.Add(new ActiveStream { 
                Stream = stream, 
                Buffer = buffer, 
                Length = length, // Store length for looping
                IsLooping = loop 
            });
        }

        public override void update()
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
}
