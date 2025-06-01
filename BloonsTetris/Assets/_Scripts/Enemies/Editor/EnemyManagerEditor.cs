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

        inspector.Add( new Button( () => { ( target as EnemyManager ).SpawnEnemy( typeof( BaseEnemy ) ); } ) { text = "Spawn Base Enemy", style = { flexGrow = 1, flexShrink = 1, minHeight = 15, marginTop = 10 } } );

        inspector.Add( new Button( () => { ( target as EnemyManager ).SpawnEnemy( typeof( EnchanterEnemy ) ); } ) { text = "Spawn Enchanter Enemy", style = { flexGrow = 1, flexShrink = 1, minHeight = 15, marginTop = 5 } } );

        inspector.Add( new Button( () => { ( target as EnemyManager ).SpawnEnemy( typeof( DisablerEnemy ) ); } ) { text = "Spawn Disabler Enemy", style = { flexGrow = 1, flexShrink = 1, minHeight = 15, marginTop = 5 } } );
        
        inspector.Add( new Button( () => { ( target as EnemyManager ).SpawnEnemy( typeof( RunnerEnemy ) ); } ) { text = "Spawn Runner Enemy", style = { flexGrow = 1, flexShrink = 1, minHeight = 15, marginTop = 5 } } );
        
        inspector.Add( new Button( () => { ( target as EnemyManager ).SpawnEnemy( typeof( BomberEnemy ) ); } ) { text = "Spawn Bomber Enemy", style = { flexGrow = 1, flexShrink = 1, minHeight = 15, marginTop = 5 } } );

        return inspector;

    }

}
