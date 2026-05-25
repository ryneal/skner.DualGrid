using UnityEditor;
using UnityEngine;

namespace skner.DualGrid.Editor.Editors
{
    [CustomEditor(typeof(HexDualGridRuleTile))]
    public class HexDualGridRuleTileEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Up Triangles", EditorStyles.boldLabel);
            DrawVariantRow(
                serializedObject.FindProperty("_upTriangleSprites"),
                serializedObject.FindProperty("_upColliderTypes"),
                serializedObject.FindProperty("_upGameObjects"));

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Down Triangles", EditorStyles.boldLabel);
            DrawVariantRow(
                serializedObject.FindProperty("_downTriangleSprites"),
                serializedObject.FindProperty("_downColliderTypes"),
                serializedObject.FindProperty("_downGameObjects"));

            serializedObject.ApplyModifiedProperties();
        }

        private static void DrawVariantRow(
            SerializedProperty sprites,
            SerializedProperty colliders,
            SerializedProperty gos)
        {
            for (int i = 0; i < 8; i++)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(
                    $"Mask {i} ({System.Convert.ToString(i, 2).PadLeft(3, '0')})",
                    GUILayout.Width(110));
                EditorGUILayout.PropertyField(
                    sprites.GetArrayElementAtIndex(i), GUIContent.none, GUILayout.Width(160));
                EditorGUILayout.PropertyField(
                    colliders.GetArrayElementAtIndex(i), GUIContent.none, GUILayout.Width(120));
                EditorGUILayout.PropertyField(
                    gos.GetArrayElementAtIndex(i), GUIContent.none);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}
