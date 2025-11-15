using System.Collections.Generic;
using UnityEngine;
using MelonLoader;

namespace MarkerMod.Services
{
    internal static class MaterialColorService
    {
        private static readonly Dictionary<string, Material> originalMaterialsCache = new Dictionary<string, Material>();

        internal static void ApplyColorToRenderer(Renderer renderer, Color color, bool useDefaultMaterial)
        {
            if (renderer == null)
            {
                return;
            }

            Material[] materials = renderer.materials;
            if (materials == null || materials.Length == 0)
            {
                return;
            }

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    string materialKey = $"{renderer.GetInstanceID()}_{i}";
                    Material originalMaterial = GetOrCacheOriginalMaterial(materialKey, materials[i]);

                    if (useDefaultMaterial)
                    {
                        materials[i] = new Material(originalMaterial);
                        continue;
                    }

                    Material newMaterial = new Material(originalMaterial);
                    ApplyColorToMaterial(newMaterial, color);
                    materials[i] = newMaterial;
                }
            }

            renderer.materials = materials;
        }

        internal static void ApplyColorToTransform(Transform transform, Color color, bool useDefaultMaterial)
        {
            if (transform == null)
            {
                return;
            }

            Renderer[] renderers = transform.GetComponentsInChildren<Renderer>(true);
            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            foreach (Renderer renderer in renderers)
            {
                ApplyColorToRenderer(renderer, color, useDefaultMaterial);
            }
        }

        private static Material GetOrCacheOriginalMaterial(string materialKey, Material currentMaterial)
        {
            if (!originalMaterialsCache.ContainsKey(materialKey))
            {
                Material originalMaterial = new Material(currentMaterial);

                if (originalMaterial.HasProperty("_MainTex") && originalMaterial.GetTexture("_MainTex") == Texture2D.whiteTexture)
                {
                    originalMaterial.SetTexture("_MainTex", null);
                }
                if (originalMaterial.HasProperty("_BaseMap") && originalMaterial.GetTexture("_BaseMap") == Texture2D.whiteTexture)
                {
                    originalMaterial.SetTexture("_BaseMap", null);
                }

                originalMaterialsCache[materialKey] = originalMaterial;
                return originalMaterial;
            }

            return originalMaterialsCache[materialKey];
        }

        private static void ApplyColorToMaterial(Material material, Color color)
        {
            if (material.HasProperty("_Color"))
            {
                ReplaceTextureWithWhite(material);
                if (IsBlackColor(color))
                {
                    material.SetColor("_Color", new Color(0.15f, 0.15f, 0.15f, 1f));
                }
                else
                {
                    material.SetColor("_Color", color);
                }
            }
            else if (material.HasProperty("_BaseColor"))
            {
                ReplaceTextureWithWhite(material);
                if (IsBlackColor(color))
                {
                    material.SetColor("_BaseColor", new Color(0.15f, 0.15f, 0.15f, 1f));
                }
                else
                {
                    material.SetColor("_BaseColor", color);
                }
            }
            else if (material.HasProperty("_TintColor"))
            {
                ReplaceTextureWithWhite(material);
                if (IsBlackColor(color))
                {
                    material.SetColor("_TintColor", new Color(0.15f, 0.15f, 0.15f, 1f));
                }
                else
                {
                    material.SetColor("_TintColor", color);
                }
            }
        }

        private static void ReplaceTextureWithWhite(Material material)
        {
            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", Texture2D.whiteTexture);
            }
            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", Texture2D.whiteTexture);
            }
        }

        private static bool IsBlackColor(Color color)
        {
            return color.r < 0.1f && color.g < 0.1f && color.b < 0.1f;
        }
    }
}

