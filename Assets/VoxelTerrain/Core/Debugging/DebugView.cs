using Eldemarkki.VoxelTerrain.World;
using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class DebugView : MonoBehaviour
{
    [Header("Voxel World")]
    [SerializeField] private VoxelWorld voxelWorld;

    [Header("Key bindings")]
    [SerializeField] private KeyCode debugViewToggleKey = KeyCode.G;

    [Header("UI")]
    [SerializeField] private GameObject backgroundPanel;
    [SerializeField] private Text debugViewText;

    private bool Enabled { get; set; }

    private class DebugProperty
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

    private DebugProperty[] GetProperties(VoxelWorld voxelWorld)
    {
        return new DebugProperty[]
        {
            new DebugProperty("GameObject count", () => FindObjectsOfType<GameObject>().Length),
            new DebugProperty("ChunkStore count", voxelWorld.ChunkStore.Chunks.Count),
            new DebugProperty("VoxelDataStore count", voxelWorld.VoxelDataStore.Chunks.Count),
            new DebugProperty("VoxelColorStore count", voxelWorld.VoxelColorStore.Chunks.Count)
        };
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
    }

    private void OnGUI()
    {
        if (Enabled)
        {
            StringBuilder debugText = new StringBuilder();
            foreach (var property in GetProperties(voxelWorld))
            {
                debugText.AppendLine(property.ToString());
            }

            if (debugViewText)
            {
                debugViewText.text = debugText.ToString();
            }
        }
    }
}
