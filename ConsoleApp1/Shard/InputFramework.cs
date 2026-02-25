/*
*
*   SDL provides an input layer, and we're using that.  This class tracks input, anchors it to the
*       timing of the game loop, and converts the SDL events into one that is more abstract so games
*       can be written more interchangeably.
*   @author Michael Heron
*   @version 1.0
*
*/

using System;
using SDL;
using static SDL.SDL3;

namespace Shard
{

    // We'll be using SDL3 here to provide our underlying input system.
    unsafe class InputFramework : InputSystem
    {

        double tick, timeInterval;
        public override void getInput()
        {

            SDL_Event ev;
            InputEvent ie;

            tick += Bootstrap.getDeltaTime();

            if (tick < timeInterval)
            {
                return;
            }

            while (tick >= timeInterval)
            {

                bool hasEvent = SDL_PollEvent(&ev);


                if (!hasEvent)
                {
                    return;
                }

                // Convert mouse coordinates from window pixels to design resolution space
                // so that logical presentation scaling is accounted for
                IntPtr rend = Bootstrap.getDisplay().getRenderer();
                if (rend != IntPtr.Zero)
                {
                    SDL_ConvertEventToRenderCoordinates((SDL_Renderer*)rend, &ev);
                }

                ie = new InputEvent();

                if (ev.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_MOTION)
                {
                    ie.X = (int)ev.motion.x;
                    ie.Y = (int)ev.motion.y;

                    informListeners(ie, "MouseMotion");
                }

                if (ev.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_BUTTON_DOWN)
                {
                    ie.Button = (int)ev.button.button;
                    ie.X = (int)ev.button.x;
                    ie.Y = (int)ev.button.y;

                    informListeners(ie, "MouseDown");
                }

                if (ev.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_BUTTON_UP)
                {
                    ie.Button = (int)ev.button.button;
                    ie.X = (int)ev.button.x;
                    ie.Y = (int)ev.button.y;

                    informListeners(ie, "MouseUp");
                }

                if (ev.type == (uint)SDL_EventType.SDL_EVENT_MOUSE_WHEEL)
                {
                    // In SDL3, wheel direction is indicated by sign of x/y values
                    ie.X = (int)ev.wheel.x;
                    ie.Y = (int)ev.wheel.y;

                    informListeners(ie, "MouseWheel");
                }


                if (ev.type == (uint)SDL_EventType.SDL_EVENT_KEY_DOWN)
                {
                    ie.Key = (int)ev.key.scancode;

                    // F11 toggles fullscreen
                    if (ie.Key == (int)SDL_Scancode.SDL_SCANCODE_F11)
                    {
                        Bootstrap.getDisplay().toggleFullscreen();
                    }

                    // ESC returns to game launcher
                    if (ie.Key == (int)SDL_Scancode.SDL_SCANCODE_ESCAPE)
                    {
                        if (!(Bootstrap.getRunningGame() is GameLauncher))
                        {
                            Bootstrap.returnToLauncher();
                            return; // Stop processing events this frame
                        }
                    }

                    Debug.getInstance().log("Keydown: " + ie.Key);
                    informListeners(ie, "KeyDown");
                }

                if (ev.type == (uint)SDL_EventType.SDL_EVENT_KEY_UP)
                {
                    ie.Key = (int)ev.key.scancode;
                    informListeners(ie, "KeyUp");
                }

                // Handle window resize events
                if (ev.type == (uint)SDL_EventType.SDL_EVENT_WINDOW_RESIZED)
                {
                    int newW = (int)ev.window.data1;
                    int newH = (int)ev.window.data2;
                    Bootstrap.getDisplay().handleResize(newW, newH);
                }

                if (ev.type == (uint)SDL_EventType.SDL_EVENT_QUIT)
                {
                    Environment.Exit(0);
                }

                tick -= timeInterval;
            }


        }

        public override void initialize()
        {
            tick = 0;
            timeInterval = 1.0 / 60.0;
        }

    }
}
