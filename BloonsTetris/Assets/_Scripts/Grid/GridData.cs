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
    public List<Vector2> EnemyWaypoints => _enemyWaypoints;

    [ExecuteAlways]
    public void CreateGrid( int rows, int columns, int cellSize = 45 )
    {

        _rows = rows;
        _columns = columns;
        _cellSize = cellSize;

        _grid = new Cell[ rows * columns ];

        _enemyWaypoints = new();

        for ( int r = 0; r < rows; r++ )
        {

            for ( int c = 0; c < columns; c++ )
            {

                _grid[ c + r * columns ] = new Cell( c, r, cellSize );

            }

        }

    }

}
