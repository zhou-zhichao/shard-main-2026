namespace Shard
{
    class AnimationFrame
    {
        public string AssetName { get; set; }
        public int Column { get; set; }
        public int Row { get; set; }

        public AnimationFrame()
        {
            AssetName = "";
            Column = 0;
            Row = 0;
        }

        public AnimationFrame(string assetName)
        {
            AssetName = assetName ?? "";
            Column = 0;
            Row = 0;
        }

        public AnimationFrame(int column, int row)
        {
            AssetName = "";
            Column = column;
            Row = row;
        }

        public AnimationFrame copy()
        {
            return new AnimationFrame
            {
                AssetName = AssetName,
                Column = Column,
                Row = Row
            };
        }
    }
}
