using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace EclipseArtifacts
{
    public static class EclipseArtifactsBehavior
    {
        private static bool _hooksEnabled = false;

        public static bool IsArtifactEnabled(int eclipseLevel)
        {
            if (RunArtifactManager.instance == null)
                return false;

            ArtifactDef artifact = EclipseArtifactsContent.Artifacts.GetArtifactDefFromEclipseLevel(eclipseLevel);
            if (artifact == null)
                return false;

            return RunArtifactManager.instance.IsArtifactEnabled(artifact);
        }

        public static void EnableHooks()
        {
            if (_hooksEnabled) return;

            IL.RoR2.CharacterMaster.OnBodyStart += (il) => GetSelectedEclipseHook(il, 1, DifficultyIndex.Eclipse1);
            IL.RoR2.HoldoutZoneController.DoUpdate += (il) => GetSelectedEclipseHook(il, 2, DifficultyIndex.Eclipse2);
            IL.RoR2.GlobalEventManager.OnCharacterHitGroundServer += (il) => GetSelectedEclipseHook(il, 3, DifficultyIndex.Eclipse3);
            IL.RoR2.CharacterBody.RecalculateStats += (il) => GetSelectedEclipseHook(il, 4, DifficultyIndex.Eclipse4);
            IL.RoR2.HealthComponent.Heal += (il) => GetSelectedEclipseHook(il, 5, DifficultyIndex.Eclipse5);
            IL.RoR2.DeathRewards.OnKilledServer += (il) => GetSelectedEclipseHook(il, 6, DifficultyIndex.Eclipse6);
            IL.RoR2.CharacterBody.RecalculateStats += (il) => GetSelectedEclipseHook(il, 7, DifficultyIndex.Eclipse7);
            IL.RoR2.HealthComponent.TakeDamageProcess += (il) => GetSelectedEclipseHook(il, 8, DifficultyIndex.Eclipse8);

            _hooksEnabled = true;
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
