using System;
using System.Collections.Generic;

namespace Shard
{
    class Animator
    {
        private GameObject owner;
        private Dictionary<string, AnimationClip> clips;
        private HashSet<string> warnedMissingFrames;
        private AnimationClip currentClip;
        private string currentClipId;
        private int currentFrame;
        private double frameAccumulator;
        private bool playing;
        private float speedMultiplier;

        public Animator(GameObject owner)
        {
            this.owner = owner;
            clips = new Dictionary<string, AnimationClip>();
            warnedMissingFrames = new HashSet<string>();
            currentClip = null;
            currentClipId = null;
            currentFrame = 0;
            frameAccumulator = 0;
            playing = false;
            speedMultiplier = 1.0f;
        }

        public void RegisterCodeClip(AnimationClip clip)
        {
            if (clip == null)
            {
                return;
            }

            if (validateClip(clip, "code") == false)
            {
                return;
            }

            AnimationClip resolved = clip.copy();

            AnimationClip jsonClip;
            if (AnimationCatalog.getInstance().TryGetClip(clip.Id, out jsonClip))
            {
                resolved = jsonClip.copy();
            }

            clips[resolved.Id] = resolved;
        }

        public void Play(string clipId, bool restart = false)
        {
            if (string.IsNullOrWhiteSpace(clipId))
            {
                return;
            }

            AnimationClip clip;
            if (clips.TryGetValue(clipId, out clip) == false)
            {
                Debug.getInstance().log("Animator warning: clip not found: " + clipId, Debug.DEBUG_LEVEL_WARNING);
                return;
            }

            if (restart == false && playing && currentClipId == clipId)
            {
                return;
            }

            currentClip = clip;
            currentClipId = clipId;
            currentFrame = 0;
            frameAccumulator = 0;
            playing = true;

            applyFrame();
        }

        public void SetSpeedMultiplier(float speed)
        {
            if (speed < 0)
            {
                speed = 0;
            }

            speedMultiplier = speed;
        }

        public void Update(double deltaTime)
        {
            if (playing == false || currentClip == null || deltaTime <= 0)
            {
                return;
            }

            float effectiveFps = currentClip.Fps * speedMultiplier;
            if (effectiveFps <= 0)
            {
                return;
            }

            double frameDuration = 1.0 / effectiveFps;
            frameAccumulator += deltaTime;

            while (frameAccumulator >= frameDuration && playing)
            {
                frameAccumulator -= frameDuration;
                advanceFrame();
            }
        }

        private bool validateClip(AnimationClip clip, string source)
        {
            if (string.IsNullOrWhiteSpace(clip.Id))
            {
                Debug.getInstance().log("Animator warning: " + source + " clip has empty id.", Debug.DEBUG_LEVEL_WARNING);
                return false;
            }

            if (clip.Fps <= 0)
            {
                Debug.getInstance().log("Animator warning: clip " + clip.Id + " has invalid fps.", Debug.DEBUG_LEVEL_WARNING);
                return false;
            }

            if (clip.Frames == null || clip.Frames.Count == 0)
            {
                Debug.getInstance().log("Animator warning: clip " + clip.Id + " has no frames.", Debug.DEBUG_LEVEL_WARNING);
                return false;
            }

            foreach (string frame in clip.Frames)
            {
                if (string.IsNullOrWhiteSpace(frame))
                {
                    Debug.getInstance().log("Animator warning: clip " + clip.Id + " contains blank frame names.", Debug.DEBUG_LEVEL_WARNING);
                    return false;
                }

                if (Bootstrap.getAssetManager().getAssetPath(frame) == null)
                {
                    Debug.getInstance().log("Animator warning: clip " + clip.Id + " references missing frame " + frame + ".", Debug.DEBUG_LEVEL_WARNING);
                    return false;
                }
            }

            return true;
        }

        private void advanceFrame()
        {
            if (currentClip == null)
            {
                return;
            }

            if (currentFrame < currentClip.Frames.Count - 1)
            {
                currentFrame += 1;
                applyFrame();
                return;
            }

            if (currentClip.Mode == AnimationPlayMode.Loop)
            {
                currentFrame = 0;
                applyFrame();
                return;
            }

            playing = false;
        }

        private void applyFrame()
        {
            if (currentClip == null || currentFrame < 0 || currentFrame >= currentClip.Frames.Count)
            {
                return;
            }

            string frame = currentClip.Frames[currentFrame];
            string path = Bootstrap.getAssetManager().getAssetPath(frame);

            if (path == null)
            {
                if (warnedMissingFrames.Contains(frame) == false)
                {
                    Debug.getInstance().log("Animator warning: missing frame during playback: " + frame, Debug.DEBUG_LEVEL_WARNING);
                    warnedMissingFrames.Add(frame);
                }

                return;
            }

            owner.Transform.SpritePath = path;
        }
    }
}
