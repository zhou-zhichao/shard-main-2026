/*
*
*   This is the implementation of the Simple Directmedia Layer through C#.   This isn't a course on
*       graphics, so we're not going to roll our own implementation.   If you wanted to replace it with
*       something using OpenGL, that'd be a pretty good extension to the base Shard engine.
*
*   Note that it extends from DisplayText, which also uses SDL.
*
*   @author Michael Heron
*   @version 1.0
*
*   Contributions to the code made by others:
*   @author Aristotelis Anthopoulos (see Changelog for 1.3.0)
*/

using SDL;
using static SDL.SDL3;
using static SDL.SDL3_image;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Shard
{

    class Line
    {
        private int sx, sy;
        private int ex, ey;
        private int r, g, b, a;

        public int Sx { get => sx; set => sx = value; }
        public int Sy { get => sy; set => sy = value; }
        public int Ex { get => ex; set => ex = value; }
        public int Ey { get => ey; set => ey = value; }
        public int R { get => r; set => r = value; }
        public int G { get => g; set => g = value; }
        public int B { get => b; set => b = value; }
        public int A { get => a; set => a = value; }
    }

    class Circle
    {
        int x, y, rad;
        private int r, g, b, a;

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public int Radius { get => rad; set => rad = value; }
        public int R { get => r; set => r = value; }
        public int G { get => g; set => g = value; }
        public int B { get => b; set => b = value; }
        public int A { get => a; set => a = value; }
    }


    unsafe class DisplaySDL : DisplayText
    {
        private List<Transform> _toDraw;
        private List<Line> _linesToDraw;
        private List<Circle> _circlesToDraw;
        private Dictionary<string, nint> spriteBuffer;

        public override void initialize()
        {
            spriteBuffer = new Dictionary<string, nint>();

            base.initialize();

            _toDraw = new List<Transform>();
            _linesToDraw = new List<Line>();
            _circlesToDraw = new List<Circle>();
        }

        public SDL_Texture* loadTexture(Transform trans)
        {
            SDL_Texture* ret;

            ret = loadTexture(trans.SpritePath);

            float w, h;
            SDL_GetTextureSize(ret, &w, &h);
            trans.Ht = (int)h;
            trans.Wid = (int)w;
            trans.recalculateCentre();

            return ret;
        }


        public SDL_Texture* loadTexture(string path)
        {
            if (spriteBuffer.ContainsKey(path))
            {
                return (SDL_Texture*)spriteBuffer[path];
            }

            SDL_Surface* img = IMG_Load(path);

            Debug.getInstance().log("IMG_Load: " + SDL_GetError());

            SDL_Texture* texture = SDL_CreateTextureFromSurface(_rend, img);
            SDL_DestroySurface(img);

            SDL_SetTextureBlendMode(texture, SDL_BlendMode.SDL_BLENDMODE_BLEND);

            spriteBuffer[path] = (nint)texture;

            return texture;
        }


        public override void addToDraw(GameObject gob)
        {
            _toDraw.Add(gob.Transform);

            if (gob.Transform.SpritePath == null)
            {
                return;
            }

            loadTexture(gob.Transform.SpritePath);
        }

        public override void removeToDraw(GameObject gob)
        {
            _toDraw.Remove(gob.Transform);
        }


        void renderCircle(int centreX, int centreY, int rad)
        {
            int dia = (rad * 2);
            byte r, g, b, a;
            int x = (rad - 1);
            int y = 0;
            int tx = 1;
            int ty = 1;
            int error = (tx - dia);

            SDL_GetRenderDrawColor(_rend, &r, &g, &b, &a);

            var points = new List<SDL_FPoint>();

            // We draw an octagon around the point, and then turn it a bit.  Do
            // that until we have an outline circle.  If you want a filled one,
            // do the same thing with an ever decreasing radius.
            while (x >= y)
            {

                points.Add(new SDL_FPoint { x = centreX + x, y = centreY - y });
                points.Add(new SDL_FPoint { x = centreX + x, y = centreY + y });
                points.Add(new SDL_FPoint { x = centreX - x, y = centreY - y });
                points.Add(new SDL_FPoint { x = centreX - x, y = centreY + y });
                points.Add(new SDL_FPoint { x = centreX + y, y = centreY - x });
                points.Add(new SDL_FPoint { x = centreX + y, y = centreY + x });
                points.Add(new SDL_FPoint { x = centreX - y, y = centreY - x });
                points.Add(new SDL_FPoint { x = centreX - y, y = centreY + x });

                if (error <= 0)
                {
                    y += 1;
                    error += ty;
                    ty += 2;
                }

                if (error > 0)
                {
                    x -= 1;
                    tx += 2;
                    error += (tx - dia);
                }

                fixed (SDL_FPoint* pointsPtr = points.ToArray())
                {
                    SDL_RenderPoints(_rend, pointsPtr, points.Count);
                }
            }
        }

        public override void drawCircle(int x, int y, int rad, int r, int g, int b, int a)
        {
            Circle c = new Circle();

            c.X = x;
            c.Y = y;
            c.Radius = rad;

            c.R = r;
            c.G = g;
            c.B = b;
            c.A = a;

            _circlesToDraw.Add(c);
        }
        public override void drawLine(int x, int y, int x2, int y2, int r, int g, int b, int a)
        {
            Line l = new Line();
            l.Sx = x;
            l.Sy = y;
            l.Ex = x2;
            l.Ey = y2;

            l.R = r;
            l.G = g;
            l.B = b;
            l.A = a;

            _linesToDraw.Add(l);
        }

        public override void display()
        {

            SDL_FRect sRect;
            SDL_FRect tRect;



            foreach (Transform trans in _toDraw)
            {

                if (trans.SpritePath == null)
                {
                    continue;
                }

                var sprite = loadTexture(trans);

                sRect.x = 0;
                sRect.y = 0;
                sRect.w = (float)(trans.Wid * trans.Scalex);
                sRect.h = (float)(trans.Ht * trans.Scaley);

                tRect.x = (float)trans.X;
                tRect.y = (float)trans.Y;
                tRect.w = sRect.w;
                tRect.h = sRect.h;

                SDL_FPoint center = new SDL_FPoint { x = sRect.w / 2, y = sRect.h / 2 };
                SDL_RenderTextureRotated(_rend, sprite, &sRect, &tRect, trans.Rotz, &center, SDL_FlipMode.SDL_FLIP_NONE);
            }

            foreach (Circle c in _circlesToDraw)
            {
                SDL_SetRenderDrawColor(_rend, (byte)c.R, (byte)c.G, (byte)c.B, (byte)c.A);
                renderCircle(c.X, c.Y, c.Radius);
            }

            foreach (Line l in _linesToDraw)
            {
                SDL_SetRenderDrawColor(_rend, (byte)l.R, (byte)l.G, (byte)l.B, (byte)l.A);
                SDL_RenderLine(_rend, l.Sx, l.Sy, l.Ex, l.Ey);
            }

            // Show it off.
            base.display();


        }

        public override void clearDisplay()
        {

            _toDraw.Clear();
            _circlesToDraw.Clear();
            _linesToDraw.Clear();

            base.clearDisplay();
        }

    }


}
