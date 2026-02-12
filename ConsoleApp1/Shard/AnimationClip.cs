using System.Collections.Generic;

namespace Shard
{
    class AnimationClip
    {
        public string Id { get; set; }
        public List<string> Frames { get; set; }
        public float Fps { get; set; }
        public AnimationPlayMode Mode { get; set; }

        public AnimationClip()
        {
            Id = "";
            Frames = new List<string>();
            Fps = 1.0f;
            Mode = AnimationPlayMode.Loop;
        }

        public AnimationClip(string id, List<string> frames, float fps, AnimationPlayMode mode)
        {
            Id = id;
            Frames = frames;
            Fps = fps;
            Mode = mode;
        }

        public AnimationClip copy()
        {
            return new AnimationClip(Id, new List<string>(Frames), Fps, Mode);
        }
    }
}
