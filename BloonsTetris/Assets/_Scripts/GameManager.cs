using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class GameManager : MonoBehaviour
{
    public enum DefaultTetriminoShape
    {

        Line,
        Square,
        T,
        L,
        ReverseL,
        Zigzag,
        ReverseZigZag

    }

    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    private MapGenerator _mapGenerator;
    public MapGenerator MapGenerator { get { return _mapGenerator; } }

    private Vector3 _mouseWorldPosition;
    public Vector3 MouseWorldPosition { get { return _mouseWorldPosition; } }


    [SerializeField] private GameObject _tetriminoPrefab;
    private List<GameObject> _allTetriminos;


    private InputAction _mouseHover, _mouseDrag;
    public Inputs Actions;

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

        _mouseHover = InputSystem.actions.FindAction( "MouseHover" );

        _mouseDrag = InputSystem.actions.FindAction( "MouseDrag" );

        Actions = new( _mouseHover, _mouseDrag );

        _mapGenerator.GenerateBasicGrid( _mapGenerator.Rows, _mapGenerator.Cols );

    }

    // Update is called once per frame
    void Update()
    {

        _mouseWorldPosition = Camera.main.ScreenToWorldPoint( _mouseHover.ReadValue<Vector2>() );

        if ( Input.GetKeyDown( KeyCode.Z ) )
            SpawnTetrimino();

    }

    void SpawnTetrimino()
    {

        for ( int i = 0; i < MapGenerator.Rows; i++ )
        {

            for ( int j = 0; j < MapGenerator.Cols; j++ )
            {

                if ( !MapGenerator.Grid[ i, j ].Occupied )
                {

                    GameObject tetrimino = Instantiate( _tetriminoPrefab, new Vector3( i + 0.5f, j + 0.5f, 0 ) * MapGenerator.CellSize, default, null );

                    tetrimino.tag = "Tetrimino";

                    tetrimino.name = $"Tetrimino{_allTetriminos.Count}";

                    tetrimino.transform.localScale *= MapGenerator.CellSize;

                    _allTetriminos.Add( tetrimino );

                    return;

                }

            }

        }

    }

}

public struct Inputs
{

    public InputAction MouseHover { get; private set; }
    public InputAction MouseDrag { get; private set; }

    public Inputs( InputAction hover, InputAction drag )
    {

        MouseHover = hover;
        MouseDrag = drag;

    }

}
