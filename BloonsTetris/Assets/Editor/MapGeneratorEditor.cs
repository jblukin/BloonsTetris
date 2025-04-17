using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( MapGenerator ), true )]
[CanEditMultipleObjects]
public class MapGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {

        MapGenerator mapGenerator = (MapGenerator)target;

        base.OnInspectorGUI();

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Toggle Cell Labels" ) )
        {

            if ( Application.isPlaying )
                mapGenerator.ToggleDebugText();

        }

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Toggle Cell Highlighting" ) )
        {

            if ( Application.isPlaying )
                mapGenerator.ToggleDebugHighlighting();

        }

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing * 1.5f );

        EditorGUILayout.LabelField( "Spawn Tetrimino Shapes", EditorStyles.boldLabel );

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Spawn Square" ) )
        {

            if ( Application.isPlaying )
                GameManager.Instance.SpawnTetriminoShape( Tetrimino.DefaultShape.Square );

        }

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Spawn L" ) )
        {

            if ( Application.isPlaying )
                GameManager.Instance.SpawnTetriminoShape( Tetrimino.DefaultShape.L );

        }

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Spawn Reverse L" ) )
        {

            if ( Application.isPlaying )
                GameManager.Instance.SpawnTetriminoShape( Tetrimino.DefaultShape.ReverseL );

        }

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Spawn Zigzag" ) )
        {

            if ( Application.isPlaying )
                GameManager.Instance.SpawnTetriminoShape( Tetrimino.DefaultShape.Zigzag );

        }

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Spawn Reverse Zigzag" ) )
        {

            if ( Application.isPlaying )
                GameManager.Instance.SpawnTetriminoShape( Tetrimino.DefaultShape.ReverseZigZag );

        }

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Spawn Line" ) )
        {

            if ( Application.isPlaying )
                GameManager.Instance.SpawnTetriminoShape( Tetrimino.DefaultShape.Line );

        }

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Spawn T" ) )
        {

            if ( Application.isPlaying )
                GameManager.Instance.SpawnTetriminoShape( Tetrimino.DefaultShape.T );

        }

    }
}
