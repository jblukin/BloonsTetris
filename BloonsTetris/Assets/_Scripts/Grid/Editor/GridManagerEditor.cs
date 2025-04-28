using UnityEditor;
using UnityEngine;

[CustomEditor( typeof( GridManager ), true ), CanEditMultipleObjects]
public class GridManagerEditor : Editor
{

    public override void OnInspectorGUI()
    {

        GridManager mapGenerator = (GridManager)target;

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
                GameManager.Instance.SpawnTetriminoShape( DefaultShape.Square );

        }

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Spawn L" ) )
        {

            if ( Application.isPlaying )
                GameManager.Instance.SpawnTetriminoShape( DefaultShape.L );

        }

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Spawn Reverse L" ) )
        {

            if ( Application.isPlaying )
                GameManager.Instance.SpawnTetriminoShape( DefaultShape.ReverseL );

        }

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Spawn Zigzag" ) )
        {

            if ( Application.isPlaying )
                GameManager.Instance.SpawnTetriminoShape( DefaultShape.Zigzag );

        }

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Spawn Reverse Zigzag" ) )
        {

            if ( Application.isPlaying )
                GameManager.Instance.SpawnTetriminoShape( DefaultShape.ReverseZigZag );

        }

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Spawn Line" ) )
        {

            if ( Application.isPlaying )
                GameManager.Instance.SpawnTetriminoShape( DefaultShape.Line );

        }

        GUILayout.Space( EditorGUIUtility.standardVerticalSpacing );

        if ( GUILayout.Button( "Spawn T" ) )
        {

            if ( Application.isPlaying )
                GameManager.Instance.SpawnTetriminoShape( DefaultShape.T );

        }

    }
}
