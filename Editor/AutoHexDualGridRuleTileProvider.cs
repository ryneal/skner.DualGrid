using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace skner.DualGrid.Editor
{
    /// <summary>
    /// Populates a <see cref="HexDualGridRuleTile"/>'s 16 sprite slots from a
    /// texture sliced into exactly 16 sprites. The canonical sprite order is:
    /// up-triangle masks 0..7 (8 sprites), then down-triangle masks 0..7 (8
    /// sprites), reading top-to-bottom then left-to-right by sprite rect.
    /// </summary>
    public static class AutoHexDualGridRuleTileProvider
    {
        public static bool ApplyAutoRules(HexDualGridRuleTile tile, Texture2D texture)
        {
            if (tile == null || texture == null) return false;

            string path = AssetDatabase.GetAssetPath(texture);
            var sprites = new List<Sprite>();
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is Sprite s) sprites.Add(s);
            }

            if (sprites.Count != 16)
            {
                Debug.LogError(
                    $"AutoHexDualGridRuleTileProvider expected 16 sliced sprites, found {sprites.Count} in {path}.",
                    tile);
                return false;
            }

            sprites.Sort((a, b) =>
            {
                int cmpY = b.rect.y.CompareTo(a.rect.y);
                return cmpY != 0 ? cmpY : a.rect.x.CompareTo(b.rect.x);
            });

            for (int i = 0; i < 8; i++) tile.UpTriangleSprites[i] = sprites[i];
            for (int i = 0; i < 8; i++) tile.DownTriangleSprites[i] = sprites[8 + i];

            EditorUtility.SetDirty(tile);
            AssetDatabase.SaveAssets();
            return true;
        }
    }
}
