using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Shard
{
    class UILayoutFile
    {
        public List<UIScreenDefinition> Screens { get; set; } = new List<UIScreenDefinition>();
    }

    class UIScreenDefinition
    {
        public string Id { get; set; }
        public List<UIElementDefinition> Elements { get; set; } = new List<UIElementDefinition>();
    }

    class UIElementDefinition
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Text { get; set; }
        public int FontSize { get; set; }
        public string Action { get; set; }
        public string AnchorX { get; set; }
        public string AnchorY { get; set; }
        public List<string> Options { get; set; } = new List<string>();
        public int DefaultIndex { get; set; }
        public bool? Visible { get; set; }
        public bool? Enabled { get; set; }
    }

    class UILayoutCatalog
    {
        public static Dictionary<string, UIScreen> Load(string path)
        {
            Dictionary<string, UIScreen> screens = new Dictionary<string, UIScreen>();

            if (string.IsNullOrWhiteSpace(path) || File.Exists(path) == false)
            {
                Debug.getInstance().log("UILayout warning: layout file not found: " + path, Debug.DEBUG_LEVEL_WARNING);
                return screens;
            }

            try
            {
                string raw = File.ReadAllText(path);
                UILayoutFile data = JsonSerializer.Deserialize<UILayoutFile>(raw, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (data == null || data.Screens == null)
                {
                    Debug.getInstance().log("UILayout warning: screens section missing.", Debug.DEBUG_LEVEL_WARNING);
                    return screens;
                }

                foreach (UIScreenDefinition screenDef in data.Screens)
                {
                    if (screenDef == null || string.IsNullOrWhiteSpace(screenDef.Id))
                    {
                        Debug.getInstance().log("UILayout warning: screen with empty id skipped.", Debug.DEBUG_LEVEL_WARNING);
                        continue;
                    }

                    UIScreen screen = new UIScreen(screenDef.Id.Trim());

                    if (screenDef.Elements != null)
                    {
                        foreach (UIElementDefinition elementDef in screenDef.Elements)
                        {
                            UIElement element = BuildElement(elementDef);

                            if (element == null)
                            {
                                continue;
                            }

                            screen.AddElement(element);
                        }
                    }

                    screens[screen.Id] = screen;
                }
            }
            catch (Exception ex)
            {
                Debug.getInstance().log("UILayout warning: failed to parse file. " + ex.Message, Debug.DEBUG_LEVEL_WARNING);
            }

            return screens;
        }

        private static UIElement BuildElement(UIElementDefinition def)
        {
            if (def == null || string.IsNullOrWhiteSpace(def.Type))
            {
                return null;
            }

            string type = def.Type.Trim().ToLowerInvariant();

            UIElement element;
            switch (type)
            {
                case "label":
                    element = new UILabel();
                    break;
                case "panel":
                    element = new UIPanel();
                    break;
                case "button":
                    element = new UIButton();
                    break;
                case "dropdown":
                    element = new UIDropdown();
                    break;
                default:
                    Debug.getInstance().log("UILayout warning: unsupported element type " + def.Type, Debug.DEBUG_LEVEL_WARNING);
                    return null;
            }

            element.Id = string.IsNullOrWhiteSpace(def.Id) ? type : def.Id.Trim();
            element.Type = type;
            element.X = def.X;
            element.Y = def.Y;
            element.Width = def.Width;
            element.Height = def.Height;
            element.Text = def.Text ?? "";
            element.AnchorX = def.AnchorX;
            element.AnchorY = def.AnchorY;

            if (def.FontSize > 0)
            {
                element.FontSize = def.FontSize;
            }

            if (string.IsNullOrWhiteSpace(def.Action) == false)
            {
                element.ActionId = def.Action.Trim();
            }

            if (def.Visible.HasValue)
            {
                element.Visible = def.Visible.Value;
            }

            if (def.Enabled.HasValue)
            {
                element.Enabled = def.Enabled.Value;
            }

            if (element.Width <= 0)
            {
                element.Width = GetDefaultWidth(type);
            }

            if (element.Height <= 0)
            {
                element.Height = GetDefaultHeight(type, element.FontSize);
            }

            if (element is UIDropdown dropdown)
            {
                dropdown.Options.Clear();

                if (def.Options != null)
                {
                    foreach (string option in def.Options)
                    {
                        if (string.IsNullOrWhiteSpace(option) == false)
                        {
                            dropdown.Options.Add(option.Trim());
                        }
                    }
                }

                dropdown.EnsureOptions();
                dropdown.SetSelectedIndex(def.DefaultIndex);
            }

            return element;
        }

        private static int GetDefaultWidth(string type)
        {
            switch (type)
            {
                case "panel":
                    return 320;
                case "button":
                case "dropdown":
                    return 240;
                default:
                    return 0;
            }
        }

        private static int GetDefaultHeight(string type, int fontSize)
        {
            switch (type)
            {
                case "panel":
                    return 140;
                case "button":
                    return 56;
                case "dropdown":
                    return 48;
                default:
                    return Math.Max(fontSize + 8, 20);
            }
        }
    }
}
