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

                HeightmapTerrainSettings settings = (HeightmapTerrainSettings)fieldInfo.GetValue(property.serializedObject.targetObject);
                settings.Heightmap = (Texture2D)EditorGUILayout.ObjectField("Heightmap", settings.Heightmap, typeof(Texture2D), false);

                if (Mathf.ClosestPowerOfTwo(settings.Heightmap.width) != settings.Heightmap.width - 1)
                    EditorGUILayout.HelpBox("The width of the heightmap should be some power of 2 plus 1. Suggested width: " + (Mathf.ClosestPowerOfTwo(settings.Heightmap.width) + 1), MessageType.Warning);
                if (Mathf.ClosestPowerOfTwo(settings.Heightmap.height) != settings.Heightmap.height - 1)
                    EditorGUILayout.HelpBox("The height of the heightmap should be some power of 2 plus 1. Suggested height: " + (Mathf.ClosestPowerOfTwo(settings.Heightmap.height) + 1), MessageType.Warning);

                fieldInfo.SetValue(property.serializedObject.targetObject, settings);

                EditorGUILayout.PropertyField(property.FindPropertyRelative("amplitude"), new GUIContent("Amplitude"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("heightOffset"), new GUIContent("Height Offset"));

                EditorGUI.indentLevel--;
            }
        }
    }
}