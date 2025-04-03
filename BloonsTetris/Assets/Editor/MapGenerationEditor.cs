using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGenerationEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        MapGenerator mapGenerator = (MapGenerator)target;

        if(GUILayout.Button("Generate Grid"))
        {

            mapGenerator.GenerateBasicGrid( mapGenerator.Rows, mapGenerator.Cols );

        }

    }

}
