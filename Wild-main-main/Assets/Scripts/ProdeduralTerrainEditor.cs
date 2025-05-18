using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ProceduralTerrain))]
public class ProceduralTerrainEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ProceduralTerrain terrain = (ProceduralTerrain)target;
        if (GUILayout.Button("Generate Terrain"))
        {
            terrain.GenerateMesh();
        }
    }
}
