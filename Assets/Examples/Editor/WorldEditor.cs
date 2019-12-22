using UnityEngine;
using UnityEditor;

namespace MarchingCubes.Examples
{
    [CustomEditor(typeof(World))]
    public class WorldEditor : Editor
    {
        private bool proceduralVisible = true;

        private SerializedProperty chunkSize;
        private SerializedProperty chunkPrefab;
        private SerializedProperty isolevel;
        private SerializedProperty terrainType;
        private SerializedProperty proceduralTerrainSettings;
        private SerializedProperty heightmapTerrainSettings;
        private SerializedProperty renderDistance;
        private SerializedProperty player;

        private void OnEnable()
        {
            chunkSize = serializedObject.FindProperty("chunkSize");
            chunkPrefab = serializedObject.FindProperty("chunkPrefab");
            isolevel = serializedObject.FindProperty("isolevel");
            terrainType = serializedObject.FindProperty("terrainType");
            proceduralTerrainSettings = serializedObject.FindProperty("proceduralTerrainSettings");
            heightmapTerrainSettings = serializedObject.FindProperty("heightmapTerrainSettings");
            renderDistance = serializedObject.FindProperty("renderDistance");
            player = serializedObject.FindProperty("player");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.LabelField("Chunk Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(chunkSize, new GUIContent("Chunk Size"));
            EditorGUILayout.PropertyField(chunkPrefab, new GUIContent("Chunk Prefab"));

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Marching Cubes Settings", EditorStyles.boldLabel);
            EditorGUILayout.Slider(isolevel, -1, 1, new GUIContent("Isolevel"));

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Terrain Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(terrainType, new GUIContent("Terrain Type"));

            TerrainType type = (TerrainType)terrainType.enumValueIndex;
            switch(type)
            {
                 case TerrainType.Procedural:
                    EditorGUILayout.PropertyField(proceduralTerrainSettings, true);
                    break;
                case TerrainType.Heightmap:
                    EditorGUILayout.PropertyField(heightmapTerrainSettings, true);
                    break;
                default:
                    EditorGUILayout.HelpBox($"No implementation for TerrainType.{type}!", MessageType.Error);
                    break;
            }

            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Player Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(renderDistance, new GUIContent("Render Distance"));
            EditorGUILayout.PropertyField(player, new GUIContent("Player"));

            serializedObject.ApplyModifiedProperties();
        }
    }
}