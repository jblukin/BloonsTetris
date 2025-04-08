using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator), true)] 
[CanEditMultipleObjects]
public class MapGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {

        MapGenerator mapGenerator = (MapGenerator)target;

        base.OnInspectorGUI();

        GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

        if(GUILayout.Button("Toggle Cell Highlighting/Labels"))
        {

            if(Application.isPlaying) mapGenerator.ToggleDebugFeatures();

        }

    }
}
