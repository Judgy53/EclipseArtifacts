using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace EclipseArtifacts
{
    public static class EclipseArtifactsBehavior
    {
        public static bool IsArtifactEnabled(int eclipseLevel)
        {
            if (RunArtifactManager.instance == null)
                return false;

            ArtifactDef artifact = EclipseArtifactsContent.Artifacts.GetArtifactDefFromEclipseLevel(eclipseLevel);
            if (artifact == null)
                return false;

            return RunArtifactManager.instance.IsArtifactEnabled(artifact);
        }

        public static void Init()
        {
            RegisterArtifactLanguageData(1, "Artifact of Eclipse 1", "Enable Eclipse 1 modifier.\n<style=cStack>>Ally Starting Health: <style=cDeath>-50%</style></style>");
            Eclipse1Hook();

            RegisterArtifactLanguageData(2, "Artifact of Eclipse 2", "Enable Eclipse 2 modifier.\n<style=cStack>>Teleporter Radius: <style=cDeath>-50%</style></style>");
            Eclipse2Hook();

            RegisterArtifactLanguageData(3, "Artifact of Eclipse 3", "Enable Eclipse 3 modifier.\n<style=cStack>>Ally Fall Damage: <style=cDeath>+100% and lethal</style></style>");
            Eclipse3Hook();

            RegisterArtifactLanguageData(4, "Artifact of Eclipse 4", "Enable Eclipse 4 modifier.\n<style=cStack>>Enemy Speed: <style=cDeath>+40%</style></style>");
            Eclipse4Hook();

            RegisterArtifactLanguageData(5, "Artifact of Eclipse 5", "Enable Eclipse 5 modifier.\n<style=cStack>>Ally Healing: <style=cDeath>-50%</style></style>");
            Eclipse5Hook();

            RegisterArtifactLanguageData(6, "Artifact of Eclipse 6", "Enable Eclipse 6 modifier.\n<style=cStack>>Enemy Gold Drops: <style=cDeath>-20%</style></style>");
            Eclipse6Hook();

            RegisterArtifactLanguageData(7, "Artifact of Eclipse 7", "Enable Eclipse 7 modifier.\n<style=cStack>>Enemy Cooldowns: <style=cDeath>-50%</style></style>");
            Eclipse7Hook();

            RegisterArtifactLanguageData(8, "Artifact of Eclipse 8", "Enable Eclipse 8 modifier.\n<style=cStack>>Allies receive <style=cDeath>permanent damage</style>.</style>");
            Eclipse8Hook();
        }

        private static void RegisterArtifactLanguageData(int eclipseLevel, string name, string desc)
        {
            EclipseArtifactsPlugin.RegisterLanguageToken(EclipseArtifactsContent.Artifacts.GetArtifactNameTokenFromEclipseLevel(eclipseLevel), name);
            EclipseArtifactsPlugin.RegisterLanguageToken(EclipseArtifactsContent.Artifacts.GetArtifactDescTokenFromEclipseLevel(eclipseLevel), desc);
        }

        private static void Eclipse1Hook()
        {
            IL.RoR2.CharacterMaster.OnBodyStart += (il) => GetSelectedEclipseHook(il, 1, DifficultyIndex.Eclipse1);
        }

        private static void Eclipse2Hook()
        {
            IL.RoR2.HoldoutZoneController.DoUpdate += (il) => GetSelectedEclipseHook(il, 2, DifficultyIndex.Eclipse2);
        }

        private static void Eclipse3Hook()
        {
            IL.RoR2.GlobalEventManager.OnCharacterHitGroundServer += (il) => GetSelectedEclipseHook(il, 3, DifficultyIndex.Eclipse3);
        }

        private static void Eclipse4Hook()
        {
            IL.RoR2.CharacterBody.RecalculateStats += (il) => GetSelectedEclipseHook(il, 4, DifficultyIndex.Eclipse4);
        }

        private static void Eclipse5Hook()
        {
            IL.RoR2.HealthComponent.Heal += (il) => GetSelectedEclipseHook(il, 5, DifficultyIndex.Eclipse5);
        }

        private static void Eclipse6Hook()
        {
            IL.RoR2.DeathRewards.OnKilledServer += (il) => GetSelectedEclipseHook(il, 6, DifficultyIndex.Eclipse6);
        }

        private static void Eclipse7Hook()
        {
            IL.RoR2.CharacterBody.RecalculateStats += (il) => GetSelectedEclipseHook(il, 7, DifficultyIndex.Eclipse7);
        }

        private static void Eclipse8Hook()
        {
            IL.RoR2.HealthComponent.TakeDamageProcess += (il) => GetSelectedEclipseHook(il, 8, DifficultyIndex.Eclipse8);
        }

        //Adapted hook method from ZetArtifacts. Source : https://github.com/William758/ZetArtifacts/blob/532af3d3e6775b6441d4025dc05e44c100ebea4d/ZetEclifact.cs#L50
        private static void GetSelectedEclipseHook(ILContext il, int targetArtifact, DifficultyIndex diff)
        {
            ILCursor c = new ILCursor(il);

            bool found = c.TryGotoNext(
                x => x.MatchCall<Run>("get_instance"),
                x => x.MatchCallvirt<Run>("get_selectedDifficulty"),
                x => x.MatchLdcI4((int)diff)
            );

            if (found)
            {
                c.Index += 2;

                c.EmitDelegate<Func<DifficultyIndex, DifficultyIndex>>((diffIndex) =>
                {
                    if (IsArtifactEnabled(targetArtifact)) return diff;

                    return diffIndex;
                });
            }
            else
                Debug.LogWarning($"Eclipse{targetArtifact}Hook Failed");
        }
    }
}
