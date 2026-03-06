using System;
using System.Collections.Generic;

namespace Shard
{
    class UISystem
    {
        private Dictionary<string, UIScreen> screens;
        private Dictionary<string, Action> buttonActions;
        private Dictionary<string, Action<string>> dropdownActions;
        private Dictionary<string, UIElement> activeElementsById;
        private UIScreen activeScreen;
        private UIButton capturedButton;
        private int mouseX;
        private int mouseY;

        public UISystem()
        {
            screens = new Dictionary<string, UIScreen>();
            buttonActions = new Dictionary<string, Action>();
            dropdownActions = new Dictionary<string, Action<string>>();
            activeElementsById = new Dictionary<string, UIElement>();
            activeScreen = null;
            capturedButton = null;
            mouseX = 0;
            mouseY = 0;
        }

        public void LoadFromAsset(string assetFileName)
        {
            if (Bootstrap.getAssetManager() == null)
            {
                Debug.getInstance().log("UISystem warning: AssetManager unavailable.", Debug.DEBUG_LEVEL_WARNING);
                return;
            }

            string path = Bootstrap.getAssetManager().getAssetPath(assetFileName);

            if (path == null)
            {
                Debug.getInstance().log("UISystem warning: layout asset not found: " + assetFileName, Debug.DEBUG_LEVEL_WARNING);
                return;
            }

            screens = UILayoutCatalog.Load(path);

            if (screens.Count == 0)
            {
                Debug.getInstance().log("UISystem warning: no screens loaded from " + assetFileName, Debug.DEBUG_LEVEL_WARNING);
                return;
            }

            foreach (KeyValuePair<string, UIScreen> pair in screens)
            {
                activeScreen = pair.Value;
                break;
            }

            cacheActiveElements();
            UpdateHoverState();
        }

        public void SetScreen(string screenId)
        {
            if (string.IsNullOrWhiteSpace(screenId))
            {
                return;
            }

            if (screens.TryGetValue(screenId, out UIScreen screen) == false)
            {
                Debug.getInstance().log("UISystem warning: missing screen " + screenId, Debug.DEBUG_LEVEL_WARNING);
                return;
            }

            activeScreen = screen;
            capturedButton = null;
            CloseAllDropdownsExcept(null);
            cacheActiveElements();
            UpdateHoverState();
        }

        public void Render()
        {
            if (activeScreen == null)
            {
                return;
            }

            UIDropdown openDropdown = GetOpenDropdown();

            foreach (UIElement element in activeScreen.Elements)
            {
                if (element == null || element.Visible == false)
                {
                    continue;
                }

                if (openDropdown != null)
                {
                    if (ReferenceEquals(element, openDropdown))
                    {
                        continue;
                    }

                    if (ShouldSkipBehindDropdownOverlay(element, openDropdown))
                    {
                        continue;
                    }
                }

                element.Render();
            }

            if (openDropdown != null)
            {
                openDropdown.Render();
            }
        }

        public void HandleInput(InputEvent inp, string eventType)
        {
            if (inp == null || activeScreen == null)
            {
                return;
            }

            if (eventType == "MouseMotion")
            {
                mouseX = inp.X;
                mouseY = inp.Y;
                UpdateHoverState();
                return;
            }

            if (eventType == "MouseDown" && inp.Button == 1)
            {
                mouseX = inp.X;
                mouseY = inp.Y;
                HandleMouseDown();
                return;
            }

            if (eventType == "MouseUp" && inp.Button == 1)
            {
                mouseX = inp.X;
                mouseY = inp.Y;
                HandleMouseUp();
            }
        }

        public void BindButtonAction(string actionId, Action callback)
        {
            if (string.IsNullOrWhiteSpace(actionId) || callback == null)
            {
                return;
            }

            buttonActions[actionId] = callback;
        }

        public void BindDropdownAction(string actionId, Action<string> callback)
        {
            if (string.IsNullOrWhiteSpace(actionId) || callback == null)
            {
                return;
            }

            dropdownActions[actionId] = callback;
        }

        public UIElement FindElement(string elementId)
        {
            if (string.IsNullOrWhiteSpace(elementId))
            {
                return null;
            }

            if (activeElementsById.TryGetValue(elementId, out UIElement element))
            {
                return element;
            }

            return null;
        }

        public bool SetDropdownSelectedOption(string elementId, string option)
        {
            if (string.IsNullOrWhiteSpace(option))
            {
                return false;
            }

            if (FindElement(elementId) is not UIDropdown dropdown)
            {
                return false;
            }

            for (int i = 0; i < dropdown.Options.Count; i++)
            {
                if (string.Equals(dropdown.Options[i], option, StringComparison.OrdinalIgnoreCase))
                {
                    dropdown.SetSelectedIndex(i);
                    return true;
                }
            }

            return false;
        }

        private void HandleMouseDown()
        {
            UIDropdown openDropdown = GetOpenDropdown();
            if (openDropdown != null)
            {
                if (openDropdown.HitTestOption(mouseX, mouseY, out int optionIndex))
                {
                    openDropdown.SetSelectedIndex(optionIndex);
                    openDropdown.Expanded = false;
                    InvokeDropdownAction(openDropdown);
                    UpdateHoverState();
                    return;
                }

                if (openDropdown.HitTest(mouseX, mouseY))
                {
                    openDropdown.Expanded = false;
                    UpdateHoverState();
                    return;
                }

                openDropdown.Expanded = false;
                UpdateHoverState();
                return;
            }

            UIElement target = FindTopElementAt(mouseX, mouseY);
            if (target == null)
            {
                capturedButton = null;
                UpdateHoverState();
                return;
            }

            if (target is UIDropdown dropdown)
            {
                CloseAllDropdownsExcept(dropdown);
                dropdown.Expanded = !dropdown.Expanded;
                UpdateHoverState();
                return;
            }

            if (target is UIButton button)
            {
                button.IsPressed = true;
                capturedButton = button;
                UpdateHoverState();
            }
        }

        private void HandleMouseUp()
        {
            if (capturedButton == null)
            {
                UpdateHoverState();
                return;
            }

            bool clickSucceeded = capturedButton.HitTest(mouseX, mouseY);
            capturedButton.IsPressed = false;

            if (clickSucceeded)
            {
                InvokeButtonAction(capturedButton);
            }

            capturedButton = null;
            UpdateHoverState();
        }

        private void InvokeButtonAction(UIButton button)
        {
            if (button == null || string.IsNullOrWhiteSpace(button.ActionId))
            {
                return;
            }

            if (buttonActions.TryGetValue(button.ActionId, out Action callback))
            {
                callback();
            }
        }

        private void InvokeDropdownAction(UIDropdown dropdown)
        {
            if (dropdown == null || string.IsNullOrWhiteSpace(dropdown.ActionId))
            {
                return;
            }

            if (dropdownActions.TryGetValue(dropdown.ActionId, out Action<string> callback))
            {
                callback(dropdown.SelectedOption);
            }
        }

        private UIElement FindTopElementAt(int x, int y)
        {
            if (activeScreen == null)
            {
                return null;
            }

            for (int i = activeScreen.Elements.Count - 1; i >= 0; i--)
            {
                UIElement element = activeScreen.Elements[i];

                if (element == null || element.Visible == false || element.Enabled == false)
                {
                    continue;
                }

                if (element is UIDropdown dropdown)
                {
                    if (dropdown.HitTest(x, y))
                    {
                        return dropdown;
                    }

                    if (dropdown.Expanded && dropdown.HitTestOption(x, y, out int _))
                    {
                        return dropdown;
                    }

                    continue;
                }

                if (element is UIButton button && button.HitTest(x, y))
                {
                    return button;
                }
            }

            return null;
        }

        private UIDropdown GetOpenDropdown()
        {
            if (activeScreen == null)
            {
                return null;
            }

            for (int i = activeScreen.Elements.Count - 1; i >= 0; i--)
            {
                if (activeScreen.Elements[i] is UIDropdown dropdown && dropdown.Expanded)
                {
                    return dropdown;
                }
            }

            return null;
        }

        private void CloseAllDropdownsExcept(UIDropdown keep)
        {
            if (activeScreen == null)
            {
                return;
            }

            foreach (UIElement element in activeScreen.Elements)
            {
                if (element is UIDropdown dropdown && dropdown != keep)
                {
                    dropdown.Expanded = false;
                    dropdown.HoveredOptionIndex = -1;
                }
            }
        }

        private void UpdateHoverState()
        {
            if (activeScreen == null)
            {
                return;
            }

            foreach (UIElement element in activeScreen.Elements)
            {
                if (element is UIButton button)
                {
                    button.IsHovered = button.HitTest(mouseX, mouseY);
                }
                else if (element is UIDropdown dropdown)
                {
                    dropdown.IsHovered = dropdown.HitTest(mouseX, mouseY);
                    dropdown.HoveredOptionIndex = -1;

                    if (dropdown.Expanded && dropdown.HitTestOption(mouseX, mouseY, out int optionIndex))
                    {
                        dropdown.HoveredOptionIndex = optionIndex;
                    }
                }
            }
        }

        private bool ShouldSkipBehindDropdownOverlay(UIElement element, UIDropdown overlayDropdown)
        {
            if (overlayDropdown == null || element == null)
            {
                return false;
            }

            if (element is UIPanel)
            {
                return false;
            }

            int elementWidth = element.Width;
            int elementHeight = element.Height;

            if (elementWidth <= 0 || elementHeight <= 0)
            {
                return false;
            }

            int overlayX = overlayDropdown.ResolvedX;
            int overlayY = overlayDropdown.ResolvedY;
            int overlayWidth = overlayDropdown.Width;
            int overlayHeight = overlayDropdown.GetExpandedRenderHeight();

            return RectanglesIntersect(
                element.ResolvedX, element.ResolvedY, elementWidth, elementHeight,
                overlayX, overlayY, overlayWidth, overlayHeight
            );
        }

        private bool RectanglesIntersect(int ax, int ay, int aw, int ah, int bx, int by, int bw, int bh)
        {
            return ax < bx + bw && ax + aw > bx && ay < by + bh && ay + ah > by;
        }

        private void cacheActiveElements()
        {
            activeElementsById.Clear();

            if (activeScreen == null)
            {
                return;
            }

            foreach (UIElement element in activeScreen.Elements)
            {
                if (element == null || string.IsNullOrWhiteSpace(element.Id))
                {
                    continue;
                }

                activeElementsById[element.Id] = element;
            }
        }
    }
}
