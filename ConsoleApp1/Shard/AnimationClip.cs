using System.Collections.Generic;

namespace Shard
{
    class AnimationClip
    {
        public string Id { get; set; }
        public List<string> Frames { get; set; }
        public string Texture { get; set; }
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public List<AnimationFrame> AtlasFrames { get; set; }
        public float Fps { get; set; }
        public AnimationPlayMode Mode { get; set; }

        public bool UsesAtlas
        {
            get
            {
                return string.IsNullOrWhiteSpace(Texture) == false &&
                    FrameWidth > 0 &&
                    FrameHeight > 0 &&
                    AtlasFrames != null &&
                    AtlasFrames.Count > 0;
            }
        }

        public AnimationClip()
        {
            Id = "";
            Frames = new List<string>();
            Texture = "";
            FrameWidth = 0;
            FrameHeight = 0;
            AtlasFrames = new List<AnimationFrame>();
            Fps = 1.0f;
            Mode = AnimationPlayMode.Loop;
        }

        public AnimationClip(string id, List<string> frames, float fps, AnimationPlayMode mode)
        {
            Id = id;
            Frames = frames;
            Texture = "";
            FrameWidth = 0;
            FrameHeight = 0;
            AtlasFrames = new List<AnimationFrame>();
            Fps = fps;
            Mode = mode;
        }

        public AnimationClip(string id, string texture, int frameWidth, int frameHeight, List<AnimationFrame> atlasFrames, float fps, AnimationPlayMode mode)
        {
            Id = id;
            Frames = new List<string>();
            Texture = texture ?? "";
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            AtlasFrames = atlasFrames ?? new List<AnimationFrame>();
            Fps = fps;
            Mode = mode;
        }

        public AnimationClip copy()
        {
            AnimationClip copy = new AnimationClip
            {
                Id = Id,
                Frames = new List<string>(Frames),
                Texture = Texture,
                FrameWidth = FrameWidth,
                FrameHeight = FrameHeight,
                AtlasFrames = new List<AnimationFrame>(),
                Fps = Fps,
                Mode = Mode
            };

            foreach (AnimationFrame frame in AtlasFrames)
            {
                copy.AtlasFrames.Add(frame.copy());
            }

            return copy;
        }
    }
}
