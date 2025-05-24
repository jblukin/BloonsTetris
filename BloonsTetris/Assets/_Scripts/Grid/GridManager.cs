using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class GridManager : MonoBehaviour
{

    [SerializeField]
    private bool _debugText, _debugHighlight;

    [SerializeField]
    private int _rows, _columns, _cellSize;
    public int Rows { get { return _rows; } }
    public int Columns { get { return _columns; } }
    public int CellSize { get { return _cellSize; } }

    [Obsolete]
    private Cell[,] _grid;
    [Obsolete]
    public Cell[,] Grid { get { return _grid; } }

    [SerializeField]
    private List<GridData> _premadeGridData;

    private Cell[] _editorGrid;

    private List<Vector2> _enemyPathWaypoints;
    public List<Vector2> EnemyPathWaypoints { get { return _enemyPathWaypoints; } }

    [SerializeField]
    private Sprite _pathSprite;

    #region Debug
#if UNITY_EDITOR
    private Cell _currentCell;

    public void HighlightCurrentGridCell( Vector3 mouseWorldPos )
    {

        if ( !_debugHighlight )
            return;

        int mouseX = (int)Math.Floor( mouseWorldPos.x ) / _cellSize, mouseY = (int)Math.Floor( mouseWorldPos.y ) / _cellSize;

        if ( mouseX < 0 || mouseX >= _rows || mouseY < 0 || mouseY >= _columns )
        {

            if ( _currentCell.IsInitialized() )
                _currentCell.SetTextMeshColor( Color.white );

            _currentCell = default;

            return;

        }

        if ( _currentCell.IsInitialized() && ( _currentCell.X != mouseX || _currentCell.Y != mouseY ) )
            _currentCell.SetTextMeshColor( Color.white );

        _currentCell = GetGridCell( mouseX, mouseY );

        _currentCell.SetTextMeshColor( Color.red );

    }
#endif
    #endregion

    public Cell GetGridCell( int x, int y )
    {

        return _editorGrid[ x + y * _columns ];

    }

    public void InitializeGrid( int rows = 15, int columns = 15 )
    {

        GridData gridData = _premadeGridData.Count > 0 ? _premadeGridData[ Random.Range( 0, _premadeGridData.Count ) ] : GenerateNewGrid( rows, columns );

        _editorGrid = gridData.Grid;
        _rows = gridData.Rows;
        _columns = gridData.Columns;
        _cellSize = gridData.CellSize;
        _enemyPathWaypoints = ConvertPathToWorldSpace( gridData.EnemyWaypoints );

        DrawGrid();

    }

    private List<Vector2> ConvertPathToWorldSpace( List<Vector2Int> waypointCells )
    {

        List<Vector2> worldSpaceWaypoints = new();

        foreach ( var waypointCell in waypointCells )
        {

            worldSpaceWaypoints.Add( new Vector2( waypointCell.x + 0.5f, waypointCell.y + 0.5f ) * _cellSize );

        }

        return worldSpaceWaypoints;

    }

    private void DrawGrid()
    {

        foreach( var cell in _editorGrid )
        {

            if ( cell.State is not Cell.CellState.Path )
                continue;

            var pathSprite = new GameObject("Pathing");

            pathSprite.transform.localScale *= _cellSize;

            pathSprite.transform.position = cell.CellCenterToWorldSpace();

            pathSprite.AddComponent<SpriteRenderer>();

            pathSprite.GetComponent<SpriteRenderer>().sprite = _pathSprite;

        }


        for ( int i = 0; i <= _rows; i++ )
        {

            Debug.DrawLine( new Vector3( 0, i, 10 ) * _cellSize, new Vector3( _columns, i, 10 ) * _cellSize, Color.white, 1000 );

        }

        for ( int i = 0; i <= _columns; i++ )
        {

            Debug.DrawLine( new Vector3( i, 0, 10 ) * _cellSize, new Vector3( i, _rows, 10 ) * _cellSize, Color.white, 1000 );

        }

    }

    private GridData GenerateNewGrid( int rows, int columns )
    {

        Debug.LogWarning( "Enemy Pathing Not Implemented For This Method Yet!" );

        GridData gridData = Instantiate( ScriptableObject.CreateInstance<GridData>() );

        gridData.CreateGrid( rows, columns );

        return gridData;


    }

    [Obsolete]
    public void GenerateBasicGrid( int rows, int cols )
    {

        _grid = new Cell[ rows, cols ];

        int count = 0;

        for ( int i = 0; i < rows; i++ )
        {

            for ( int j = 0; j < cols; j++ )
            {

                _grid[ i, j ] = new Cell( i, j, _cellSize, Cell.CellState.Empty, CreateWorldText( $"{count++}", GameManager.Instance.transform, new Vector3( i + 0.5f, j + 0.5f ) * _cellSize, 200 ) );

                if ( !_debugText )
                    _grid[ i, j ].SetTextMeshColor( Color.clear );

            }

        }

        for ( int i = 0; i <= _rows; i++ )
        {

            Debug.DrawLine( new Vector3( 0, i, 10 ) * _cellSize, new Vector3( _columns, i, 10 ) * _cellSize, Color.white, 1000 );

        }

        for ( int i = 0; i <= _columns; i++ )
        {

            Debug.DrawLine( new Vector3( i, 0, 10 ) * _cellSize, new Vector3( i, _rows, 10 ) * _cellSize, Color.white, 1000 );

        }

    }

    public bool TryPlaceTetriminoShape( List<Vector2> shapeLocalPositions, out List<Cell> newCells, out Vector3 newPosition )
    {

        int mouseX = (int)Math.Floor( GameManager.Instance.MouseWorldPosition.x ) / _cellSize,
            mouseY = (int)Math.Floor( GameManager.Instance.MouseWorldPosition.y ) / _cellSize;

        newPosition = default;

        newCells = new();

        foreach ( Vector2 tetriminoCell in shapeLocalPositions )
        {

            int currCellX = mouseX + (int)Math.Floor( tetriminoCell.x ), currCellY = mouseY + (int)Math.Floor( tetriminoCell.y );

            if ( currCellX < 0 || currCellY >= _rows || currCellY < 0 || currCellX >= _columns ||
                    GetGridCell( currCellX, currCellY ).State != Cell.CellState.Empty )
                return false;

            newCells.Add( GetGridCell( currCellX, currCellY ) );

        }

        newPosition = GetGridCell( mouseX, mouseY ).CellCenterToWorldSpace();

        return true;

    }

    public void ToggleDebugText()
    {

        _debugHighlight = _debugText = !_debugText;

        foreach ( Cell c in _editorGrid )
        {

            c.SetTextMeshColor( _debugText ? Color.white : Color.clear );

        }

    }

    public void ToggleDebugHighlighting()
    {

        if ( !_debugText )
            return;

        _debugHighlight = !_debugHighlight;

    }

    public static TextMesh CreateWorldText( string text, Transform parent = null, Vector3 localPos = default, int fontSize = 40, Color color = default, TextAnchor anchor = TextAnchor.MiddleCenter, int sortingOrder = 1 )
    {

        if ( color == default )
            color = Color.white;

        GameObject go = new( "World_Text", typeof( TextMesh ) );
        Transform t = go.transform;
        t.SetParent( parent, false );
        t.localPosition = localPos;
        TextMesh tM = go.GetComponent<TextMesh>();
        tM.anchor = anchor;
        tM.text = text;
        tM.fontSize = fontSize;
        tM.color = color;
        tM.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

        return tM;

    }

}

[Serializable]
public struct Cell
{

    public enum CellState
    {

        Empty,
        Occupied,
        Path

    }

    public Cell( int x, int y, int size, CellState cellState = CellState.Empty, TextMesh textMesh = null )
    {

        _x = x;
        _y = y;
        _size = size;

        _cellState = cellState;

        _textMesh = textMesh;

    }

    private readonly TextMesh _textMesh;

    [SerializeField]
    private int _x, _y, _size;

    [SerializeField]
    private CellState _cellState;

    public readonly int X => _x;
    public readonly int Y => _y;
    public readonly int Size => _size;

    public readonly CellState State => _cellState;
    public void SetToOccupied() { _cellState = CellState.Occupied; /*SetTextMeshColor( Color.clear );*/ }
    public void SetToEmpty() { _cellState = CellState.Empty; /*SetTextMeshColor( Color.white );*/ }
    public void SetToPath() { _cellState = CellState.Path; /*SetTextMeshColor( Color.green );*/ }

    public readonly void SetTextMeshColor( Color color ) => _textMesh.color = color;

    public readonly Vector3 CellCenterToWorldSpace()
    {

        return new Vector3( X + 0.5f, Y + 0.5f, 1 / _size ) * _size;

    }

    public readonly bool IsInitialized()
    {
        return _textMesh != null;
    }

    public override readonly string ToString()
    {
        return $"({_x},{_y})";
    }

}
