using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent( typeof( CircleCollider2D ) ), RequireComponent( typeof( Rigidbody2D ) ), DisallowMultipleComponent]
public class Tetrimino : MonoBehaviour
{

    private bool _dragging;

    private GridManager _gridManager;

    private Vector3 _currentPosition;

    private Quaternion _currentRotation;


    private float _power, _cooldown, _range;
    public float Power { get { return _power; } }
    public float Cooldown { get { return _cooldown; } }
    public float Range { get { return _range; } }


    private List<Cell> _currentCells;

    private List<Vector2> _localCellPositions;

    private List<Vector2> _currentLocalCells;

    private DefaultShape _baseShape;

    private ElementalTypes _elementalTypes;

    private Coroutine _abilityAction;

    private List<GameObject> _enemiesInRange;

    #region Init and Operation Functions
    public void Init( DefaultTetrimino shapeData )
    {

        _gridManager = GameManager.Instance.GridManager;

        _dragging = false;

        _currentPosition = transform.position;

        _currentRotation = transform.rotation;

        _currentCells = new List<Cell>();

        _currentLocalCells = new List<Vector2>();

        _enemiesInRange = new List<GameObject>();

        SubscribeInputs();

        CopyShapeData( shapeData );

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;

        rb.constraints = RigidbodyConstraints2D.FreezePosition;

        CircleCollider2D rangeCollider = GetComponent<CircleCollider2D>();

        rangeCollider.isTrigger = true;

        rangeCollider.radius = _range * 0.5f;

        _abilityAction = InitializeAbility();

    }

    private void CopyShapeData( DefaultTetrimino shapeData )
    {

        _localCellPositions = shapeData.localCellPositions;

        _baseShape = shapeData.Shape;

        _power = shapeData.Power;

        _range = shapeData.Range;

        _cooldown = shapeData.Cooldown;

        _elementalTypes = shapeData.ElementalTypes;

    }

    private void SubscribeInputs()
    {

        GameManager.Instance.Actions.MouseDrag.started += OnDragStart;
        GameManager.Instance.Actions.MouseDrag.performed += OnDragProccessing;
        GameManager.Instance.Actions.MouseDrag.canceled += OnDragEnd;
        GameManager.Instance.Actions.Rotate.performed += OnRotate;

    }

    private void UnsubscribeInputs()
    {

        GameManager.Instance.Actions.MouseDrag.started -= OnDragStart;
        GameManager.Instance.Actions.MouseDrag.performed -= OnDragProccessing;
        GameManager.Instance.Actions.MouseDrag.canceled -= OnDragEnd;
        GameManager.Instance.Actions.Rotate.performed -= OnRotate;

    }

    private void OnDestroy()
    {

        UnsubscribeInputs();

    }

    private void OnDragEnd( InputAction.CallbackContext ctx )
    {

        if ( !_dragging )
            return;

        foreach ( Cell cell in _currentCells )
            _gridManager.Grid[ cell.X, cell.Y ].SetToEmpty();

        if ( GameManager.Instance.GridManager.TryPlaceTetriminoShape( _localCellPositions, out List<Cell> newCells, out Vector3 newPosition ) )
        {

            _currentCells = newCells;

            foreach ( Cell cell in _currentCells )
                _gridManager.Grid[ cell.X, cell.Y ].SetToOccupied();

            transform.position = _currentPosition = newPosition;

        }
        else
        {

            foreach ( Cell cell in _currentCells )
                _gridManager.Grid[ cell.X, cell.Y ].SetToOccupied();

            transform.SetPositionAndRotation( _currentPosition, _currentRotation );

            _localCellPositions = _currentLocalCells;

        }

        _dragging = false;

        //Debug.Log( $"Ending! - ({name})" );
    }

    private void OnDragProccessing( InputAction.CallbackContext ctx )
    {

        if ( !_dragging )
            return;

        transform.position = new( GameManager.Instance.MouseWorldPosition.x, GameManager.Instance.MouseWorldPosition.y );

        //Debug.Log( $"Processing! - ({name})" );

    }

    private void OnDragStart( InputAction.CallbackContext ctx )
    {

        if ( ctx.canceled )
            return;

        Ray mouseCast = Camera.main.ScreenPointToRay( ctx.ReadValue<Vector2>() );

        RaycastHit2D hit = Physics2D.GetRayIntersection( mouseCast, Mathf.Infinity, LayerMask.GetMask( "TetriminoBase" ) );

        if ( hit.collider != null )
        {

            if ( hit.collider.transform.parent == transform )
            {

                //Debug.Log( $"Starting! - ({name})" );

                _dragging = true;

                _currentPosition = transform.position;

                _currentRotation = transform.rotation;

                _currentLocalCells = new( _localCellPositions );

                return;

            }

        }

        _dragging = false;

    }

    private void OnRotate( InputAction.CallbackContext ctx )
    {

        //Debug.Log( $"Rotating - {name}, (ReadValue: {ctx.ReadValue<float>()})" );

        if ( !_dragging || ctx.ReadValue<float>() == 0 )
            return;

        RotateTetrimino( ctx.ReadValue<float>() > 0 );

    }

    private void RotateTetrimino( bool clockwise )
    {

        if ( clockwise )
        {

            transform.RotateAround( transform.position, Vector3.back, 90f );

            for ( int i = 0; i < _localCellPositions.Count; i++ )
            {

                _localCellPositions[ i ] = new( _localCellPositions[ i ].y, -( _localCellPositions[ i ].x ) );

            }

        }
        else
        {

            transform.RotateAround( transform.position, Vector3.back, -90f );

            for ( int i = 0; i < _localCellPositions.Count; i++ )
            {

                _localCellPositions[ i ] = new( -( _localCellPositions[ i ].y ), _localCellPositions[ i ].x );

            }

        }

    }

    public void OnTriggerEnter2D( Collider2D collidingObject )
    {

        Debug.Log( collidingObject.name );

        if ( collidingObject.GetComponent<Enemy>() != null )
            _enemiesInRange.Add( collidingObject.gameObject );

    }

    public void OnTriggerExit2D( Collider2D collidingObject )
    {

        if ( collidingObject.GetComponent<Enemy>() != null )
            _enemiesInRange.Remove( collidingObject.gameObject );

    }
    #endregion

    #region Ability Functions
    private Coroutine InitializeAbility()
    {

        IEnumerator ability = _baseShape switch
        {
            ( DefaultShape.L ) => Use_L_Ability(),
            ( DefaultShape.T ) => Use_T_Ability(),
            ( DefaultShape.Line ) => Use_Line_Ability(),
            ( DefaultShape.Square ) => Use_Square_Ability(),
            ( DefaultShape.Zigzag ) => Use_Zigzag_Ability(),
            ( DefaultShape.ReverseZigZag ) => Use_ReverseZigzag_Ability(),
            ( DefaultShape.ReverseL ) => Use_ReverseL_Ability(),
            _ => throw new System.Exception( "Invalid Shape!" ),
        };

        return StartCoroutine( ability );


    }

    private IEnumerator Use_L_Ability()
    {

        while ( true )
        {

            yield return new WaitUntil( () => _enemiesInRange.Count > 0 );

            //Perform Ability Here
            Debug.Log( "L Used" );

            yield return new WaitForSeconds( _cooldown );

        }

    }

    private IEnumerator Use_ReverseL_Ability()
    {

        yield return new WaitUntil( () => _enemiesInRange.Count > 0 );

        //Perform Ability Here
        Debug.Log( "Reverse L Used" );

        yield return new WaitForSeconds( _cooldown );

    }

    private IEnumerator Use_T_Ability()
    {

        yield return new WaitUntil( () => _enemiesInRange.Count > 0 );

        //Perform Ability Here
        Debug.Log( "T Used" );

        yield return new WaitForSeconds( _cooldown );

    }

    private IEnumerator Use_Line_Ability()
    {

        yield return new WaitUntil( () => _enemiesInRange.Count > 0 );

        //Perform Ability Here
        Debug.Log( "Line Used" );

        yield return new WaitForSeconds( _cooldown );

    }

    private IEnumerator Use_Zigzag_Ability()
    {

        yield return new WaitUntil( () => _enemiesInRange.Count > 0 );

        //Perform Ability Here
        Debug.Log( "Zigzag Used" );

        yield return new WaitForSeconds( _cooldown );

    }

    private IEnumerator Use_ReverseZigzag_Ability()
    {

        yield return new WaitUntil( () => _enemiesInRange.Count > 0 );

        //Perform Ability Here
        Debug.Log( "Reverse Zigzag Used" );

        yield return new WaitForSeconds( _cooldown );

    }

    private IEnumerator Use_Square_Ability()
    {

        yield return new WaitUntil( () => _enemiesInRange.Count > 0 );

        //Perform Ability Here
        Debug.Log( "Square Used" );

        yield return new WaitForSeconds( _cooldown );

    }
    #endregion

}
