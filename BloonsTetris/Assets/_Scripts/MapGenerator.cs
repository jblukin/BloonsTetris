using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    [SerializeField] private int _rows, _cols, _cellSize;
    public int Rows { get { return _rows; } }
    public int Cols { get { return _cols; } }
    public int CellSize { get { return _cellSize; } }

    private Cell[,] _grid;
    public Cell[,] Grid { get { return _grid; } }

    private Cell _currentCell;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {



    }

    // Update is called once per frame
    void Update()
    {



    }

    public void HighlightCurrentGridCell( Vector3 mouseWorldPos )
    {

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

        for ( int i = 0; i < rows; i++ )
        {

            for ( int j = 0; j < cols; j++ )
            {

                _grid[ i, j ] = new Cell( i, j, _cellSize, false, CreateWorldText( $"({i}, {j})", null, new Vector3( i + 0.5f, j + 0.5f ) * _cellSize, 160 ) );

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

    public Cell( int x, int y, int size, bool occupied = false, TextMesh textMesh = null )
    {

        X = x;
        Y = y;
        Size = size;

        Occupied = occupied;

        _textMesh = textMesh;

    }

    private TextMesh _textMesh;

    public int X { get; private set; }
    public int Y { get; private set; }
    public int Size { get; private set; }

    public bool Occupied { get; private set; }
    public void SetToOccupied() => Occupied = true;
    public void SetToUnOccupied() => Occupied = false;

    public readonly void SetTextMeshColor( Color color ) => _textMesh.color = color;

    public readonly bool IsInitialized()
    {
        return _textMesh != null;
    }

    public override readonly string ToString()
    {
        return $"({X},{Y})";
    }

}
