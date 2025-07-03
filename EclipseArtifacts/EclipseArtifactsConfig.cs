using BepInEx.Configuration;

namespace EclipseArtifacts
{
    public static class EclipseArtifactsConfig
    {
        public enum FrailtyCompatMode
        {
            Vanilla,
            Additive,
            Multiplicative
        }

        public static ConfigEntry<FrailtyCompatMode> frailtyCompatMode;

        public static void Init(ConfigFile config)
        {
            frailtyCompatMode = config.Bind("Compatibility", "Artifact of Frailty", FrailtyCompatMode.Additive, 
                "Determines how `Artifact of Eclipse 3` and `Artifact of Frailty` interacts with each other."
                + "\n- Vanilla: only counts 1 of them (+100% damage)"
                + "\n- Additive: adds the values (+200% damage)"
                + "\n- Multiplicative: multiplies the values (+300% damage)"
            );

            if (RiskOfOptionsCompat.Enabled)
                RiskOfOptionsCompat.AddConfig();
        }
    }
}
