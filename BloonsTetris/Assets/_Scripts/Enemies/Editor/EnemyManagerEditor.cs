using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor( typeof( EnemyManager ), true ), CanEditMultipleObjects]
public class EnemyManagerEditor : Editor
{

    public override VisualElement CreateInspectorGUI()
    {

        VisualElement inspector = new();

        InspectorElement.FillDefaultInspector( inspector, serializedObject, this );

        inspector.Add( new Button( () => { ( target as EnemyManager ).SpawnEnemy(); } ) { text = "Spawn Base Enemy", style = { flexGrow = 1, flexShrink = 1, minHeight = 15 } } );

        return inspector;

    }

}
