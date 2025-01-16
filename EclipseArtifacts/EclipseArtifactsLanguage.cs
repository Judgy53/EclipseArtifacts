using RoR2;
using System.Linq;

namespace EclipseArtifacts
{
    public static class EclipseArtifactsLanguage
    {
        public static bool TokensRegistered { get; private set; } = false;

        public static void TryRegisterTokens()
        {
            if (TokensRegistered) return;

            for (int i = 1; i <= 8; i++)
            {
                if (Language.IsTokenInvalid($"ECLIPSE_{i}_DESCRIPTION")) return;
            }

            for (int i = 1; i <= 8; i++)
            {
                RegisterArtifactLanguageData(i);
            }
            
            TokensRegistered = true;
        }

        private static void RegisterArtifactLanguageData(int eclipseLevel)
        {
            R2API.LanguageAPI.Add(EclipseArtifactsContent.Artifacts.GetArtifactNameTokenFromEclipseLevel(eclipseLevel), GetArtifactName(eclipseLevel));
            R2API.LanguageAPI.Add(EclipseArtifactsContent.Artifacts.GetArtifactDescTokenFromEclipseLevel(eclipseLevel), GetArtifactDescription(eclipseLevel));
        }

        private static string GetArtifactName(int eclipseLevel)
        {
            return $"Artifact of Eclipse {eclipseLevel}";
        }

        private static string GetArtifactDescription(int eclipseLevel)
        {
            var token = $"ECLIPSE_{eclipseLevel}_DESCRIPTION";
            var prefix = $"<mspace=0.5em>({eclipseLevel})</mspace> ";

            var languageString = Language.GetString(token);

            var modifierFull = languageString.Split('\n').FirstOrDefault(s => s.StartsWith(prefix));
            if (modifierFull == null)
            {
                Log.LogError($"Error while parsing \"{token}\":\n\n{languageString}");
                return token;
            }

            var cleanedModifier = modifierFull[prefix.Length..].TrimEnd('\n');
            //var description = $"Enable Eclipse {eclipseLevel} modifier.\n<style=cStack>>{cleanedModifier}";
            var description = $"<style=cStack>>{cleanedModifier}";


            if (EclipseRefurbishedCompat.Enabled)
                description = EclipseRefurbishedCompat.AddDirectorCreditsToDescription(description);

            return description;
        }
    }
}
