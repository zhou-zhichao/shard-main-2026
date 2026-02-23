using System;

namespace Shard
{
    class UIButton : UIElement
    {
        private bool isHovered;
        private bool isPressed;

        public UIButton()
        {
            Type = "button";
            Width = 240;
            Height = 56;
            FontSize = 24;
        }

        public override void Render()
        {
            if (Visible == false)
            {
                return;
            }

            int r = 170;
            int g = 170;
            int b = 170;

            if (Enabled == false)
            {
                r = 90;
                g = 90;
                b = 90;
            }
            else if (IsPressed)
            {
                r = 90;
                g = 190;
                b = 120;
            }
            else if (IsHovered)
            {
                r = 235;
                g = 215;
                b = 100;
            }

            DrawRect(X, Y, Width, Height, r, g, b, 255);

            int textY = Y + Math.Max(2, (Height - FontSize) / 2);
            DrawLabel(Text, X + 14, textY, FontSize, 245, 245, 245);
        }

        public bool IsHovered { get => isHovered; set => isHovered = value; }
        public bool IsPressed { get => isPressed; set => isPressed = value; }
    }
}
