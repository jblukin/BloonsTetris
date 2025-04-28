using System;
using UnityEngine;

[CreateAssetMenu( fileName = "GridData", menuName = "Scriptable Objects/GridData" ), Serializable]
public class GridData : ScriptableObject
{

    [SerializeField]
    private int _rows, _columns, _cellSize;

    [SerializeField]
    private Cell[,] _grid;

    public int Rows => _rows; 
    public int Columns => _columns;
    public int CellSize => _cellSize;
    public Cell[,] Grid => _grid;

    public void CreateGrid( int rows, int cols, int cellSize )
    {

        _rows = rows;
        _columns = cols;
        _cellSize = cellSize;

        _grid = new Cell[ rows, cols ];

        for ( int i = 0; i < rows; i++ )
        {
            for ( int j = 0; j < cols; j++ )
            {

                Grid[ i, j ] = new Cell( i, j, cellSize );

            }
        }

    }

}
