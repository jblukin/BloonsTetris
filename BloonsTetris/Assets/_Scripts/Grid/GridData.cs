using UnityEngine;

[CreateAssetMenu( fileName = "GridData", menuName = "Scriptable Objects/GridData" )]
public class GridData : ScriptableObject
{

    public int Rows, Columns, CellSize;

    public Cell[,] Grid;

}
