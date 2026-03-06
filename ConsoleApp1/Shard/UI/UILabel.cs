namespace Shard
{
    class UILabel : UIElement
    {
        public UILabel()
        {
            Type = "label";
            Enabled = false;
            FontSize = 24;
        }

        public override void Render()
        {
            if (Visible == false)
            {
                return;
            }

            DrawLabel(Text, ResolvedX, ResolvedY, FontSize, 240, 240, 240);
        }

        public override bool HitTest(int px, int py)
        {
            return false;
        }
    }
}
