using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Shard
{
    class AnimationCatalogFile
    {
        public List<AnimationCatalogClip> Clips { get; set; }
    }

    class AnimationCatalogClip
    {
        public string Id { get; set; }
        public float Fps { get; set; }
        public string Mode { get; set; }
        public string Texture { get; set; }
        public int FrameWidth { get; set; }
        public int FrameHeight { get; set; }
        public JsonElement Frames { get; set; }
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

            AnimationPlayMode mode;
            if (Enum.TryParse(cfg.Mode, true, out mode) == false)
            {
                Debug.getInstance().log("AnimationCatalog warning: clip " + cfg.Id + " has invalid mode, clip skipped.", Debug.DEBUG_LEVEL_WARNING);
                return false;
            }

            if (string.IsNullOrWhiteSpace(cfg.Texture))
            {
                if (cfg.Frames.ValueKind != JsonValueKind.Array || cfg.Frames.GetArrayLength() == 0)
                {
                    Debug.getInstance().log("AnimationCatalog warning: clip " + cfg.Id + " has no frames, clip skipped.", Debug.DEBUG_LEVEL_WARNING);
                    return false;
                }

                List<string> frames = new List<string>();
                foreach (JsonElement element in cfg.Frames.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.String)
                    {
                        Debug.getInstance().log("AnimationCatalog warning: clip " + cfg.Id + " mixes legacy and atlas frame formats, clip skipped.", Debug.DEBUG_LEVEL_WARNING);
                        return false;
                    }

                    string frame = element.GetString() ?? "";
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

                    frames.Add(frame);
                }

                clip = new AnimationClip(cfg.Id, frames, cfg.Fps, mode);
                return true;
            }

            if (Bootstrap.getAssetManager().getAssetPath(cfg.Texture) == null)
            {
                Debug.getInstance().log("AnimationCatalog warning: clip " + cfg.Id + " references missing texture " + cfg.Texture + ", clip skipped.", Debug.DEBUG_LEVEL_WARNING);
                return false;
            }

            if (cfg.FrameWidth <= 0 || cfg.FrameHeight <= 0)
            {
                Debug.getInstance().log("AnimationCatalog warning: clip " + cfg.Id + " has invalid atlas frame size, clip skipped.", Debug.DEBUG_LEVEL_WARNING);
                return false;
            }

            if (cfg.Frames.ValueKind != JsonValueKind.Array || cfg.Frames.GetArrayLength() == 0)
            {
                Debug.getInstance().log("AnimationCatalog warning: clip " + cfg.Id + " has no atlas frames, clip skipped.", Debug.DEBUG_LEVEL_WARNING);
                return false;
            }

            List<AnimationFrame> atlasFrames = new List<AnimationFrame>();
            foreach (JsonElement element in cfg.Frames.EnumerateArray())
            {
                if (element.ValueKind != JsonValueKind.Object)
                {
                    Debug.getInstance().log("AnimationCatalog warning: clip " + cfg.Id + " atlas frames must be objects, clip skipped.", Debug.DEBUG_LEVEL_WARNING);
                    return false;
                }

                if (element.TryGetProperty("col", out JsonElement colElement) == false ||
                    element.TryGetProperty("row", out JsonElement rowElement) == false ||
                    colElement.TryGetInt32(out int col) == false ||
                    rowElement.TryGetInt32(out int row) == false)
                {
                    Debug.getInstance().log("AnimationCatalog warning: clip " + cfg.Id + " atlas frame is missing col/row, clip skipped.", Debug.DEBUG_LEVEL_WARNING);
                    return false;
                }

                atlasFrames.Add(new AnimationFrame(col, row));
            }

            clip = new AnimationClip(cfg.Id, cfg.Texture, cfg.FrameWidth, cfg.FrameHeight, atlasFrames, cfg.Fps, mode);
            return true;
        }
    }
}
