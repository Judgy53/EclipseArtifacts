using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EclipseArtifacts
{
    public class EclipseArtifactsContent : IContentPackProvider
    {
        public ContentPack pack = new ContentPack();

        public string identifier
        {
            get { return "EclipseArtifactContent"; }
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            Artifacts.Create();

            pack.artifactDefs.Add(new List<ArtifactDef>(Artifacts.artifactDefs.Values).ToArray());

            args.ReportProgress(1.0f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(pack, args.output);
            args.ReportProgress(1.0f);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public static class Artifacts
        {
            public static Dictionary<int, ArtifactDef> artifactDefs = new Dictionary<int, ArtifactDef>();

            public static ArtifactDef GetArtifactDefFromEclipseLevel(int eclipseLevel)
            {
                if (artifactDefs.TryGetValue(eclipseLevel, out ArtifactDef def))
                    return def;

                return null;
            }

            public static void Create()
            {
                for(int i = 1; i <= 8; i++)
                {
                    ArtifactDef artifact = ScriptableObject.CreateInstance<ArtifactDef>();
                    artifact.cachedName = GetArtifactCachedNameFromEclipseLevel(i);
                    artifact.nameToken = GetArtifactNameTokenFromEclipseLevel(i);
                    artifact.descriptionToken = GetArtifactDescTokenFromEclipseLevel(i);
                    artifact.smallIconSelectedSprite = GetSelectedSpriteFromEclipseLevel(i);
                    artifact.smallIconDeselectedSprite = GetDeselectedSpriteFromEclipseLevel(i);

                    artifactDefs.Add(i, artifact);
                }
            }

            public static string GetArtifactNameTokenFromEclipseLevel(int eclipseLevel)
            {
                return $"ARTIFACT_INDIVIDUALECLIPSE{eclipseLevel}_NAME";
            }

            public static string GetArtifactDescTokenFromEclipseLevel(int eclipseLevel)
            {
                return $"ARTIFACT_INDIVIDUALECLIPSE{eclipseLevel}_DESC";
            }

            private static string GetArtifactCachedNameFromEclipseLevel(int eclipseLevel)
            {
                return $"ARTIFACT_INDIVIDUALECLIPSE{eclipseLevel}";
            }

            private static Sprite GetSelectedSpriteFromEclipseLevel(int eclipseLevel)
            {
                Sprite sprite = EclipseArtifactsPlugin.LoadResourceSprite($"E{eclipseLevel}_selected");
                if (sprite != null)
                    return sprite;
                
                return Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(3, 3));
            }

            private static Sprite GetDeselectedSpriteFromEclipseLevel(int eclipseLevel)
            {
                Sprite sprite = EclipseArtifactsPlugin.LoadResourceSprite($"E{eclipseLevel}_deselected");
                if (sprite != null)
                    return sprite;

                return Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 4, 4), new Vector2(3, 3));
            }
        }
    }
}
