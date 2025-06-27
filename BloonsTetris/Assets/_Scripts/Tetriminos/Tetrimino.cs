using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private SortedSet<Enemy> _enemiesInRange;

    #region Init and Operation Functions
    public void Init( DefaultTetrimino shapeData )
    {

        _gridManager = GameManager.Instance.GridManager;

        _dragging = false;

        _currentPosition = transform.position;

        _currentRotation = transform.rotation;

        _currentCells = new List<Cell>();

        _currentLocalCells = new List<Vector2>();

        _enemiesInRange = new SortedSet<Enemy>( new PathTravelledComparer() );

        SubscribeInputs();

        CopyShapeData( shapeData );

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;

        rb.constraints = RigidbodyConstraints2D.FreezePosition;

        CircleCollider2D rangeCollider = GetComponent<CircleCollider2D>();

        rangeCollider.isTrigger = true;

        rangeCollider.radius = _range * 0.5f;

        rangeCollider.callbackLayers = rangeCollider.contactCaptureLayers = LayerMask.GetMask( "Enemy", "TetriminoBase" );

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
            _gridManager.GetGridCell( cell.X, cell.Y ).SetToEmpty();

        if ( GameManager.Instance.GridManager.TryPlaceTetriminoShape( _localCellPositions, out List<Cell> newCells, out Vector3 newPosition ) )
        {

            _currentCells = newCells;

            foreach ( Cell cell in _currentCells )
                _gridManager.GetGridCell( cell.X, cell.Y ).SetToOccupied();

            transform.position = _currentPosition = newPosition;

        }
        else
        {

            foreach ( Cell cell in _currentCells )
                _gridManager.GetGridCell( cell.X, cell.Y ).SetToOccupied();

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

    private void OnTriggerEnter2D( Collider2D collidingObject )
    {

        if ( collidingObject.TryGetComponent<Enemy>( out var enemy ) )
        {

            _enemiesInRange.Add( enemy );

            enemy.AddTetriminoToTargetingList( this );

        }

    }

    private void OnTriggerExit2D( Collider2D collidingObject )
    {

        if ( collidingObject.TryGetComponent<Enemy>( out var enemy ) )
        {

            _enemiesInRange.Remove( enemy );

            enemy.RemoveTetriminoFromTargetingList( this );

            switch ( _baseShape )
            {

                case DefaultShape.Square:
                    {
                        enemy.ClearStatusEffects( Enemy.StatusEffects.Slowed );
                        break;
                    }

            }

        }
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

    public void DisableAbility()
    {

        StopCoroutine( _abilityAction );

        foreach ( var enemy in _enemiesInRange )
        {

            switch ( _baseShape )
            {

                case DefaultShape.Square:
                    {
                        enemy.ClearStatusEffects( Enemy.StatusEffects.Slowed );
                        break;
                    }


            }

        }

    }

    public void EnableAbility()
    {

        _abilityAction = InitializeAbility();

        foreach ( var enemy in _enemiesInRange )
        {
            switch ( _baseShape )
            {

                case DefaultShape.Square:
                    {
                        enemy.ApplyElementalEffects( _elementalTypes, 0, _power );
                        break;
                    }

            }
        }
    }

    public void RemoveDeadEnemyFromTargeting( Enemy enemy )
    {

        _enemiesInRange.Remove( enemy );

    }

    private IEnumerator Use_L_Ability()
    {

        while ( true )
        {

            yield return new WaitUntil( () => _enemiesInRange.Count > 0 );

            //Perform Ability Here

            #region Brief Partial Example
            /*//Find Farthest Enemy Along Path

            //Fire/Spawn Projectile (Needs to be gameObject with CircleCollider2D)
            //Projectile Script Needed with OnTriggerEnter2D Message that calls ReceiveAbility Func of collidingObject (which will be an enemy)

            GameObject farthestEnemy = null;

            float distanceToFarthest = 0;

            foreach(var enemy in _enemiesInRange)
            {

                //enemy.RecieveAbility( _power, _elementalTypes );

            }

            //var projectile = Instantiate( _projectilePrefab );
            //var projectileScript = projectile.GetComponent<Projectile>();
            //projectileScript.Init( target = farthestEnemy, speed, baseDamage, etc... );
            //projectileScript.Fire();*/
            #endregion

            Debug.Log( "L Used" );

            yield return new WaitForSeconds( _cooldown );

        }

    }

    private IEnumerator Use_ReverseL_Ability()
    {

        while ( true )
        {

            yield return new WaitUntil( () => _enemiesInRange.Count > 0 );

            //Spawn projectile

            //Do Visual Stuff Here

            //Grab Furthest Enemy Position in Range
            //Calculate time to reach position
            //Explode after that duration

            var mortarTargetPosition = _enemiesInRange.Max().transform.position;

            var _mortarSpeed = 250;

            float timeTillExplosion = Vector2.Distance( mortarTargetPosition, transform.position ) / _mortarSpeed;

            Debug.Log( "Start of Explosion Timer; Time till Explosion = " + timeTillExplosion );

            StartCoroutine( nameof( ExplodeMortar ), new Tuple<Vector2, float>( mortarTargetPosition, timeTillExplosion ) );

            //Perform Ability Here
            Debug.Log( "Reverse L Used" );

            yield return new WaitForSeconds( _cooldown );

        }

    }

    private IEnumerator Use_T_Ability()
    {

        while ( true )
        {

            yield return new WaitUntil( () => _enemiesInRange.Count > 0 );

            //Perform Ability Here
            Debug.Log( "T Used" );

            yield return new WaitForSeconds( _cooldown );

        }

    }

    private IEnumerator Use_Line_Ability()
    {

        while ( true )
        {

            yield return new WaitUntil( () => _enemiesInRange.Count > 0 );

            //Perform Ability Here
            Debug.Log( "Line Used" );

            yield return new WaitForSeconds( _cooldown );

        }

    }

    private IEnumerator Use_Zigzag_Ability()
    {

        while ( true )
        {

            yield return new WaitUntil( () => _enemiesInRange.Count > 0 );


            //Perform Ability Here
            Debug.Log( "Zigzag Used" );

            //VISUALS
            //Make a gameobject that is literally just a sprite
            //this sprite will translate to the enemy position really really fast (like bullet)

            /*Continue Here*/

            //ACTUAL DAMAGE
            //Grab the enemy farthest on the path, and KILL
            var enemy = _enemiesInRange.Max();
            enemy.ReceiveDamageOrHealth( _power );

            yield return new WaitForSeconds( _cooldown );

        }

    }

    private IEnumerator Use_ReverseZigzag_Ability()
    {

        while ( true )
        {

            yield return new WaitUntil( () => _enemiesInRange.Count > 0 );

            //Perform Ability Here
            Debug.Log( "Reverse Zigzag Used" );

            yield return new WaitForSeconds( _cooldown );

        }

    }

    private IEnumerator Use_Square_Ability()
    {

        while ( true )
        {

            yield return new WaitUntil( () => _enemiesInRange.Count > 0 );

            //Perform Ability Here
            Debug.Log( "Square Used" );

            foreach ( Enemy enemy in _enemiesInRange )
            {

                enemy.ApplyElementalEffects( _elementalTypes, 0, _power );

            }


            yield return new WaitForSeconds( _cooldown );

        }

    }
    #endregion

    private IEnumerator ExplodeMortar( Tuple<Vector2, float> explosionData )
    {

        yield return new WaitForSeconds( explosionData.Item2 );

        Debug.Log( "Exploded!" );

        Collider2D[] results = Physics2D.OverlapCircleAll( explosionData.Item1, 5 /*Mortar Explosion Radius*/ * GameManager.Instance.GridManager.CellSize * 0.5f, LayerMask.GetMask( "Enemy" ) );

        foreach ( var collider in results )
        {

            if ( collider.TryGetComponent<Enemy>( out var enemy ) )
            {

                float prevHealth = enemy.CurrentHP;

                enemy.ApplyElementalEffects( ElementalTypes.Fire );
                enemy.ReceiveDamageOrHealth( _power );

                Debug.Log( $"{enemy.name} -- (Current Health / Previous Health): {enemy.CurrentHP} / {prevHealth}" );


            }

        }

    }

}
