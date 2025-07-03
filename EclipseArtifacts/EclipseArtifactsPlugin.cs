using BepInEx;
using RoR2.ContentManagement;
using System;
using UnityEngine;

namespace EclipseArtifacts
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    [BepInDependency("com.Puporongod.EclipseRefurbished", BepInDependency.DependencyFlags.SoftDependency)]
    public class EclipseArtifactsPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Judgy";
        public const string PluginName = "EclipseArtifacts";
        public const string PluginVersion = "1.2.1";

        public static string PluginDirectory;

        public void Awake()
        {
            PluginDirectory = System.IO.Path.GetDirectoryName(Info.Location);

            Log.Init(Logger);
            EclipseArtifactsConfig.Init(Config);

            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;

            EclipseArtifactsBehavior.EnableHooks();

            Log.LogInfo(nameof(Awake) + " done.");
        }

        public void OnDestroy()
        {
            EclipseArtifactsBehavior.DisableHooks();
        }

        public void Update()
        {
            if (!EclipseArtifactsLanguage.TokensRegistered)
                EclipseArtifactsLanguage.TryRegisterTokens();
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new EclipseArtifactsContent());
        }

        public static Sprite LoadResourceSprite(string resName)
        {
            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Sprite sprite = null;

            try
            {
                byte[] resBytes = (byte[])Properties.Resources.ResourceManager.GetObject(resName);
                tex.LoadImage(resBytes, false);
                tex.Apply();

                sprite = Sprite.Create(tex, new Rect(0, 0, 128, 128), new Vector2(64, 64));
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

            return sprite;
        }
    }
}
