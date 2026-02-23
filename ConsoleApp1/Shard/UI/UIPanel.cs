namespace Shard
{
    class UIPanel : UIElement
    {
        public UIPanel()
        {
            Type = "panel";
            FontSize = 28;
        }

        public override void Render()
        {
            if (Visible == false)
            {
                return;
            }

            DrawRect(X, Y, Width, Height, 110, 170, 220, 255);

            if (string.IsNullOrWhiteSpace(Text) == false)
            {
                DrawLabel(Text, X + 16, Y + 16, FontSize, 230, 240, 255);
            }
        }
    }
}
