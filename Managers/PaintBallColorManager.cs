using System.Collections.Generic;
using UnityEngine;
using MimicAPI.GameAPI;
using MelonLoader;

namespace MarkerMod.Managers
{
    internal static class PaintBallColorManager
    {
        private static readonly List<Color> ColorPalette = new()
        {
            new Color(1f, 0f, 0f, 1f),     // Rot
            new Color(1f, 1f, 0f, 1f),     // Gelb
            new Color(0f, 1f, 0f, 1f),     // Grün
            new Color(0f, 0f, 1f, 1f),     // Blau
            new Color(1f, 0.5f, 0f, 1f),  // Orange
            new Color(0.5f, 0f, 1f, 1f),  // Lila
            new Color(1f, 1f, 1f, 1f),    // Weiß
            new Color(0f, 0f, 0f, 1f),    // Schwarz
        };

        private static int currentColorIndex = -1;
        private static bool hasColorBeenSelected = false;
        private static readonly HashSet<int> PaintballMasterIDs = new()
        {
            2030, 70010, 70011, 70012, 70013, 70014, 70015, 70016, 70017, 70018
        };
        
        private static readonly Dictionary<string, Material> originalMaterialsCache = new Dictionary<string, Material>();

        internal static void CycleColor()
        {
            hasColorBeenSelected = true;
            if (currentColorIndex == -1)
            {
                currentColorIndex = 0;
            }
            else if (currentColorIndex == ColorPalette.Count - 1)
            {
                currentColorIndex = -1;
            }
            else
            {
                currentColorIndex = currentColorIndex + 1;
            }
        }
        
        internal static void ResetColorSelection()
        {
            hasColorBeenSelected = false;
            currentColorIndex = -1;
        }

        internal static Color GetCurrentColor()
        {
            if (currentColorIndex == -1)
            {
                return Color.white;
            }
            return ColorPalette[currentColorIndex];
        }

        internal static string GetCurrentColorName()
        {
            return currentColorIndex switch
            {
                -1 => "Default",
                0 => "Rot",
                1 => "Gelb",
                2 => "Grün",
                3 => "Blau",
                4 => "Orange",
                5 => "Lila",
                6 => "Weiß",
                7 => "Schwarz",
                _ => "Unbekannt"
            };
        }

        internal static bool IsPaintball(int itemMasterID)
        {
            return PaintballMasterIDs.Contains(itemMasterID);
        }

        internal static void ApplyColorToPaintball(object inventoryItem)
        {
            if (inventoryItem == null)
            {
                return;
            }

            try
            {
                Transform itemTransform = ReflectionHelper.GetPropertyValue<Transform>(inventoryItem, "Transform");
                if (itemTransform == null)
                {
                    return;
                }

                Renderer[] renderers = itemTransform.GetComponentsInChildren<Renderer>(true);
                if (renderers == null || renderers.Length == 0)
                {
                    return;
                }

                foreach (Renderer renderer in renderers)
                {
                    if (renderer == null || !renderer.enabled)
                    {
                        continue;
                    }

                    Material[] materials = renderer.materials;
                    if (materials == null || materials.Length == 0)
                    {
                        continue;
                    }

                    for (int i = 0; i < materials.Length; i++)
                    {
                        if (materials[i] != null)
                        {
                            string materialKey = $"{renderer.GetInstanceID()}_{i}";
                            Material originalMaterial;
                            
                            if (!originalMaterialsCache.ContainsKey(materialKey))
                            {
                                Material currentMat = materials[i];
                                originalMaterial = new Material(currentMat);
                                
                                if (originalMaterial.HasProperty("_MainTex") && originalMaterial.GetTexture("_MainTex") == Texture2D.whiteTexture)
                                {
                                    originalMaterial.SetTexture("_MainTex", null);
                                }
                                if (originalMaterial.HasProperty("_BaseMap") && originalMaterial.GetTexture("_BaseMap") == Texture2D.whiteTexture)
                                {
                                    originalMaterial.SetTexture("_BaseMap", null);
                                }
                                
                                originalMaterialsCache[materialKey] = originalMaterial;
                            }
                            else
                            {
                                originalMaterial = originalMaterialsCache[materialKey];
                            }
                            
                            if (!hasColorBeenSelected || currentColorIndex == -1)
                            {
                                materials[i] = new Material(originalMaterial);
                                continue;
                            }
                            
                            Material newMaterial = new Material(originalMaterial);
                            Color currentColor = GetCurrentColor();
                            
                            if (newMaterial.HasProperty("_Color"))
                            {
                                if (newMaterial.HasProperty("_MainTex"))
                                {
                                    Texture2D whiteTexture = Texture2D.whiteTexture;
                                    newMaterial.SetTexture("_MainTex", whiteTexture);
                                }
                                if (newMaterial.HasProperty("_BaseMap"))
                                {
                                    Texture2D whiteTexture = Texture2D.whiteTexture;
                                    newMaterial.SetTexture("_BaseMap", whiteTexture);
                                }
                                
                                if (currentColorIndex == 7)
                                {
                                    newMaterial.SetColor("_Color", new Color(0.15f, 0.15f, 0.15f, 1f));
                                }
                                else
                                {
                                    newMaterial.SetColor("_Color", currentColor);
                                }
                            }
                            else if (newMaterial.HasProperty("_BaseColor"))
                            {
                                if (newMaterial.HasProperty("_BaseMap"))
                                {
                                    Texture2D whiteTexture = Texture2D.whiteTexture;
                                    newMaterial.SetTexture("_BaseMap", whiteTexture);
                                }
                                
                                if (currentColorIndex == 7)
                                {
                                    newMaterial.SetColor("_BaseColor", new Color(0.15f, 0.15f, 0.15f, 1f));
                                }
                                else
                                {
                                    newMaterial.SetColor("_BaseColor", currentColor);
                                }
                            }
                            else if (newMaterial.HasProperty("_TintColor"))
                            {
                                if (newMaterial.HasProperty("_MainTex"))
                                {
                                    Texture2D whiteTexture = Texture2D.whiteTexture;
                                    newMaterial.SetTexture("_MainTex", whiteTexture);
                                }
                                
                                if (currentColorIndex == 7)
                                {
                                    newMaterial.SetColor("_TintColor", new Color(0.15f, 0.15f, 0.15f, 1f));
                                }
                                else
                                {
                                    newMaterial.SetColor("_TintColor", currentColor);
                                }
                            }

                            materials[i] = newMaterial;
                        }
                    }

                    renderer.materials = materials;
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error($"[PaintBallColorManager] Error: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}

