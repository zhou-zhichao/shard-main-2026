using System.Collections.Generic;

namespace Shard
{
    class UIScreen
    {
        private string id;
        private List<UIElement> elements;

        public UIScreen(string id)
        {
            this.id = id;
            elements = new List<UIElement>();
        }

        public void AddElement(UIElement element)
        {
            if (element == null)
            {
                return;
            }

            elements.Add(element);
        }

        public string Id { get => id; set => id = value; }
        public List<UIElement> Elements { get => elements; }
    }
}
