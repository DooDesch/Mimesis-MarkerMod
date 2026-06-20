using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using MarkerMod.Config;
using MarkerMod.Managers;
using MarkerMod.Services;
using MelonLoader;
using Mimic;
using Mimic.Actors;
using MimicAPI.GameAPI;
using ReluProtocol;
using UnityEngine;

namespace MarkerMod.Patches
{
    [HarmonyPatch(typeof(FieldSkillActor), nameof(FieldSkillActor.Spawn))]
    internal static class FieldSkillActorSpawnPatch
    {
        private static readonly System.Reflection.MethodInfo Destroy2Arg = AccessTools.Method(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new[] { typeof(UnityEngine.Object), typeof(float) });
        private static readonly System.Reflection.MethodInfo DestroyOverride = AccessTools.Method(typeof(PaintPersistenceManager), nameof(PaintPersistenceManager.DestroyWithOverride));

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(Destroy2Arg))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    instruction.operand = DestroyOverride;
                }

                yield return instruction;
            }
        }

        [HarmonyPostfix]
        public static void Postfix(FieldSkillActor __instance, FieldSkillObjectInfo fieldSkillObjectInfo, ref float __result)
        {
            PaintPersistenceManager.TrackDecal(fieldSkillObjectInfo);
            __result = PaintPersistenceManager.OverrideLifetime(fieldSkillObjectInfo, __result);

            if (MarkerPreferences.EnablePaintballColorChange && fieldSkillObjectInfo != null)
            {
                try
                {
                    if (PaintBallColorManager.IsPaintball(fieldSkillObjectInfo.fieldSkillMasterID) && PaintBallColorManager.HasColorBeenSelected())
                    {
                        if (__instance != null)
                        {
                            __instance.StartCoroutine(ApplyColorToProjectileCoroutine(__instance, PaintBallColorManager.GetCurrentColor()));
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    MelonLogger.Error($"[PaintballProjectileColor] Error: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        private static System.Collections.IEnumerator ApplyColorToProjectileCoroutine(FieldSkillActor actor, Color color)
        {
            yield return null;
            yield return null;

            if (actor != null && actor.transform != null)
            {
                MarkerMod.Services.MaterialColorService.ApplyColorToTransform(actor.transform, color, useDefaultMaterial: false);
            }
        }
    }

    [HarmonyPatch(typeof(GameMainBase), "SpawnFieldSkill")]
    internal static class GameMainBaseSpawnFieldSkillPatch
    {
        private static readonly System.Reflection.MethodInfo Destroy2Arg = AccessTools.Method(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new[] { typeof(UnityEngine.Object), typeof(float) });
        private static readonly System.Reflection.MethodInfo DestroyOverride = AccessTools.Method(typeof(PaintPersistenceManager), nameof(PaintPersistenceManager.DestroyWithOverride));

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.Calls(Destroy2Arg))
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    instruction.operand = DestroyOverride;
                }

                yield return instruction;
            }
        }
    }

    [HarmonyPatch(typeof(WorldDecal), nameof(WorldDecal.Activate))]
    internal static class WorldDecalActivatePatch
    {
        [HarmonyPrefix]
        public static void Prefix(WorldDecal __instance)
        {
            if (PaintPersistenceManager.IsPermanentDecal(__instance.decalId))
            {
                __instance.lifetimeMSec = PaintPersistenceManager.PermanentLifetimeMilliseconds;
                __instance.fadeoutMSec = 0L;
            }

            // Bug 3: in game 0.3.0 the colorId -> decal-color pipeline was removed, so SpawnDecal never tints
            // the ground decal (decalColor stays white). Activate() applies decalColor via SetColor() right after
            // this prefix, so set it here to match the currently selected paintball color for paint decals.
            if (MarkerPreferences.EnablePaintballColorChange
                && PaintBallColorManager.HasColorBeenSelected()
                && PaintBallColorManager.GetCurrentColorIndex() != -1
                && PaintPersistenceManager.IsPaintDecal(__instance.decalId))
            {
                __instance.decalColor = PaintBallColorManager.GetCurrentColor();
            }
        }
    }

    [HarmonyPatch(typeof(DecalManager.DecalData), nameof(DecalManager.DecalData.CreateDecalData))]
    internal static class DecalDataCreatePatch
    {
        [HarmonyPrefix]
        public static void Prefix(string pathWithSocket, ref long lifetimeMSec, ref long fadeoutMSec, Transform spawnBase)
        {
            // Check if this is a paintball decal
            bool hasPaintKeyword = !string.IsNullOrEmpty(pathWithSocket) && pathWithSocket.IndexOf("paint", StringComparison.OrdinalIgnoreCase) >= 0;
            bool attachedToActor = spawnBase != null;

            bool keep = false;
            if (MarkerPreferences.KeepFootprints && attachedToActor && hasPaintKeyword)
            {
                keep = true;
            }
            if (MarkerPreferences.KeepPuddles && !attachedToActor && hasPaintKeyword)
            {
                keep = true;
            }

            if (keep)
            {
                lifetimeMSec = PaintPersistenceManager.PermanentLifetimeMilliseconds;
                fadeoutMSec = 0L;
            }

            // NOTE: Decal color tinting via colorId was removed in game 0.3.0
            // (DecalData.DecalColor / WorldDecal.decalColor / FieldSkillMemberInfo.DecalColorId
            // and the CreateDecalData "colorId" parameter no longer exist). The paintball
            // projectile recoloring path in PaintBallColorManager/MaterialColorService still works.
        }
        
        [HarmonyPostfix]
        public static void Postfix(DecalManager.DecalData __result, string pathWithSocket, Transform spawnBase)
        {
            // Track decal if needed
            bool hasPaintKeyword = !string.IsNullOrEmpty(pathWithSocket) && pathWithSocket.IndexOf("paint", StringComparison.OrdinalIgnoreCase) >= 0;
            bool attachedToActor = spawnBase != null;
            
            if (MarkerPreferences.KeepFootprints && attachedToActor && hasPaintKeyword && __result != null)
            {
                PaintPersistenceManager.TrackDecalIfNeeded(__result);
            }
        }
    }

    // Bug 1 (primary, game 0.3.0): a thrown paintball is consumed via the new
    // InventoryController.OnUseSkill_SpawnProjectile path (RemoveFromInventory -> ReserveRemoveInvenItem),
    // which bypasses SetDurability entirely. Re-wire the InfinitePaintballs toggle onto that path: when enabled
    // and the projectile is a paintball, skip the method so the item stays in the inventory.
    [HarmonyPatch(typeof(InventoryController), "OnUseSkill_SpawnProjectile", new[] { typeof(int), typeof(VProjectileObject) })]
    internal static class OnUseSkillSpawnProjectilePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(VProjectileObject projectile)
        {
            if (!MarkerPreferences.InfinitePaintballs || projectile == null)
            {
                return true;
            }

            try
            {
                object projectileInfo = ReflectionHelper.GetFieldValue(projectile, "_projectileInfo");
                if (projectileInfo == null)
                {
                    return true;
                }

                int spawnItemID = ReflectionHelper.GetFieldValue<int>(projectileInfo, "SpawnItemMasterIDonCollision");
                if (PaintBallColorManager.IsPaintball(spawnItemID))
                {
                    // Skip removal + re-pickup setup -> the paintball remains in the inventory (infinite).
                    return false;
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[InfinitePaintballs] OnUseSkill_SpawnProjectile patch error: {ex.Message}");
            }

            return true;
        }
    }

    // Bug 1 (fallback): for the !RemoveFromInventory durability branch (and other skill/looting durability sites),
    // paintball consumption still flows through EquipmentItemElement.SetDurability. Keep blocking that when enabled.
    [HarmonyPatch(typeof(EquipmentItemElement), "SetDurability")]
    internal static class EquipmentItemElementSetDurabilityPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(EquipmentItemElement __instance, int durability)
        {
            if (!MarkerPreferences.InfinitePaintballs)
            {
                return true;
            }

            try
            {
                int itemMasterID = MimicAPI.GameAPI.ReflectionHelper.GetFieldValue<int>(__instance, "ItemMasterID");

                if (PaintBallColorManager.IsPaintball(itemMasterID))
                {
                    int currentDurability = MimicAPI.GameAPI.ReflectionHelper.GetPropertyValue<int>(__instance, "RemainDurability");
                    
                    if (durability < currentDurability)
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[InfinitePaintballs] Error in SetDurability patch: {ex.Message}");
            }

            return true;
        }
    }
}
