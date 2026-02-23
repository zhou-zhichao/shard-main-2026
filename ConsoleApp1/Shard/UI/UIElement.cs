using System;

namespace Shard
{
    class UIElement
    {
        private string id;
        private string type;
        private int x;
        private int y;
        private int width;
        private int height;
        private bool visible;
        private bool enabled;
        private string text;
        private int fontSize;
        private string actionId;

        public UIElement()
        {
            id = "";
            type = "element";
            x = 0;
            y = 0;
            width = 0;
            height = 0;
            visible = true;
            enabled = true;
            text = "";
            fontSize = 20;
            actionId = "";
        }

        public virtual void Render()
        {
        }

        public virtual bool HitTest(int px, int py)
        {
            if (Visible == false || Enabled == false)
            {
                return false;
            }

            if (Width <= 0 || Height <= 0)
            {
                return false;
            }

            return px >= X && px <= X + Width && py >= Y && py <= Y + Height;
        }

        protected static void DrawRect(int x, int y, int width, int height, int r, int g, int b, int a)
        {
            if (width <= 0 || height <= 0)
            {
                return;
            }

            Bootstrap.getDisplay().drawLine(x, y, x + width, y, r, g, b, a);
            Bootstrap.getDisplay().drawLine(x + width, y, x + width, y + height, r, g, b, a);
            Bootstrap.getDisplay().drawLine(x + width, y + height, x, y + height, r, g, b, a);
            Bootstrap.getDisplay().drawLine(x, y + height, x, y, r, g, b, a);
        }

        protected static void DrawLabel(string text, int x, int y, int size, int r, int g, int b)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            Bootstrap.getDisplay().showText(text, x, y, Math.Max(size, 8), r, g, b);
        }

        public string Id { get => id; set => id = value; }
        public string Type { get => type; set => type = value; }
        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }
        public int Width { get => width; set => width = value; }
        public int Height { get => height; set => height = value; }
        public bool Visible { get => visible; set => visible = value; }
        public bool Enabled { get => enabled; set => enabled = value; }
        public string Text { get => text; set => text = value; }
        public int FontSize { get => fontSize; set => fontSize = value; }
        public string ActionId { get => actionId; set => actionId = value; }
    }
}
