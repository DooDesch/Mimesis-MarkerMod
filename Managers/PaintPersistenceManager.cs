using System;
using System.Collections.Generic;
using System.Reflection;
using Bifrost.Cooked;
using MarkerMod.Config;
using ReluProtocol;
using HarmonyLib;
using UnityEngine;

namespace MarkerMod.Managers
{
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

        internal static long PermanentLifetimeMilliseconds
        {
            get
            {
                double milliseconds = MarkerPreferences.PermanentLifetimeSeconds * 1000d;
                return milliseconds >= long.MaxValue ? long.MaxValue : (long)Math.Max(1d, milliseconds);
            }
        }

        internal static bool ShouldKeep(FieldSkillObjectInfo info)
        {
            if (info == null)
            {
                return false;
            }

            if (MarkerPreferences.KeepFootprints && info.fieldSkillMasterID == PaintspotMasterId)
            {
                return true;
            }

            if (MarkerPreferences.KeepPuddles && PaintballMasterIds.Contains(info.fieldSkillMasterID))
            {
                return true;
            }

            return false;
        }

        internal static float OverrideLifetime(FieldSkillObjectInfo info, float originalSeconds)
        {
            return ShouldKeep(info) ? MathF.Max(originalSeconds, MarkerPreferences.PermanentLifetimeSeconds) : originalSeconds;
        }

        internal static void DestroyWithOverride(UnityEngine.Object target, float originalSeconds, FieldSkillObjectInfo info)
        {
            UnityEngine.Object.Destroy(target, OverrideLifetime(info, originalSeconds));
        }

        internal static void TrackDecal(FieldSkillObjectInfo info)
        {
            if (!MarkerPreferences.KeepFootprints || info == null || info.fieldSkillMasterID != PaintspotMasterId)
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

            if (!keep && MarkerPreferences.KeepFootprints && attachedToActor && hasPaintKeyword)
            {
                keep = true;
            }

            if (!keep && MarkerPreferences.KeepPuddles && !attachedToActor && hasPaintKeyword)
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
            LifetimeField?.SetValue(decalData, lifetime);
            FadeoutField?.SetValue(decalData, 0L);
        }

        private static bool ContainsPaintKeyword(string value)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf("paint", StringComparison.OrdinalIgnoreCase) >= 0;
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
            return (getter == null) ? _ => null : AccessTools.MethodDelegate<Func<Hub, DataManager>>(getter);
        }
    }
}

