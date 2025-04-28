using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu( fileName = "DefaultTetrimino", menuName = "Scriptable Objects/DefaultTetrimino" )]
public class DefaultTetrimino : ScriptableObject
{

    public float Power, Cooldown, Range;

    public DefaultShape Shape;

    public ElementalTypes ElementalTypes;

    public List<Vector2> localCellPositions;

}
