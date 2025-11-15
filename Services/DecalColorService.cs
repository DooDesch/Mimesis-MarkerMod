using MarkerMod.Managers;
using MimicAPI.GameAPI;
using UnityEngine;

namespace MarkerMod.Services
{
    internal static class DecalColorService
    {
        private static readonly string[] ColorIds = new[]
        {
            "red", "yellow", "green", "blue", "orange", "purple", "white", "black"
        };

        internal static string GetDecalColorIdForCurrentColor()
        {
            int colorIndex = PaintBallColorManager.GetCurrentColorIndex();
            if (colorIndex == -1 || !PaintBallColorManager.HasColorBeenSelected())
            {
                return string.Empty;
            }

            if (colorIndex >= 0 && colorIndex < ColorIds.Length)
            {
                return ColorIds[colorIndex];
            }

            return string.Empty;
        }

        internal static string GetDecalColorIdForColor(Color color)
        {
            if (color.r > 0.9f && color.g < 0.1f && color.b < 0.1f)
                return "red";
            if (color.r > 0.9f && color.g > 0.9f && color.b < 0.1f)
                return "yellow";
            if (color.r < 0.1f && color.g > 0.9f && color.b < 0.1f)
                return "green";
            if (color.r < 0.1f && color.g < 0.1f && color.b > 0.9f)
                return "blue";
            if (color.r > 0.9f && color.g > 0.4f && color.g < 0.6f && color.b < 0.1f)
                return "orange";
            if (color.r > 0.4f && color.r < 0.6f && color.g < 0.1f && color.b > 0.9f)
                return "purple";
            if (color.r > 0.9f && color.g > 0.9f && color.b > 0.9f)
                return "white";
            if (color.r < 0.1f && color.g < 0.1f && color.b < 0.1f)
                return "black";

            return string.Empty;
        }
    }
}

