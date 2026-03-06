/*
*
*   The abstract display class setting out the consistent interface all display implementations need.  
*   @author Michael Heron
*   @version 1.0
*   
*/

using System;
using System.Drawing;

namespace Shard
{
    abstract class Display
    {
        protected int _height, _width;
        protected int _designWidth = 1280;
        protected int _designHeight = 864;
        protected float _scaleX = 1.0f;
        protected float _scaleY = 1.0f;
        protected bool _isFullscreen = false;
        protected int _clearR = 0;
        protected int _clearG = 0;
        protected int _clearB = 0;
        protected int _clearA = 255;

        public virtual void drawLine(int x, int y, int x2, int y2, int r, int g, int b, int a)
        {
        }

        public virtual void drawLine(int x, int y, int x2, int y2, Color col)
        {
            drawLine(x, y, x2, y2, col.R, col.G, col.B, col.A);
        }


        public virtual void drawCircle(int x, int y, int rad, int r, int g, int b, int a)
        {
        }

        public virtual void drawCircle(int x, int y, int rad, Color col)
        {
            drawCircle(x, y, rad, col.R, col.G, col.B, col.A);
        }

        public virtual void drawFilledCircle(int x, int y, int rad, Color col)
        {
            drawFilledCircle(x, y, rad, col.R, col.G, col.B, col.A);
        }

        public virtual void drawFilledCircle(int x, int y, int rad, int r, int g, int b, int a)
        {
            while (rad > 0)
            {
                drawCircle(x, y, rad, r, g, b, a);
                rad -= 1;
            }
        }

        public virtual void drawFilledRect(int x, int y, int width, int height, int r, int g, int b, int a)
        {
            for (int row = 0; row < height; row++)
            {
                drawLine(x, y + row, x + width, y + row, r, g, b, a);
            }
        }

        public virtual void setClearColor(int r, int g, int b, int a = 255)
        {
            _clearR = Math.Clamp(r, 0, 255);
            _clearG = Math.Clamp(g, 0, 255);
            _clearB = Math.Clamp(b, 0, 255);
            _clearA = Math.Clamp(a, 0, 255);
        }

        public void setClearColor(Color col)
        {
            setClearColor(col.R, col.G, col.B, col.A);
        }

        public void showText(string text, double x, double y, int size, Color col)
        {
            showText(text, x, y, size, col.R, col.G, col.B);
        }



        public virtual void setFullscreen()
        {
        }

        public virtual void toggleFullscreen()
        {
        }

        public virtual void setFullscreen(bool fullscreen)
        {
            if (fullscreen)
            {
                setFullscreen();
            }
            else if (_isFullscreen)
            {
                toggleFullscreen();
            }
        }

        public virtual void handleResize(int newW, int newH)
        {
            _width = newW;
            _height = newH;
            updateViewport();
        }

        public virtual void setWindowSize(int w, int h)
        {
            setSize(w, h);
            updateViewport();
        }

        public void setDesignResolution(int w, int h)
        {
            _designWidth = w;
            _designHeight = h;
            updateViewport();
        }

        protected virtual void updateViewport()
        {
            if (_designWidth > 0 && _designHeight > 0)
            {
                _scaleX = (float)_width / _designWidth;
                _scaleY = (float)_height / _designHeight;
            }
        }

        public int getDesignWidth() { return _designWidth; }
        public int getDesignHeight() { return _designHeight; }
        public float getScaleX() { return _scaleX; }
        public float getScaleY() { return _scaleY; }
        public bool isFullscreen() { return _isFullscreen; }
        public virtual IntPtr getRenderer() { return IntPtr.Zero; }

        public virtual void addToDraw(GameObject gob)
        {
        }

        public virtual void removeToDraw(GameObject gob)
        {
        }
        public int getHeight()
        {
            return _height;
        }

        public int getWidth()
        {
            return _width;
        }

        public virtual void setSize(int w, int h)
        {
            _height = h;
            _width = w;
        }

        public abstract void initialize();
        public abstract void clearDisplay();
        public abstract void display();

        public abstract void showText(string text, double x, double y, int size, int r, int g, int b);
        public abstract void showText(char[,] text, double x, double y, int size, int r, int g, int b);
    }
}
