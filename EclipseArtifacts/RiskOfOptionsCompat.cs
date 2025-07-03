using RiskOfOptions;
using RiskOfOptions.Options;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EclipseArtifacts
{
    internal class RiskOfOptionsCompat
    {
        private static bool? _enabled;

        public static bool Enabled
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.rune580.riskofoptions");
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void AddConfig()
        {
            ModSettingsManager.SetModIcon(LoadIcon());

            ModSettingsManager.AddOption(new ChoiceOption(EclipseArtifactsConfig.frailtyCompatMode));
        }

        private static Sprite LoadIcon()
        {
            var iconFilePath = FindIconFilePath();
            if (iconFilePath == null)
                return null;

            var texture = new Texture2D(256, 256);
            texture.LoadImage(File.ReadAllBytes(iconFilePath));
            return Sprite.Create(texture, new Rect(0, 0, 256, 256), new Vector2(0, 0));
        }

        private static string FindIconFilePath()
        {
            var dir = new DirectoryInfo(EclipseArtifactsPlugin.PluginDirectory);
            do
            {
                var files = dir.GetFiles("icon.png", SearchOption.TopDirectoryOnly);
                if (files != null && files.Length > 0)
                    return files[0].FullName;

                dir = dir.Parent;
            } while (dir != null && !string.Equals(dir.Name, "plugins", StringComparison.OrdinalIgnoreCase));

            return null;
        }
    }
}
