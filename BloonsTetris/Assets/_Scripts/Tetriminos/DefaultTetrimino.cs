using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "DefaultTetrimino", menuName = "Scriptable Objects/DefaultTetrimino" )]
public class DefaultTetrimino : ScriptableObject
{

    public float Power, Cooldown, Range;

    public Tetrimino.DefaultShape Shape;

    public List<Vector2> localCellPositions;

}
