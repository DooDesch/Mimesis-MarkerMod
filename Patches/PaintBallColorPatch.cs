using HarmonyLib;
using MarkerMod.Config;
using MarkerMod.Managers;
using MelonLoader;
using Mimic;
using Mimic.Actors;
using Mimic.InputSystem;
using MimicAPI.GameAPI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MarkerMod.Patches
{
    [HarmonyPatch(typeof(ProtoActor), "Update")]
    internal static class PaintBallColorUpdatePatch
    {
        private static long lastSelectedItemID = -1;
        private static bool wasRightMouseButtonPressedLastFrame = false;

        [HarmonyPostfix]
        public static void Postfix(ProtoActor __instance)
        {
            if (!MarkerPreferences.EnablePaintballColorChange)
            {
                return;
            }

            try
            {
                if (!__instance.AmIAvatar())
                {
                    return;
                }

                ProtoActor localPlayer = PlayerAPI.GetLocalPlayer();
                if (localPlayer == null)
                {
                    lastSelectedItemID = -1;
                    return;
                }

                object inventory = PlayerAPI.GetLocalInventory();
                if (inventory == null)
                {
                    lastSelectedItemID = -1;
                    return;
                }

                object selectedItemObj = ReflectionHelper.GetPropertyValue(inventory, "SelectedItem");
                if (selectedItemObj == null)
                {
                    if (lastSelectedItemID != -1)
                    {
                        MelonLogger.Msg("[PaintBallColor] No item selected");
                        lastSelectedItemID = -1;
                    }
                    return;
                }

                object itemIDObj = ReflectionHelper.GetPropertyValue(selectedItemObj, "ItemID");
                object itemMasterIDObj = ReflectionHelper.GetPropertyValue(selectedItemObj, "ItemMasterID");

                if (itemIDObj == null || itemMasterIDObj == null)
                {
                    return;
                }

                long itemID = Convert.ToInt64(itemIDObj);
                int itemMasterID = Convert.ToInt32(itemMasterIDObj);

                bool isPaintball = PaintBallColorManager.IsPaintball(itemMasterID);

                if (isPaintball && itemID != lastSelectedItemID)
                {
                    PaintBallColorManager.ResetColorSelection();
                    PaintBallColorManager.ApplyColorToPaintball(selectedItemObj);
                    lastSelectedItemID = itemID;
                }
                else if (!isPaintball)
                {
                    if (lastSelectedItemID != -1)
                    {
                        lastSelectedItemID = -1;
                        PaintBallColorManager.ResetColorSelection();
                    }
                }

                if (isPaintball)
                {
                    Mouse mouse = Mouse.current;
                    if (mouse != null)
                    {
                        bool isRightMouseButtonPressed = mouse.rightButton.isPressed;
                        bool wasRightMouseButtonPressedThisFrame = isRightMouseButtonPressed && !wasRightMouseButtonPressedLastFrame;
                        wasRightMouseButtonPressedLastFrame = isRightMouseButtonPressed;

                        if (wasRightMouseButtonPressedThisFrame)
                        {
                            PaintBallColorManager.CycleColor();
                            PaintBallColorManager.ApplyColorToPaintball(selectedItemObj);
                            string colorName = PaintBallColorManager.GetCurrentColorName();
                            MelonLogger.Msg($"[PaintBallColor] Farbe geändert zu: {colorName}");
                        }
                    }
                }
                else
                {
                    wasRightMouseButtonPressedLastFrame = false;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[PaintBallColor] Error: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}

