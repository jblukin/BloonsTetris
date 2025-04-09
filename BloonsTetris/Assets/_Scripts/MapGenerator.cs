using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class MapGenerator : MonoBehaviour
{

    [SerializeField]
    private int _rows, _cols, _cellSize;
    public int Rows { get { return _rows; } }
    public int Cols { get { return _cols; } }
    public int CellSize { get { return _cellSize; } }

    private Cell[,] _grid;
    public Cell[,] Grid { get { return _grid; } }

    private Cell _currentCell;

    public void HighlightCurrentGridCell( Vector3 mouseWorldPos )
    {

        if ( !_debugOn )
            return;

        int mouseX = (int)Math.Floor( mouseWorldPos.x ) / _cellSize, mouseY = (int)Math.Floor( mouseWorldPos.y ) / _cellSize;

        if ( mouseX < 0 || mouseX >= _rows || mouseY < 0 || mouseY >= _cols )
        {

            if ( _currentCell.IsInitialized() )
                _currentCell.SetTextMeshColor( Color.white );

            _currentCell = default;

            return;

        }

        if ( _currentCell.IsInitialized() && ( _currentCell.X != mouseX || _currentCell.Y != mouseY ) )
            _currentCell.SetTextMeshColor( Color.white );

        _currentCell = _grid[ mouseX, mouseY ];

        _currentCell.SetTextMeshColor( Color.red );

    }

    public void GenerateBasicGrid( int rows, int cols )
    {

        _grid = new Cell[ rows, cols ];

        int count = 0;

        for ( int i = 0; i < rows; i++ )
        {

            for ( int j = 0; j < cols; j++ )
            {

                _grid[ i, j ] = new Cell( i, j, _cellSize, Cell.CellStatus.Unoccupied, CreateWorldText( $"{count++}", null, new Vector3( i + 0.5f, j + 0.5f ) * _cellSize, 200 ) );

            }

        }

        for ( int i = 0; i <= _rows; i++ )
        {

            Debug.DrawLine( new Vector3( 0, i, 10 ) * _cellSize, new Vector3( _cols, i, 10 ) * _cellSize, Color.white, 1000 );

        }

        for ( int i = 0; i <= _cols; i++ )
        {

            Debug.DrawLine( new Vector3( i, 0, 10 ) * _cellSize, new Vector3( i, _rows, 10 ) * _cellSize, Color.white, 1000 );

        }

    }

    public bool TryPlaceTetrimino( out Cell newCell )
    {

        int mouseX = (int)Math.Floor( GameManager.Instance.MouseWorldPosition.x ) / _cellSize,
            mouseY = (int)Math.Floor( GameManager.Instance.MouseWorldPosition.y ) / _cellSize;

        if ( mouseX < 0 || mouseX >= _rows || mouseY < 0 || mouseY >= _cols || _currentCell.Status == Cell.CellStatus.Occupied )
        {

            newCell = default;

            return false;

        }

        newCell = _grid[ mouseX, mouseY ];

        return true;

    }

    public bool TryPlaceTetriminoShape( out List<Cell> newCells )
    {

        newCells = new();

        return false;

    }

    [SerializeField]
    private bool _debugOn = true;

    public void ToggleDebugFeatures()
    {

        _debugOn = !_debugOn;

        foreach ( Cell c in _grid )
        {

            c.SetTextMeshColor( _debugOn ? Color.white : Color.clear );

        }

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

public struct Cell
{

    public enum CellStatus
    {

        Unoccupied,
        Occupied,
        Path

    }

    public Cell( int x, int y, int size, CellStatus cellStatus = CellStatus.Unoccupied, TextMesh textMesh = null )
    {

        X = x;
        Y = y;
        Size = size;

        Status = cellStatus;

        _textMesh = textMesh;

    }

    private TextMesh _textMesh;

    public int X { get; private set; }
    public int Y { get; private set; }
    public int Size { get; private set; }

    public CellStatus Status { get; private set; }
    public void SetToOccupied() => Status = CellStatus.Occupied;
    public void SetToUnOccupied() => Status = CellStatus.Unoccupied;
    public void SetToPath() => Status = CellStatus.Path;

    public readonly void SetTextMeshColor( Color color ) => _textMesh.color = color;

    public readonly Vector3 CellCenterToWorldSpace()
    {

        return new Vector3( X + 0.5f, Y + 0.5f, 0 ) * Size;

    }

    public readonly bool IsInitialized()
    {
        return _textMesh != null;
    }

    public override readonly string ToString()
    {
        return $"({X},{Y})";
    }

}
