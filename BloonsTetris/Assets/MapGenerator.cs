using UnityEngine;

[ExecuteInEditMode]
public class MapGenerator : MonoBehaviour
{

    [SerializeField] private int _rows, _cols, _cellSize;
    public int Rows { get { return _rows; } }
    public int Cols { get { return _cols; } }

    public int CellSize { get { return _cellSize; } }

    Cell[,] _grid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GenerateBasicGrid( Rows, Cols );
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void GenerateBasicGrid( int rows, int cols )
    {

        _grid = new Cell[ rows, cols ];

        int halfRows = rows / 2, halfCols = cols / 2;

        for ( int i = 0; i < rows; i++ )
        {

            for ( int j = 0; j < cols; j++ )
            {

                _grid[ i, j ] = new Cell( i - halfRows, j - halfCols, CellSize, false );

            }

        }

        foreach ( Cell c in _grid )
        {

            CreateWorldText( c.ToString(), null, new Vector3(c.X, c.Y) * CellSize);

        }

    }

    public static TextMesh CreateWorldText( string text, Transform parent = null, Vector3 localPos = default, int fontSize = 30, Color color = default, TextAnchor anchor = TextAnchor.MiddleCenter, int sortingOrder = -1 )
    {

        if(color == default) color = Color.white;

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

    public Cell( int x, int y, int size, bool occupied )
    {

        X = x;
        Y = y;
        Size = size;

        Occupied = occupied;

    }

    public int X { get; }
    public int Y { get; }
    public int Size { get; }

    public bool Occupied { get; }

    public override string ToString()
    {
        return $"({X},{Y})";
    }

}
