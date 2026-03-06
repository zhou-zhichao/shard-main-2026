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

            DrawRect(ResolvedX, ResolvedY, Width, Height, 110, 170, 220, 255);

            if (string.IsNullOrWhiteSpace(Text) == false)
            {
                DrawLabel(Text, ResolvedX + 16, ResolvedY + 16, FontSize, 230, 240, 255);
            }
        }
    }
}
