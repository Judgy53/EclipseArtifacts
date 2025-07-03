using MonoMod.Cil;
using RoR2;
using System;

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

            IL.RoR2.CharacterMaster.OnBodyStart += (il) => ILHook_GetSelectedDifficulty(il);
            IL.RoR2.HoldoutZoneController.DoUpdate += (il) => ILHook_GetSelectedDifficulty(il);
            IL.RoR2.GlobalEventManager.OnCharacterHitGroundServer += (il) => ILHook_GetSelectedDifficulty(il);
            IL.RoR2.CharacterBody.RecalculateStats += (il) => ILHook_GetSelectedDifficulty(il);
            IL.RoR2.HealthComponent.Heal += (il) => ILHook_GetSelectedDifficulty(il);
            IL.RoR2.DeathRewards.OnKilledServer += (il) => ILHook_GetSelectedDifficulty(il);
            IL.RoR2.CharacterBody.RecalculateStats += (il) => ILHook_GetSelectedDifficulty(il);
            IL.RoR2.HealthComponent.TakeDamageProcess += (il) => ILHook_GetSelectedDifficulty(il);

            //Compat between E3 and Frailty
            IL.RoR2.GlobalEventManager.OnCharacterHitGroundServer += ILHook_FrailtyCompat;

            if (EclipseRefurbishedCompat.Enabled)
                EclipseRefurbishedCompat.EnableHooks();

            _hooksEnabled = true;
        }

        public static void DisableHooks()
        {
            if (!_hooksEnabled) return;

            IL.RoR2.CharacterMaster.OnBodyStart -= (il) => ILHook_GetSelectedDifficulty(il);
            IL.RoR2.HoldoutZoneController.DoUpdate -= (il) => ILHook_GetSelectedDifficulty(il);
            IL.RoR2.GlobalEventManager.OnCharacterHitGroundServer -= (il) => ILHook_GetSelectedDifficulty(il);
            IL.RoR2.CharacterBody.RecalculateStats -= (il) => ILHook_GetSelectedDifficulty(il);
            IL.RoR2.HealthComponent.Heal -= (il) => ILHook_GetSelectedDifficulty(il);
            IL.RoR2.DeathRewards.OnKilledServer -= (il) => ILHook_GetSelectedDifficulty(il);
            IL.RoR2.CharacterBody.RecalculateStats -= (il) => ILHook_GetSelectedDifficulty(il);
            IL.RoR2.HealthComponent.TakeDamageProcess -= (il) => ILHook_GetSelectedDifficulty(il);

            IL.RoR2.GlobalEventManager.OnCharacterHitGroundServer -= ILHook_FrailtyCompat;

            if (EclipseRefurbishedCompat.Enabled)
                EclipseRefurbishedCompat.DisableHooks();

            _hooksEnabled = false;
        }


        //Adapted hook method from ZetArtifacts. Source : https://github.com/William758/ZetArtifacts/blob/532af3d3e6775b6441d4025dc05e44c100ebea4d/ZetEclifact.cs#L50
        public static void ILHook_GetSelectedDifficulty(ILContext il, bool overrideDiffWhenAnyArtifactIsEnabled = false)
        {
            var cursor = new ILCursor(il);
            bool found = false;

            do
            {
                int checkedDiff = -1;
                found = cursor.TryGotoNext( // Run.instance.get_selectedDifficulty >= {checkedDiff}
                    x => x.MatchCall<Run>("get_instance"),
                    x => x.MatchCallvirt<Run>("get_selectedDifficulty"),
                    x => x.MatchLdcI4(out checkedDiff)
                );

                if (found && checkedDiff >= (int)DifficultyIndex.Eclipse1 && checkedDiff <= (int)DifficultyIndex.Eclipse8)
                {
                    cursor.Index += 2;
                    cursor.EmitDelegate<Func<DifficultyIndex, DifficultyIndex>>((currentDiffIndex) =>
                    {
                        if (ShouldOverrideDifficulty(checkedDiff, overrideDiffWhenAnyArtifactIsEnabled))
                        {
                            //Log.LogDebug($"{il.Method?.FullName}: overriden (checkedDiff: {checkedDiff - (int)DifficultyIndex.Hard}, any: {overrideDiffWhenAnyArtifactIsEnabled})");
                            return (DifficultyIndex)checkedDiff;
                        }
                        //Log.LogDebug($"{il.Method?.FullName}: NOT overriden (checkedDiff: {checkedDiff - (int)DifficultyIndex.Hard}, any: {overrideDiffWhenAnyArtifactIsEnabled})");

                        return currentDiffIndex;
                    });
                }
            } while (found);
        }

        public static bool ShouldOverrideDifficulty(int checkedDiff, bool overrideDiffWhenAnyArtifactIsEnabled)
        {
            int eclipseIndex = checkedDiff - (int)DifficultyIndex.Hard;

            for (int i = 1; i <= 8; i++)
            {
                if (i != eclipseIndex && overrideDiffWhenAnyArtifactIsEnabled == false)
                    continue;

                if (IsArtifactEnabled(i))
                    return true;
            }

            return false;
        }

        public static void ILHook_FrailtyCompat(ILContext il)
        {
            var cursor = new ILCursor(il);

            bool found = cursor.TryGotoNext(
                x => x.MatchLdloc(7),
                x => x.MatchDup(),
                x => x.MatchLdfld<DamageInfo>("damage"),
                x => x.MatchLdcR4(2),
                x => x.MatchMul()
            );

            if (!found)
            {
                Log.LogError("FrailtyCompat hook failed");
                return;
            }

            cursor.Index += 3; // Next = ldc.r4 2
            cursor.Remove();

            cursor.EmitDelegate(() =>
            {
                bool frailtyEnabled = RunArtifactManager.instance.IsArtifactEnabled(RoR2Content.Artifacts.weakAssKneesArtifactDef);
                bool e3Enabled = IsArtifactEnabled(3);

                if (frailtyEnabled && e3Enabled)
                {
                    switch (EclipseArtifactsConfig.frailtyCompatMode.Value)
                    {
                        case EclipseArtifactsConfig.FrailtyCompatMode.Vanilla:
                            return 2f; // default * 2 = 100% total
                        case EclipseArtifactsConfig.FrailtyCompatMode.Additive:
                            return 2f + 1f; // default * 2, added +100% = +200% total
                        case EclipseArtifactsConfig.FrailtyCompatMode.Multiplicative:
                            return 2f * 2f; // default * 2, added * 2 = +300% total
                    }
                }

                return 2f;
            });
        }
    }
}
