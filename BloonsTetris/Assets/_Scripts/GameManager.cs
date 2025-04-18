using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{

    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    private MapGenerator _mapGenerator;
    public MapGenerator MapGenerator { get { return _mapGenerator; } }

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

            _mapGenerator = GetComponent<MapGenerator>();

        }

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        _allTetriminos = new List<GameObject>();

        Actions = new( InputSystem.actions.FindAction( "MouseHover" ), InputSystem.actions.FindAction( "MouseDrag" ), InputSystem.actions.FindAction( "Rotate" ) );

        _mapGenerator.GenerateBasicGrid( _mapGenerator.Rows, _mapGenerator.Cols );

    }

    // Update is called once per frame
    void Update()
    {

        _mouseWorldPosition = Camera.main.ScreenToWorldPoint( Actions.MouseHover.ReadValue<Vector2>() );

        _mapGenerator.HighlightCurrentGridCell( _mouseWorldPosition );

    }

    public void SpawnTetriminoShape( Tetrimino.DefaultShape shape )
    {

        DefaultTetrimino baseShapeData = _defaultTetriminos.Find( x => x.Shape == shape );

        GameObject parentObj = new( $"{shape}" );

        Tetrimino tetrimino = parentObj.AddComponent<Tetrimino>();

        tetrimino.CopyBaseShapeData( baseShapeData );

        parentObj.tag = "Tetrimino";

        parentObj.transform.position = new Vector3( -50f, 100f, 1f );

        parentObj.transform.localScale *= _mapGenerator.CellSize;

        foreach ( Vector2 localPos in baseShapeData.localCellPositions )
        {

            GameObject currCell = Instantiate( _tetriminoBasePrefab, parentObj.transform, false );

            currCell.transform.localPosition = localPos;

        }

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
