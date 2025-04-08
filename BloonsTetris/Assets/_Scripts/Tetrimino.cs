using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class Tetrimino : MonoBehaviour, IDraggable
{

    private Cell _currentCell;

    private Vector3 _currentPosition;

    private bool _dragging;

    private MapGenerator _mapGenerator;

    private GameManager.DefaultTetriminoShape _shape;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        _currentPosition = transform.position;

        _mapGenerator = GameManager.Instance.MapGenerator;

        _currentCell = _mapGenerator.Grid[ (int)Math.Floor( _currentPosition.x / GameManager.Instance.MapGenerator.CellSize ),
            (int)Math.Floor( _currentPosition.y / GameManager.Instance.MapGenerator.CellSize ) ];

        _mapGenerator.Grid[ _currentCell.X, _currentCell.Y ].SetToOccupied();

        _dragging = false;

        SubscribeInputs();

    }

    // Update is called once per frame
    void Update()
    {


    }

    public void OnDestroy()
    {

        UnsubscribeInputs();

    }

    public void OnDragEnd( InputAction.CallbackContext ctx )
    {

        if ( !_dragging )
            return;

        if ( GameManager.Instance.MapGenerator.TryPlaceTetrimino( out Cell newCell ) )
        {

            _mapGenerator.Grid[ _currentCell.X, _currentCell.Y ].SetToUnOccupied();

            _currentCell = newCell;

            _mapGenerator.Grid[ _currentCell.X, _currentCell.Y ].SetToOccupied();

            _currentPosition = transform.position = _currentCell.CellCenterToWorldSpace();

        }
        else
        {

            transform.position = _currentPosition;

        }

        _dragging = false;

        Debug.Log( $"Ending! - ({name})" );
    }

    public void OnDragProccessing( InputAction.CallbackContext ctx )
    {

        if ( !_dragging )
            return;

        transform.position = new( GameManager.Instance.MouseWorldPosition.x, GameManager.Instance.MouseWorldPosition.y, 10 );

        Debug.Log( $"Processing! - ({name})" );

    }

    public void OnDragStart( InputAction.CallbackContext ctx )
    {

        Ray mouseCast = Camera.main.ScreenPointToRay( ctx.ReadValue<Vector2>() );

        RaycastHit2D hit = Physics2D.GetRayIntersection( mouseCast );

        if ( hit.collider != null )
        {

            if ( hit.collider.gameObject == gameObject )
            {
                Debug.Log( $"Starting! - ({name})" );

                _dragging = true;

                _currentPosition = transform.position;

                return;

            }

        }

        _dragging = false;

    }

    public void SubscribeInputs()
    {

        GameManager.Instance.Actions.MouseDrag.started += OnDragStart;
        GameManager.Instance.Actions.MouseDrag.performed += OnDragProccessing;
        GameManager.Instance.Actions.MouseDrag.canceled += OnDragEnd;

    }

    public void UnsubscribeInputs()
    {

        GameManager.Instance.Actions.MouseDrag.started -= OnDragStart;
        GameManager.Instance.Actions.MouseDrag.performed -= OnDragProccessing;
        GameManager.Instance.Actions.MouseDrag.canceled -= OnDragEnd;

    }

}
