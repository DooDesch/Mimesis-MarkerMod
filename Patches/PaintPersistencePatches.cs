using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using MarkerMod.Managers;
using Mimic.Actors;
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
        public static void Postfix(FieldSkillObjectInfo fieldSkillObjectInfo, ref float __result)
        {
            PaintPersistenceManager.TrackDecal(fieldSkillObjectInfo);
            __result = PaintPersistenceManager.OverrideLifetime(fieldSkillObjectInfo, __result);
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
        }
    }

    [HarmonyPatch(typeof(DecalManager.DecalData), nameof(DecalManager.DecalData.CreateDecalData))]
    internal static class DecalDataCreatePatch
    {
        [HarmonyPostfix]
        public static void Postfix(DecalManager.DecalData __result)
        {
            PaintPersistenceManager.EnsureDecalLifetime(__result);
        }
    }
}

