using System;
using MelonLoader;
using UnityEngine;

namespace MarkerMod.Config
{
	internal static class MarkerPreferences
	{
		private const string CategoryId = "MarkerMod";

		private static MelonPreferences_Category _category;
		private static MelonPreferences_Entry<bool> _keepFootprints;
		private static MelonPreferences_Entry<bool> _keepPuddles;
		private static MelonPreferences_Entry<float> _lifetimeSeconds;
		private static MelonPreferences_Entry<bool> _infinitePaintballs;
		private static MelonPreferences_Entry<bool> _enablePaintballColorChange;

		internal static void Initialize()
		{
			if (_category != null)
			{
				return;
			}

			_category = MelonPreferences.CreateCategory(CategoryId, "Marker Mod");
			_keepFootprints = CreateEntry("keepFootprints", true, "Keep Footprints", "Keep paint footprints after puddles dry. When enabled, paint footprints will persist even after the paint puddles have dried up. Default: true");
			_keepPuddles = CreateEntry("keepPuddles", false, "Keep Puddles", "Keep spawned paint puddles as well. When enabled, both paint footprints and paint puddles will persist. Default: false");
			_lifetimeSeconds = CreateEntry("permanentLifetimeSeconds", 86400f, "Lifetime", "Lifetime in seconds for persistent paint. This determines how long paint marks will remain visible. Default: 86400 (24 hours)");
			_infinitePaintballs = CreateEntry("infinitePaintballs", false, "Infinite Paintballs", "Paintballs are not consumed when thrown. When enabled, you can throw paintballs infinitely without them being removed from your inventory. Default: false");
			_enablePaintballColorChange = CreateEntry("enablePaintballColorChange", true, "Enable Paintball Color Change", "Allow changing paintball color by right-clicking. When enabled, you can cycle through colors (Red, Yellow, Green, Blue, Orange, Purple, White, Black) by right-clicking while holding a paintball. Default: true");
		}

		private static MelonPreferences_Entry<T> CreateEntry<T>(string identifier, T defaultValue, string displayName, string description = null)
		{
			if (_category == null)
			{
				throw new InvalidOperationException("Preference category not initialized.");
			}

			return _category.CreateEntry(identifier, defaultValue, displayName, description);
		}

		internal static bool KeepFootprints => _keepFootprints.Value;

		internal static bool KeepPuddles => _keepPuddles.Value;

		internal static float PermanentLifetimeSeconds => Mathf.Max(1f, _lifetimeSeconds.Value);

		internal static bool InfinitePaintballs => _infinitePaintballs.Value;

		internal static bool EnablePaintballColorChange => _enablePaintballColorChange.Value;
	}
}

