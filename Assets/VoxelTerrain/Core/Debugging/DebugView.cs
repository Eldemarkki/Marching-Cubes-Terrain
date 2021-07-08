#if DEVELOPMENT_BUILD || UNITY_EDITOR
#define DEBUG
#endif

#if DEBUG

using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Eldemarkki.VoxelTerrain.Debugging
{
    public class DebugProperty
    {
        public string Label { get; private set; }
        public Func<string> GetValue { get; private set; }

        public DebugProperty(string label, Func<string> getValue)
        {
            Label = label;
            GetValue = getValue;
        }

        public DebugProperty(string label, Func<int> getValue) : this(label, () => getValue().ToString()) { }

        public override string ToString()
        {
            return Label + ": " + GetValue();
        }
    }

    public class DebugView : MonoBehaviour
    {
        private static List<DebugProperty> DebugProperties { get; set; } = new List<DebugProperty>();
        public static void AddDebugProperty(string label, Func<int> getValue)
        {
            DebugProperties.Add(new DebugProperty(label, getValue));
        }

        [Header("Voxel World")]
        [SerializeField] private VoxelWorld voxelWorld;

        [Header("Key bindings")]
        [SerializeField] private KeyCode debugViewToggleKey = KeyCode.G;

        [Header("UI")]
        [SerializeField] private GameObject backgroundPanel;
        [SerializeField] private Text debugViewText;

        private bool Enabled { get; set; }

        private void Start()
        {
            DebugProperties.Add(new DebugProperty("GameObject count", () => FindObjectsOfType<GameObject>().Length));
            DebugProperties.Add(new DebugProperty("Player coordinate", () => VectorUtilities.WorldPositionToCoordinate(voxelWorld.Player.position, voxelWorld.WorldSettings.ChunkSize).ToString()));
        }

        private void OnApplicationQuit()
        {
            // We have to clear the debug properties list because "Reload Domain" is 
            // disabled in the Player Settings to make loading faster
            DebugProperties.Clear();
        }

        void Update()
        {
            if (Input.GetKeyDown(debugViewToggleKey))
            {
                Enabled = !Enabled;
                if (backgroundPanel)
                {
                    backgroundPanel.SetActive(Enabled);
                }
            }

            if (Enabled && debugViewText)
            {
                StringBuilder debugText = new StringBuilder();
                for (int i = 0; i < DebugProperties.Count; i++)
                {
                    debugText.AppendLine(DebugProperties[i].ToString());
                }

                debugViewText.text = debugText.ToString();
            }
        }
    }
}
#endif