using System;

namespace GameTest
{
    static class DemoSettings
    {
        public const string FrameLimitAction = "set_frame_limit";
        public const string WindowSizeAction = "set_window_size";
        public const string MusicVolumeAction = "set_music_volume";
        public const string SfxVolumeAction = "set_sfx_volume";

        private static string currentFrameLimit = "60";
        private static string currentWindowSize = "1280x864";
        private static string currentMusicVolume = "80%";
        private static string currentSfxVolume = "100%";

        public static void Bind(Shard.UISystem uiSystem)
        {
            if (uiSystem == null)
            {
                return;
            }

            uiSystem.BindDropdownAction(FrameLimitAction, ApplyFrameLimit);
            uiSystem.BindDropdownAction(WindowSizeAction, ApplyWindowSize);
            uiSystem.BindDropdownAction(MusicVolumeAction, ApplyMusicVolume);
            uiSystem.BindDropdownAction(SfxVolumeAction, ApplySfxVolume);
        }

        public static void SyncCurrentScreen(Shard.UISystem uiSystem)
        {
            if (uiSystem == null)
            {
                return;
            }

            uiSystem.SetDropdownSelectedOption("frame_limit_dropdown", currentFrameLimit);
            uiSystem.SetDropdownSelectedOption("window_size_dropdown", currentWindowSize);
            uiSystem.SetDropdownSelectedOption("music_volume_dropdown", currentMusicVolume);
            uiSystem.SetDropdownSelectedOption("sfx_volume_dropdown", currentSfxVolume);
        }

        public static void ApplyCurrentRuntimeValues()
        {
            Shard.Bootstrap.setTargetFrameRate(GetTargetFrameRate());
            Shard.Bootstrap.getSound().MusicVolume = ParsePercentage(currentMusicVolume, 0.8f);
            Shard.Bootstrap.getSound().EffectsVolume = ParsePercentage(currentSfxVolume, 1.0f);
        }

        public static int GetTargetFrameRate()
        {
            if (string.Equals(currentFrameLimit, "Unlimited", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (int.TryParse(currentFrameLimit, out int fps))
            {
                return fps;
            }

            return 60;
        }

        private static void ApplyFrameLimit(string option)
        {
            if (string.IsNullOrWhiteSpace(option))
            {
                return;
            }

            currentFrameLimit = option;
            Shard.Bootstrap.setTargetFrameRate(GetTargetFrameRate());
        }

        private static void ApplyWindowSize(string option)
        {
            if (string.IsNullOrWhiteSpace(option))
            {
                return;
            }

            currentWindowSize = option;

            if (string.Equals(option, "Fullscreen", StringComparison.OrdinalIgnoreCase))
            {
                Shard.Bootstrap.getDisplay().setFullscreen(true);
                return;
            }

            string[] parts = option.Split('x', 'X');
            if (parts.Length != 2)
            {
                return;
            }

            if (int.TryParse(parts[0], out int width) == false ||
                int.TryParse(parts[1], out int height) == false)
            {
                return;
            }

            Shard.Bootstrap.getDisplay().setFullscreen(false);
            Shard.Bootstrap.getDisplay().setWindowSize(width, height);
        }

        private static void ApplyMusicVolume(string option)
        {
            currentMusicVolume = option;
            Shard.Bootstrap.getSound().MusicVolume = ParsePercentage(option, 0.8f);
        }

        private static void ApplySfxVolume(string option)
        {
            currentSfxVolume = option;
            Shard.Bootstrap.getSound().EffectsVolume = ParsePercentage(option, 1.0f);
        }

        private static float ParsePercentage(string option, float fallback)
        {
            if (string.IsNullOrWhiteSpace(option))
            {
                return fallback;
            }

            string cleaned = option.Replace("%", "").Trim();
            if (int.TryParse(cleaned, out int percentage))
            {
                return Math.Clamp(percentage / 100.0f, 0.0f, 1.0f);
            }

            return fallback;
        }
    }
}
