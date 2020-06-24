using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HeightMap))]
public class TerrainEditor : Editor
{
    HeightMap terrain;
    // Start is called before the first frame update
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Generate Terrain"))
        {
            terrain.GenerateTerrain();
        }

        if(GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrainValues();
        }
        GUILayout.EndHorizontal();
    }

    private void OnEnable()
    {
        terrain = (HeightMap)target;
    }
}
