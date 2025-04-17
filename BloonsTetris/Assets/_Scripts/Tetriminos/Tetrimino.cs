using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent( typeof( CompositeCollider2D ) )]
public class Tetrimino : MonoBehaviour, IDraggable
{
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

    private bool _dragging;

    private MapGenerator _mapGenerator;

    private Vector3 _currentPosition;


    private float _power, _cooldown, _range;
    public float Power { get { return _power; } }
    public float Cooldown { get { return _cooldown; } }
    public float Range { get { return _range; } }


    private List<Cell> _currentCells;

    private List<Vector2> _localCellPositions;

    private DefaultShape _baseShape;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        _mapGenerator = GameManager.Instance.MapGenerator;

        _dragging = false;

        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        GetComponent<CompositeCollider2D>().geometryType = CompositeCollider2D.GeometryType.Polygons;

        _currentPosition = transform.position;

        _currentCells = new List<Cell>();

        SubscribeInputs();

    }

    public void CopyBaseShapeData( DefaultTetrimino shapeData )
    {

        _localCellPositions = shapeData.localCellPositions;

        _baseShape = shapeData.Shape;

        _power = shapeData.Power;

        _range = shapeData.Range;

        _cooldown = shapeData.Cooldown;

    }

    public void OnDestroy()
    {

        UnsubscribeInputs();

    }

    public void OnRotate( InputAction.CallbackContext ctx )
    {

        if ( !_dragging )
            return;

        if(ctx.ReadValue<float>() > 0)
        {

            //AddRotationFunctionHere

        } else if(ctx.ReadValue<float>() < 0)
        {

            //AddRotationFunctionHere

        }

    }

    public void OnDragEnd( InputAction.CallbackContext ctx )
    {

        if ( !_dragging )
            return;

        foreach ( Cell cell in _currentCells )
            _mapGenerator.Grid[ cell.X, cell.Y ].SetToUnOccupied();

        if ( GameManager.Instance.MapGenerator.TryPlaceTetriminoShape( _localCellPositions, out List<Cell> newCells, out Vector3 newPosition ) )
        {

            _currentCells = newCells;

            foreach ( Cell cell in _currentCells )
                _mapGenerator.Grid[ cell.X, cell.Y ].SetToOccupied();

            transform.position = _currentPosition = newPosition;

        }
        else
        {

            foreach ( Cell cell in _currentCells )
                _mapGenerator.Grid[ cell.X, cell.Y ].SetToOccupied();

            transform.position = _currentPosition;

        }

        _dragging = false;

        //Debug.Log( $"Ending! - ({name})" );
    }

    public void OnDragProccessing( InputAction.CallbackContext ctx )
    {

        if ( !_dragging )
            return;

        transform.position = new( GameManager.Instance.MouseWorldPosition.x, GameManager.Instance.MouseWorldPosition.y, 10 );

        //Debug.Log( $"Processing! - ({name})" );

    }

    public void OnDragStart( InputAction.CallbackContext ctx )
    {

        if ( ctx.canceled )
            return;

        Ray mouseCast = Camera.main.ScreenPointToRay( ctx.ReadValue<Vector2>() );

        RaycastHit2D hit = Physics2D.GetRayIntersection( mouseCast );

        if ( hit.collider != null )
        {

            if ( hit.collider.transform == transform )
            {

                //Debug.Log( $"Starting! - ({name})" );

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
        GameManager.Instance.Actions.Rotate.performed += OnRotate;

    }

    public void UnsubscribeInputs()
    {

        GameManager.Instance.Actions.MouseDrag.started -= OnDragStart;
        GameManager.Instance.Actions.MouseDrag.performed -= OnDragProccessing;
        GameManager.Instance.Actions.MouseDrag.canceled -= OnDragEnd;
        GameManager.Instance.Actions.Rotate.performed -= OnRotate;

    }

}
