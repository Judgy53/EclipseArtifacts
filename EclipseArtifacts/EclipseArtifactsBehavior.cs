using MonoMod.Cil;
using RoR2;

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

            IL.RoR2.CharacterMaster.OnBodyStart += (il) => ReplaceCall_Run_GetSelectedDifficulty(il);
            IL.RoR2.HoldoutZoneController.DoUpdate += (il) => ReplaceCall_Run_GetSelectedDifficulty(il);
            IL.RoR2.GlobalEventManager.OnCharacterHitGroundServer += (il) => ReplaceCall_Run_GetSelectedDifficulty(il);
            IL.RoR2.CharacterBody.RecalculateStats += (il) => ReplaceCall_Run_GetSelectedDifficulty(il);
            IL.RoR2.HealthComponent.Heal += (il) => ReplaceCall_Run_GetSelectedDifficulty(il);
            IL.RoR2.DeathRewards.OnKilledServer += (il) => ReplaceCall_Run_GetSelectedDifficulty(il);
            IL.RoR2.CharacterBody.RecalculateStats += (il) => ReplaceCall_Run_GetSelectedDifficulty(il);
            IL.RoR2.HealthComponent.TakeDamageProcess += (il) => ReplaceCall_Run_GetSelectedDifficulty(il);

            if (EclipseRefurbishedCompat.Enabled)
                EclipseRefurbishedCompat.EnableHooks();

            _hooksEnabled = true;
        }

        public static void DisableHooks()
        {
            if (!_hooksEnabled) return;

            IL.RoR2.CharacterMaster.OnBodyStart -= (il) => ReplaceCall_Run_GetSelectedDifficulty(il);
            IL.RoR2.HoldoutZoneController.DoUpdate -= (il) => ReplaceCall_Run_GetSelectedDifficulty(il);
            IL.RoR2.GlobalEventManager.OnCharacterHitGroundServer -= (il) => ReplaceCall_Run_GetSelectedDifficulty(il);
            IL.RoR2.CharacterBody.RecalculateStats -= (il) => ReplaceCall_Run_GetSelectedDifficulty(il);
            IL.RoR2.HealthComponent.Heal -= (il) => ReplaceCall_Run_GetSelectedDifficulty(il);
            IL.RoR2.DeathRewards.OnKilledServer -= (il) => ReplaceCall_Run_GetSelectedDifficulty(il);
            IL.RoR2.CharacterBody.RecalculateStats -= (il) => ReplaceCall_Run_GetSelectedDifficulty(il);
            IL.RoR2.HealthComponent.TakeDamageProcess -= (il) => ReplaceCall_Run_GetSelectedDifficulty(il);

            if (EclipseRefurbishedCompat.Enabled)
                EclipseRefurbishedCompat.DisableHooks();

            _hooksEnabled = false;
        }

        //Adapted hook method from ZetArtifacts. Source : https://github.com/William758/ZetArtifacts/blob/532af3d3e6775b6441d4025dc05e44c100ebea4d/ZetEclifact.cs#L50
        public static void ReplaceCall_Run_GetSelectedDifficulty(ILContext il, bool overrideDiffWhenAnyArtifactIsEnabled = false)
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
                    int eclipseIndex = checkedDiff - (int)DifficultyIndex.Hard;

                    var labels = cursor.IncomingLabels; //Save labels pointing to next instr before removing them.

                    cursor.RemoveRange(2); //Remove `Run.instance.get_selectedDifficulty`
                    cursor.EmitDelegate(() =>
                    {
                        for (int i = 1; i <= 8; i++)
                        {
                            if ((i == eclipseIndex || overrideDiffWhenAnyArtifactIsEnabled) && IsArtifactEnabled(i))
                                return (DifficultyIndex)checkedDiff;
                        }

                        return Run.instance.selectedDifficulty;
                    });

                    //Restore saved labels
                    cursor.Index -= 2;
                    foreach (var label in labels) 
                        cursor.MarkLabel(label);
                    cursor.Index += 2;
                }
            } while (found);
        }
    }
}
