using HarmonyLib;
using MelonLoader;
using System;

[assembly: MelonInfo(typeof(MarkerMod.Core), "MarkerMod", "1.0.0", "DooDesch", null)]
[assembly: MelonGame("ReLUGames", "MIMESIS")]

namespace MarkerMod
{
    public class Core : MelonMod
    {
        private HarmonyLib.Harmony _harmony;

        internal static MelonPreferences_Category PrefCategory { get; private set; }
        internal static MelonPreferences_Entry<bool> KeepFootprintsEntry { get; private set; }
        internal static MelonPreferences_Entry<bool> KeepPuddlesEntry { get; private set; }
        internal static MelonPreferences_Entry<float> LifetimeSecondsEntry { get; private set; }

        internal static float PermanentLifetimeSeconds => MathF.Max(1f, LifetimeSecondsEntry?.Value ?? 86400f);

        internal static bool KeepFootprints => KeepFootprintsEntry?.Value ?? true;
        internal static bool KeepPuddles => KeepPuddlesEntry?.Value ?? false;

        public override void OnInitializeMelon()
        {
            PrefCategory = MelonPreferences.CreateCategory("MarkerMod", "Marker Mod");
            KeepFootprintsEntry = PrefCategory.CreateEntry("keepFootprints", true, "Keep paint footprints after puddles dry?");
            KeepPuddlesEntry = PrefCategory.CreateEntry("keepPuddles", false, "Keep spawned paint puddles as well?");
            LifetimeSecondsEntry = PrefCategory.CreateEntry("permanentLifetimeSeconds", 86400f, "Lifetime in seconds for persistent paint");
            PrefCategory.SaveToFile(false);

            _harmony = new HarmonyLib.Harmony("MarkerMod.PaintPersistence");
            _harmony.PatchAll();

            LoggerInstance.Msg($"Initialized paint persistence. Lifetime={PermanentLifetimeSeconds:F0}s, footprints={(KeepFootprints ? "on" : "off")}, puddles={(KeepPuddles ? "on" : "off")}.");
        }

        public override void OnDeinitializeMelon()
        {
            _harmony?.UnpatchSelf();
        }
    }
}