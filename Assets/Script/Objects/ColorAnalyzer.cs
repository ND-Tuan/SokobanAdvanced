using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class ColorAnalyzer
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Color32Array
    {
        [FieldOffset(0)] public int key;
        [FieldOffset(0)] public Color32 color;
    }

    public static Color32 MainColorFromTexture(Texture2D tex)
    {
        if (tex == null) return new Color32(0, 0, 0, 255);

        Color32[] texColors = tex.GetPixels32();
        int total = texColors.Length;

        Dictionary<int, int> colors = new Dictionary<int, int>();
        int max = 1;
        Color32 mostCol = texColors[0];

        for (int i = 0; i < total; i++)
        {
            Color32Array c = new Color32Array { color = texColors[i] };

            if (texColors[i].a < 10) continue; // Bỏ qua pixel trong suốt

            if (colors.ContainsKey(c.key))
            {
                int count = ++colors[c.key];
                if (count > max)
                {
                    max = count;
                    mostCol = c.color;
                }
            }
            else
            {
                colors.Add(c.key, 1);
            }
        }

        return mostCol;
    }
}
