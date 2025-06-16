using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using RoR2;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace EclipseArtifacts
{
    public class EclipseRefurbishedCompat
    {
        private static bool? _enabled;
        private static bool _hooksEnabled = false;
        private static string _creditsModifierDesc;

        public static bool Enabled
        {
            get
            {
                _enabled ??= BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("com.Puporongod.EclipseRefurbished");
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static string AddDirectorCreditsToDescription(string description)
        {
            _creditsModifierDesc ??= Language.GetString("ECLIPSE_1_DESCRIPTION").Split('\n').FirstOrDefault(s => s.StartsWith("Director Credits:")).TrimEnd('\n');

            var descriptionLines = description.Split("\n").ToList();
            descriptionLines.Add($"<style=cStack>>{_creditsModifierDesc} (Non-Stacking)</style>");

            return string.Join('\n', descriptionLines);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void EnableHooks()
        {
            if (_hooksEnabled) return;

            HookEndpointManager.Modify(GetCombatDirectorEnableMethod(), Modify_CombatDirector_OnEnable);
            HookEndpointManager.Modify(GetATKSpeedMethod(), Modify_ATKSpeedAndAWUCooldown);

            _hooksEnabled = true;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void DisableHooks()
        {
            if(!_hooksEnabled) return;

            HookEndpointManager.Unmodify(GetCombatDirectorEnableMethod(), Modify_CombatDirector_OnEnable);
            HookEndpointManager.Unmodify(GetATKSpeedMethod(), Modify_ATKSpeedAndAWUCooldown);

            _hooksEnabled = false;
        }

        public static void Modify_CombatDirector_OnEnable(ILContext il)
        {
            EclipseArtifactsBehavior.ILHook_GetSelectedDifficulty(il, overrideDiffWhenAnyArtifactIsEnabled: true);
        }

        public static void Modify_ATKSpeedAndAWUCooldown(ILContext il)
        {
            EclipseArtifactsBehavior.ILHook_GetSelectedDifficulty(il);
        }

        private static MethodInfo GetCombatDirectorEnableMethod()
        {
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            return typeof(EclipseRefurbished.EclipseRefurbished).GetMethod("CombatDirector_OnEnable", bindingFlags);
        }

        private static MethodInfo GetATKSpeedMethod()
        {
            var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
            return typeof(EclipseRefurbished.EclipseRefurbished).GetMethod("ATKSpeedAndAWUCooldown", bindingFlags);
        }
    }
}
