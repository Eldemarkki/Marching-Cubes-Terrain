using UnityEngine;
using UnityEditor;

namespace MarchingCubes.Examples
{
    [CustomPropertyDrawer(typeof(HeightmapTerrainSettings))]
    public class HeightmapDensityInspector : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.ObjectField(property.FindPropertyRelative("heightmap"), typeof(Texture2D), new GUIContent("Heightmap"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("amplitude"), new GUIContent("Amplitude"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("heightOffset"), new GUIContent("Height Offset"));
                EditorGUI.indentLevel--;
            }
        }
    }
}