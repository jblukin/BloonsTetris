using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{

    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    private GridManager _gridManager;
    public GridManager GridManager { get { return _gridManager; } }

    private EnemyManager _enemyManager;
    public EnemyManager EnemyManager { get { return _enemyManager; } }

    private Vector3 _mouseWorldPosition;
    public Vector3 MouseWorldPosition { get { return _mouseWorldPosition; } }


    [SerializeField] private GameObject _tetriminoBasePrefab;
    [SerializeField] private List<DefaultTetrimino> _defaultTetriminos;
    private List<GameObject> _allTetriminos;

    public Inputs Actions { get; private set; }

    private void Awake()
    {

        if ( _instance != null && _instance != this )
        {

            Destroy( this );

        }
        else
        {

            _instance = this;

            _gridManager = GetComponent<GridManager>();

            _enemyManager = GetComponent<EnemyManager>();

        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        _allTetriminos = new List<GameObject>();

        Actions = new( InputSystem.actions.FindAction( "MouseHover" ), InputSystem.actions.FindAction( "MouseDrag" ), InputSystem.actions.FindAction( "Rotate" ) );

        _gridManager.GenerateBasicGrid( _gridManager.Rows, _gridManager.Cols );

    }

    // Update is called once per frame
    void Update()
    {

        _mouseWorldPosition = Camera.main.ScreenToWorldPoint( Actions.MouseHover.ReadValue<Vector2>() );

        _gridManager.HighlightCurrentGridCell( _mouseWorldPosition );

    }

    public void SpawnTetriminoShape( DefaultShape shape )
    {

        DefaultTetrimino baseShapeData = Instantiate(_defaultTetriminos.Find( x => x.Shape == shape ));

        GameObject parentObj = new( $"{shape}" );

        Tetrimino tetrimino = parentObj.AddComponent<Tetrimino>();

        parentObj.transform.position = new Vector3( -50f, 100f, 1f );

        parentObj.transform.localScale *= _gridManager.CellSize;

        foreach ( Vector2 localPos in baseShapeData.localCellPositions )
        {

            GameObject currCell = Instantiate( _tetriminoBasePrefab, parentObj.transform, false );

            currCell.transform.localPosition = localPos;

        }

        tetrimino.Init( baseShapeData );

        _allTetriminos.Add( parentObj );

    }

}

public struct Inputs
{

    public InputAction MouseHover { get; private set; }
    public InputAction MouseDrag { get; private set; }
    public InputAction Rotate { get; private set; }

    public Inputs( InputAction hover, InputAction drag, InputAction rotate )
    {

        MouseHover = hover;
        MouseDrag = drag;
        Rotate = rotate;

    }

}

public enum DefaultShape
{

    Line,
    Square,
    T,
    L,
    ReverseL,
    Zigzag,
    ReverseZigZag

}

[Flags]
public enum ElementalTypes
{
    None = 0,
    Physical = 1,
    Ice = 2,
    Poison = 4,
    Lightning = 8,
    Fire = 16

}

[Flags]
public enum ElementalResistances
{

    None = 0,
    PhysicalLow = 1,
    PhysicalMedium = 2,
    PhysicalHigh = 4,
    PoisonLow = 8,
    PoisonMedium = 16,
    PoisonHigh = 32,
    FireLow = 64,
    FireMedium = 128,
    FireHigh = 256

}


