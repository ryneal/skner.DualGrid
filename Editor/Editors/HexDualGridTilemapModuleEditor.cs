using UnityEditor;
using UnityEngine;

namespace skner.DualGrid.Editor.Editors
{
    [CustomEditor(typeof(HexDualGridTilemapModule))]
    public class HexDualGridTilemapModuleEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var module = (HexDualGridTilemapModule)target;

            EditorGUILayout.Space(8);
            if (GUILayout.Button("Refresh Render Tilemaps"))
            {
                module.RefreshRenderTilemap();
            }

            if (module.UpRenderTilemap == null || module.DownRenderTilemap == null)
            {
                EditorGUILayout.HelpBox(
                    $"Missing render tilemaps. Expected children named " +
                    $"'{HexDualGridTilemapModule.UpRenderTilemapName}' and " +
                    $"'{HexDualGridTilemapModule.DownRenderTilemapName}'.",
                    MessageType.Warning);
            }
        }
    }
}
