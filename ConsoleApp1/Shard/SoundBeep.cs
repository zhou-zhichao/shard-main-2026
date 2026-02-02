/*
*
*   A very simple implementation of a very simple sound system.
*   @author Michael Heron
*   @version 1.0
*
*/

using SDL;
using static SDL.SDL3;
using System;

namespace Shard
{
    public unsafe class SoundSDL : Sound
    {

        public override void playSound(string file)
        {
            SDL_AudioSpec spec;
            byte* buffer;
            uint length;

            file = Bootstrap.getAssetManager().getAssetPath(file);

            // Load WAV file
            if (!SDL_LoadWAV(file, &spec, &buffer, &length))
            {
                Debug.getInstance().log("SDL_LoadWAV error: " + SDL_GetError());
                return;
            }

            // Open audio device with default output
            SDL_AudioDeviceID dev = SDL_OpenAudioDevice(SDL_AUDIO_DEVICE_DEFAULT_PLAYBACK, &spec);

            if (dev == 0)
            {
                Debug.getInstance().log("SDL_OpenAudioDevice error: " + SDL_GetError());
                SDL_free(buffer);
                return;
            }

            // Create audio stream and bind to device
            SDL_AudioStream* stream = SDL_CreateAudioStream(&spec, &spec);
            if (stream == null)
            {
                Debug.getInstance().log("SDL_CreateAudioStream error: " + SDL_GetError());
                SDL_CloseAudioDevice(dev);
                SDL_free(buffer);
                return;
            }

            SDL_BindAudioStream(dev, stream);

            // Put audio data into stream
            SDL_PutAudioStreamData(stream, (nint)buffer, (int)length);

            // Resume playback
            SDL_ResumeAudioDevice(dev);

            // Clean up buffer (stream has its own copy)
            SDL_free(buffer);

            // Note: For proper cleanup, you would need to track the stream/device
            // and clean them up when audio finishes playing
        }

    }
}
