using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "GridData", menuName = "Scriptable Objects/GridData" ), Serializable]
public class GridData : ScriptableObject
{

    [SerializeField]
    private int _rows, _columns, _cellSize;

    [SerializeField]
    private Cell[] _grid;

    private List<Vector2> _enemyWaypoints;

    public int Rows => _rows;
    public int Columns => _columns;
    public int CellSize => _cellSize;
    public Cell[] Grid => _grid;

    [ExecuteAlways]
    public void CreateGrid( int rows, int cols, int cellSize )
    {

        _rows = rows;
        _columns = cols;
        _cellSize = cellSize;

        _grid = new Cell[ rows * cols ];

        _enemyWaypoints = new();

        for ( int x = 0; x < rows; x++ )
        {

            for ( int y = 0; y < cols; y++ )
            {

                _grid[ x + y * _columns ] = new Cell( x, y, cellSize );

            }

        }

    }

}
