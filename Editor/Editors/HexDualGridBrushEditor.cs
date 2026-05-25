using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;

namespace skner.DualGrid.Editor.Editors
{
    /// <summary>
    /// Custom editor for <see cref="HexDualGridBrush"/>. Prevents direct
    /// painting on the render tilemaps and redirects selection back to the
    /// data tilemap if the user tries.
    /// </summary>
    [CustomEditor(typeof(HexDualGridBrush), false)]
    public class HexDualGridBrushEditor : GridBrushEditor
    {
        public override void OnToolActivated(GridBrushBase.Tool tool)
        {
            ProtectAgainstEditingRenderTilemap();
            base.OnToolActivated(tool);
        }

        private static void ProtectAgainstEditingRenderTilemap()
        {
            var current = Selection.activeObject as GameObject;
            if (current == null) return;

            var parent = current.transform.parent;
            if (parent == null) return;

            var module = parent.GetComponent<HexDualGridTilemapModule>();
            if (module == null) return;

            if (current.name == HexDualGridTilemapModule.UpRenderTilemapName ||
                current.name == HexDualGridTilemapModule.DownRenderTilemapName)
            {
                Debug.LogWarning(
                    $"{current.name} is a render tilemap. Painting redirected to the data tilemap {module.DataTilemap.name}.");
                Selection.activeObject = module.DataTilemap.gameObject;
            }
        }
    }
}
