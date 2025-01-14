using BepInEx;
using RoR2.ContentManagement;
using System;
using UnityEngine;

namespace EclipseArtifacts
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    public class EclipseArtifactsPlugin : BaseUnityPlugin
    {
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "Judgy";
        public const string PluginName = "EclipseArtifacts";
        public const string PluginVersion = "1.1.0";

        public void Awake()
        {
            Log.Init(Logger);

            ContentManager.collectContentPackProviders += ContentManager_collectContentPackProviders;

            EclipseArtifactsBehavior.Init();

            Log.LogInfo(nameof(Awake) + " done.");
        }

        private void ContentManager_collectContentPackProviders(ContentManager.AddContentPackProviderDelegate addContentPackProvider)
        {
            addContentPackProvider(new EclipseArtifactsContent());
        }

        public static void RegisterLanguageToken(string token, string text)
        {
            R2API.LanguageAPI.Add(token, text);
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
