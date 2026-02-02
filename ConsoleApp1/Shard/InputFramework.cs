/*
*
*   SDL provides an input layer, and we're using that.  This class tracks input, anchors it to the
*       timing of the game loop, and converts the SDL events into one that is more abstract so games
*       can be written more interchangeably.
*   @author Michael Heron
*   @version 1.0
*
*/

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
                    Debug.getInstance().log("Keydown: " + ie.Key);
                    informListeners(ie, "KeyDown");
                }

                if (ev.type == (uint)SDL_EventType.SDL_EVENT_KEY_UP)
                {
                    ie.Key = (int)ev.key.scancode;
                    informListeners(ie, "KeyUp");
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
