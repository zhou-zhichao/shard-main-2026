using System;
using System.Collections.Generic;

namespace Shard
{
    class UIDropdown : UIElement
    {
        private List<string> options;
        private int selectedIndex;
        private bool expanded;
        private bool isHovered;
        private int hoveredOptionIndex;

        public UIDropdown()
        {
            Type = "dropdown";
            Width = 240;
            Height = 48;
            FontSize = 20;
            options = new List<string>();
            selectedIndex = 0;
            expanded = false;
            isHovered = false;
            hoveredOptionIndex = -1;
        }

        public override void Render()
        {
            if (Visible == false)
            {
                return;
            }

            int rowHeight = Math.Max(Height, 24);

            int r = 170;
            int g = 170;
            int b = 170;

            if (Enabled == false)
            {
                r = 90;
                g = 90;
                b = 90;
            }
            else if (IsHovered)
            {
                r = 235;
                g = 215;
                b = 100;
            }

            DrawRect(X, Y, Width, rowHeight, r, g, b, 255);

            string label;
            if (string.IsNullOrWhiteSpace(Text))
            {
                label = SelectedOption;
            }
            else
            {
                label = Text + ": " + SelectedOption;
            }

            DrawLabel(label, X + 12, Y + Math.Max(2, (rowHeight - FontSize) / 2), FontSize, 245, 245, 245);
            DrawLabel(Expanded ? "^" : "v", X + Width - 22, Y + Math.Max(2, (rowHeight - FontSize) / 2), FontSize, 245, 245, 245);

            if (Expanded == false)
            {
                return;
            }

            int startY = Y + rowHeight;

            for (int i = 0; i < options.Count; i++)
            {
                int rowY = startY + (i * rowHeight);
                int rowR = 130;
                int rowG = 130;
                int rowB = 130;

                if (i == selectedIndex)
                {
                    rowR = 90;
                    rowG = 170;
                    rowB = 220;
                }

                if (i == hoveredOptionIndex)
                {
                    rowR = 220;
                    rowG = 180;
                    rowB = 90;
                }

                DrawRect(X, rowY, Width, rowHeight, rowR, rowG, rowB, 255);
                DrawLabel(options[i], X + 12, rowY + Math.Max(2, (rowHeight - FontSize) / 2), FontSize, 245, 245, 245);
            }
        }

        public int GetRowHeight()
        {
            return Math.Max(Height, 24);
        }

        public int GetExpandedRenderHeight()
        {
            if (Expanded == false)
            {
                return GetRowHeight();
            }

            EnsureOptions();
            return GetRowHeight() * (Options.Count + 1);
        }

        public bool HitTestOption(int px, int py, out int index)
        {
            index = -1;

            if (Visible == false || Enabled == false || Expanded == false)
            {
                return false;
            }

            int rowHeight = Math.Max(Height, 24);
            int startY = Y + rowHeight;

            if (px < X || px > X + Width || py < startY)
            {
                return false;
            }

            int relativeY = py - startY;
            int optionIndex = relativeY / rowHeight;

            if (optionIndex < 0 || optionIndex >= options.Count)
            {
                return false;
            }

            index = optionIndex;
            return true;
        }

        public void SetSelectedIndex(int idx)
        {
            EnsureOptions();
            selectedIndex = Math.Clamp(idx, 0, options.Count - 1);
        }

        public void EnsureOptions()
        {
            if (options.Count == 0)
            {
                options.Add("N/A");
            }
        }

        public string SelectedOption
        {
            get
            {
                EnsureOptions();
                if (selectedIndex < 0 || selectedIndex >= options.Count)
                {
                    selectedIndex = 0;
                }

                return options[selectedIndex];
            }
        }

        public List<string> Options { get => options; set => options = value ?? new List<string>(); }
        public int SelectedIndex { get => selectedIndex; set => SetSelectedIndex(value); }
        public bool Expanded { get => expanded; set => expanded = value; }
        public bool IsHovered { get => isHovered; set => isHovered = value; }
        public int HoveredOptionIndex { get => hoveredOptionIndex; set => hoveredOptionIndex = value; }
    }
}
