using System.Linq;
using HarmonyLib;
using MarkerMod.Config;
using MelonLoader;

[assembly: MelonInfo(typeof(MarkerMod.Core), "MarkerMod", "1.3.0", "DooDesch", null)]
[assembly: MelonGame("ReLUGames", "MIMESIS")]
[assembly: MelonOptionalDependencies("MimicAPI")]

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