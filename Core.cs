using HarmonyLib;
using MarkerMod.Configuration;
using MelonLoader;

[assembly: MelonInfo(typeof(MarkerMod.Core), "MarkerMod", "1.0.0", "DooDesch", null)]
[assembly: MelonGame("ReLUGames", "MIMESIS")]

namespace MarkerMod
{
    public class Core : MelonMod
    {
        private HarmonyLib.Harmony _harmony;

        public override void OnInitializeMelon()
        {
            MarkerPreferences.Initialize();

            _harmony = new HarmonyLib.Harmony("MarkerMod.PaintPersistence");
            _harmony.PatchAll();

            LoggerInstance.Msg($"MarkerMod ready | lifespan={MarkerPreferences.PermanentLifetimeSeconds:F0}s footprints={MarkerPreferences.KeepFootprints} puddles={MarkerPreferences.KeepPuddles}");
        }

        public override void OnDeinitializeMelon()
        {
            _harmony?.UnpatchSelf();
        }
    }
}