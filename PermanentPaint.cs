using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using HarmonyLib;
using Mimic.Actors;
using ReluProtocol;
using Bifrost.Cooked;
using UnityEngine;

namespace MarkerMod;

internal static class PaintPersistenceManager
{
    private const int PaintspotMasterId = 71001;
    private static readonly HashSet<int> PaintballMasterIds = new()
    {
        70010, 70011, 70012, 70013, 70014, 70015, 70016, 70017, 70018
    };

    private static readonly HashSet<string> PermanentDecalIds = new(StringComparer.OrdinalIgnoreCase);
    private static readonly object DecalLock = new();
    private static readonly Func<Hub, DataManager> DataManagerGetter = CreateDataManagerGetter();
    private static readonly FieldInfo LifetimeField = AccessTools.Field(typeof(DecalManager.DecalData), "LifetimeMSec");
    private static readonly FieldInfo FadeoutField = AccessTools.Field(typeof(DecalManager.DecalData), "FadeoutMSec");

    internal static bool ShouldKeep(FieldSkillObjectInfo info)
    {
        if (info == null)
        {
            return false;
        }

        if (Core.KeepFootprints && info.fieldSkillMasterID == PaintspotMasterId)
        {
            return true;
        }

        if (Core.KeepPuddles && PaintballMasterIds.Contains(info.fieldSkillMasterID))
        {
            return true;
        }

        return false;
    }

    internal static float OverrideLifetime(FieldSkillObjectInfo info, float originalSeconds)
    {
        return ShouldKeep(info) ? MathF.Max(originalSeconds, Core.PermanentLifetimeSeconds) : originalSeconds;
    }

    internal static long PermanentLifetimeMilliseconds
    {
        get
        {
            double milliseconds = Core.PermanentLifetimeSeconds * 1000d;
            return milliseconds >= long.MaxValue ? long.MaxValue : (long)Math.Max(1d, milliseconds);
        }
    }

    internal static void DestroyWithOverride(UnityEngine.Object target, float originalTime, FieldSkillObjectInfo info)
    {
        UnityEngine.Object.Destroy(target, OverrideLifetime(info, originalTime));
    }

    internal static void TrackDecal(FieldSkillObjectInfo info)
    {
        if (!Core.KeepFootprints || info == null || info.fieldSkillMasterID != PaintspotMasterId)
        {
            return;
        }

        FieldSkillMemberInfo memberInfo = ResolveMemberInfo(info);
        if (memberInfo == null || string.IsNullOrEmpty(memberInfo.DecalId))
        {
            return;
        }

        lock (DecalLock)
        {
            PermanentDecalIds.Add(memberInfo.DecalId);
        }
    }

    internal static bool IsPermanentDecal(string decalId)
    {
        if (string.IsNullOrWhiteSpace(decalId))
        {
            return false;
        }

        lock (DecalLock)
        {
            return PermanentDecalIds.Contains(decalId);
        }
    }

    private static FieldSkillMemberInfo ResolveMemberInfo(FieldSkillObjectInfo info)
    {
        Hub hub = Hub.s;
        if (hub == null)
        {
            return null;
        }

        DataManager dataManager = DataManagerGetter(hub);
        if (dataManager == null)
        {
            return null;
        }

        FieldSkillInfo fieldSkillInfo = dataManager.ExcelDataManager.GetFieldSkillData(info.fieldSkillMasterID);
        if (fieldSkillInfo == null)
        {
            return null;
        }

        return fieldSkillInfo.FieldSkillMemberInfos.TryGetValue(info.fieldSkillIndex, out FieldSkillMemberInfo value) ? value : null;
    }

    private static Func<Hub, DataManager> CreateDataManagerGetter()
    {
        MethodInfo getter = AccessTools.PropertyGetter(typeof(Hub), "dataman");
        if (getter == null)
        {
            return _ => null;
        }

        return AccessTools.MethodDelegate<Func<Hub, DataManager>>(getter);
    }
    internal static void EnsureDecalLifetime(DecalManager.DecalData decalData)
    {
        if (decalData == null)
        {
            return;
        }

        bool keep = false;
        if (IsPermanentDecal(decalData.DecalId))
        {
            keep = true;
        }

        bool hasPaintKeyword = ContainsPaintKeyword(decalData.DecalId) || ContainsPaintKeyword(decalData.Socket);
        bool attachedToActor = decalData.SpawnBase != null;

        if (!keep && Core.KeepFootprints && attachedToActor && hasPaintKeyword)
        {
            keep = true;
        }

        if (!keep && Core.KeepPuddles && !attachedToActor && hasPaintKeyword)
        {
            keep = true;
        }

        if (!keep)
        {
            return;
        }

        TrySetDecalLifetime(decalData, PermanentLifetimeMilliseconds);
    }

    private static void TrySetDecalLifetime(DecalManager.DecalData decalData, long lifetime)
    {
        if (LifetimeField != null)
        {
            LifetimeField.SetValue(decalData, lifetime);
        }
        if (FadeoutField != null)
        {
            FadeoutField.SetValue(decalData, 0L);
        }
    }

    private static bool ContainsPaintKeyword(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }
        return value.IndexOf("paint", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

[HarmonyPatch(typeof(FieldSkillActor), nameof(FieldSkillActor.Spawn))]
internal static class FieldSkillActorSpawnPatches
{
    private static readonly System.Reflection.MethodInfo Destroy2Arg = AccessTools.Method(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new[] { typeof(UnityEngine.Object), typeof(float) })!;
    private static readonly System.Reflection.MethodInfo DestroyOverride = AccessTools.Method(typeof(PaintPersistenceManager), nameof(PaintPersistenceManager.DestroyWithOverride))!;

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
    private static readonly System.Reflection.MethodInfo Destroy2Arg = AccessTools.Method(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy), new[] { typeof(UnityEngine.Object), typeof(float) })!;
    private static readonly System.Reflection.MethodInfo DestroyOverride = AccessTools.Method(typeof(PaintPersistenceManager), nameof(PaintPersistenceManager.DestroyWithOverride))!;

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

