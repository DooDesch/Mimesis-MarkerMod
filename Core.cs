using MarkerMod.Configuration;
using MelonLoader;

[assembly: MelonInfo(typeof(MarkerMod.Core), "MarkerMod", "1.0.0", "DooDesch", null)]
[assembly: MelonGame("ReLUGames", "MIMESIS")]

namespace MarkerMod
{
    public class Core : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MarkerPreferences.Initialize();
			HarmonyInstance.PatchAll();
            LoggerInstance.Msg($"MarkerMod initialized. Lifespan={MarkerPreferences.PermanentLifetimeSeconds:F0}s, Footprints={MarkerPreferences.KeepFootprints}, Puddles={MarkerPreferences.KeepPuddles}, InfinitePaintballs={MarkerPreferences.InfinitePaintballs}");
        }
    }
}