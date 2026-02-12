using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shard
{
    class AnimationCatalogFile
    {
        [JsonPropertyName("clips")]
        public List<AnimationCatalogClip> Clips { get; set; }
    }

    class AnimationCatalogClip
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("fps")]
        public float Fps { get; set; }

        [JsonPropertyName("mode")]
        public string Mode { get; set; }

        [JsonPropertyName("frames")]
        public List<string> Frames { get; set; }
    }

    class AnimationCatalog
    {
        private static AnimationCatalog me;
        private Dictionary<string, AnimationClip> clips;
        private bool initialized;

        private AnimationCatalog()
        {
            clips = new Dictionary<string, AnimationClip>();
            initialized = false;
        }

        public static AnimationCatalog getInstance()
        {
            if (me == null)
            {
                me = new AnimationCatalog();
            }

            return me;
        }

        public void initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            clips.Clear();

            if (Bootstrap.getAssetManager() == null)
            {
                Debug.getInstance().log("AnimationCatalog warning: AssetManager unavailable.", Debug.DEBUG_LEVEL_WARNING);
                return;
            }

            string path = Bootstrap.getAssetManager().getAssetPath("animations.json");

            if (path == null || File.Exists(path) == false)
            {
                Debug.getInstance().log("AnimationCatalog warning: Assets/animations.json not found. Falling back to code clips.", Debug.DEBUG_LEVEL_WARNING);
                return;
            }

            try
            {
                string raw = File.ReadAllText(path);

                AnimationCatalogFile data = JsonSerializer.Deserialize<AnimationCatalogFile>(raw, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (data == null || data.Clips == null)
                {
                    Debug.getInstance().log("AnimationCatalog warning: animations.json has no clips section. Falling back to code clips.", Debug.DEBUG_LEVEL_WARNING);
                    return;
                }

                foreach (AnimationCatalogClip clipCfg in data.Clips)
                {
                    AnimationClip clip;
                    if (tryBuildClip(clipCfg, out clip) == false)
                    {
                        continue;
                    }

                    clips[clip.Id] = clip;
                }
            }
            catch (Exception ex)
            {
                Debug.getInstance().log("AnimationCatalog warning: Failed to parse animations.json. " + ex.Message, Debug.DEBUG_LEVEL_WARNING);
            }
        }

        public bool TryGetClip(string id, out AnimationClip clip)
        {
            return clips.TryGetValue(id, out clip);
        }

        private bool tryBuildClip(AnimationCatalogClip cfg, out AnimationClip clip)
        {
            clip = null;

            if (cfg == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(cfg.Id))
            {
                Debug.getInstance().log("AnimationCatalog warning: clip id missing, clip skipped.", Debug.DEBUG_LEVEL_WARNING);
                return false;
            }

            if (cfg.Fps <= 0)
            {
                Debug.getInstance().log("AnimationCatalog warning: clip " + cfg.Id + " has invalid fps, clip skipped.", Debug.DEBUG_LEVEL_WARNING);
                return false;
            }

            if (cfg.Frames == null || cfg.Frames.Count == 0)
            {
                Debug.getInstance().log("AnimationCatalog warning: clip " + cfg.Id + " has no frames, clip skipped.", Debug.DEBUG_LEVEL_WARNING);
                return false;
            }

            AnimationPlayMode mode;
            if (Enum.TryParse(cfg.Mode, true, out mode) == false)
            {
                Debug.getInstance().log("AnimationCatalog warning: clip " + cfg.Id + " has invalid mode, clip skipped.", Debug.DEBUG_LEVEL_WARNING);
                return false;
            }

            foreach (string frame in cfg.Frames)
            {
                if (string.IsNullOrWhiteSpace(frame))
                {
                    Debug.getInstance().log("AnimationCatalog warning: clip " + cfg.Id + " has blank frame entry, clip skipped.", Debug.DEBUG_LEVEL_WARNING);
                    return false;
                }

                if (Bootstrap.getAssetManager().getAssetPath(frame) == null)
                {
                    Debug.getInstance().log("AnimationCatalog warning: clip " + cfg.Id + " references missing frame " + frame + ", clip skipped.", Debug.DEBUG_LEVEL_WARNING);
                    return false;
                }
            }

            clip = new AnimationClip(cfg.Id, new List<string>(cfg.Frames), cfg.Fps, mode);
            return true;
        }
    }
}
