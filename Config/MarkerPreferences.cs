using System;
using MelonLoader;

namespace MarkerMod.Configuration
{
    internal static class MarkerPreferences
    {
        private const string CategoryId = "MarkerMod";
        private const string CategoryDisplay = "Marker Mod";

        private static bool _initialized;

        private static MelonPreferences_Category _category;
        private static MelonPreferences_Entry<bool> _keepFootprints;
        private static MelonPreferences_Entry<bool> _keepPuddles;
        private static MelonPreferences_Entry<float> _lifetimeSeconds;
        private static MelonPreferences_Entry<bool> _infinitePaintballs;

        public static bool KeepFootprints => _keepFootprints?.Value ?? true;
        public static bool KeepPuddles => _keepPuddles?.Value ?? false;
        public static float PermanentLifetimeSeconds => MathF.Max(1f, _lifetimeSeconds?.Value ?? 86400f);
        public static bool InfinitePaintballs => _infinitePaintballs?.Value ?? false;

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _category = MelonPreferences.CreateCategory(CategoryId, CategoryDisplay);
            _keepFootprints = _category.CreateEntry("keepFootprints", true, "Keep paint footprints after puddles dry?");
            _keepPuddles = _category.CreateEntry("keepPuddles", false, "Keep spawned paint puddles as well?");
            _lifetimeSeconds = _category.CreateEntry("permanentLifetimeSeconds", 86400f, "Lifetime in seconds for persistent paint");
            _infinitePaintballs = _category.CreateEntry("infinitePaintballs", false, "Paintballs are not consumed when thrown (infinite paintballs)");
            _category.SaveToFile(false);

            _initialized = true;
        }
    }
}

