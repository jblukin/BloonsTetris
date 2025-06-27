using System.Collections;
using UnityEngine;

public class BaseEnemy : Enemy
{
    public override float PathTraversedPercetange => _pathTraversedPercentage;
    public override float MaxHP => _maxHP;
    public override int CurrentWaypointIndex => _currentWaypointIdx;
    public override float CurrentHP => _currentHP;

    private bool onInit = false;

    // Update is called once per frame
    protected override void Update()
    {

        if ( !onInit )
        {

            Debug.LogWarning( "Enemy Was Not Initialized after Instantiating! Please check your code!" );
            onInit = true;

        }
        else
        {

            Move();

        }

    }

    public override void Init( EnemyData enemyData )
    {

        onInit = true;

        _maxHP = _currentHP = enemyData.MaxHP;
        _power = enemyData.Power;
        _speed = enemyData.Speed;
        _range = enemyData.Range;
        _abilityCooldown = enemyData.AbilityCooldown;
        _abilityDuration = enemyData.AbilityDuration;
        _elementalTypes = _baseElementalTypes = enemyData.ElementalTypes;
        _elementalResistances = _baseElementalResistences = enemyData.ElementalResistances;
        _statusEffects = _baseStatusEffects = enemyData.BaseStatusEffects;
        _deathExplosionPower = enemyData.DeathExplosionPower;
        _deathExplosionRadius = enemyData.DeathExplosionRadius;
        _pathWaypoints = GameManager.Instance.GridManager.EnemyPathWaypoints;
        _currentWaypointIdx = enemyData.StartingWaypointIndex + 1;
        _pathTraversedPercentage = _poisonDoTValue = _fireDoTValue = 0;

        _currentTargetingTetriminos = new();

        _distanceBetweenPrevCurrWaypoint = Vector2.Distance( _pathWaypoints[ _currentWaypointIdx - 1 ], _pathWaypoints[ _currentWaypointIdx ] );

        if ( enemyData.HasAbility )
        {

            _receivingObjects = new();

            GameManager.Instance.EnemyManager.ReceivingObjectDestroyed.AddListener( UpdateListData );

            GameObject rangeDetector = new();

            rangeDetector.transform.SetParent( transform, false );

            rangeDetector.transform.localPosition = Vector2.zero;

            _rangeCollider = rangeDetector.AddComponent<CircleCollider2D>();

            _rangeCollider.isTrigger = true;

            _rangeCollider.radius = _range * 0.5f;

            _rangeCollider.callbackLayers = _rangeCollider.contactCaptureLayers = LayerMask.GetMask( "Enemy", "TetriminoBase" );

            _abilityAction = StartCoroutine( nameof( UseAbility ) );

        }

        CircleCollider2D c = GetComponent<CircleCollider2D>();

        c.isTrigger = true;

        c.contactCaptureLayers = LayerMask.GetMask( "Default", "Enemy" );

        c.callbackLayers = LayerMask.GetMask( "Nothing" );

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;

        rb.gravityScale = 0;

        gameObject.layer = LayerMask.NameToLayer( "Enemy" );

    }

    public override void ReceiveDamageOrHealth( float amountReceived, bool isPercentage = false, bool isHealing = false )
    {

        if ( amountReceived > 0 )
        {

            if ( isHealing )
                _currentHP += CalculateFinalReceivedHealth( amountReceived );

            else
            {

                _currentHP -= CalculateFinalReceivedDamage( amountReceived );

                if ( _currentHP <= 0 )
                {

                    OnDeath( _statusEffects.HasFlag( StatusEffects.Burnt ) );
                    return;

                }

            }

        }

    }

    public override void ApplyElementalEffects( ElementalTypes elementalTypes, float doTValueIncrement = 0, float incomingSlowPercentage = 0 )
    {

        if ( elementalTypes.HasFlag( ElementalTypes.Fire ) )
        {

            _fireDoTValue += doTValueIncrement;

            _fireDoTAction ??= StartCoroutine( ProcessDamageOverTime( 5f, true ) );

            _statusEffects |= StatusEffects.Burnt;

        }

        if ( elementalTypes.HasFlag( ElementalTypes.Poison ) )
        {

            _poisonDoTValue += doTValueIncrement;

            _poisonDoTAction ??= StartCoroutine( ProcessDamageOverTime( 5f, false ) );

            _statusEffects |= StatusEffects.Poisoned;

        }


        if ( elementalTypes.HasFlag( ElementalTypes.Lightning ) )
        {

            _statusEffects |= StatusEffects.Marked;

        }

        if ( elementalTypes.HasFlag( ElementalTypes.Ice ) )
        {

            _slowedPercentage = Mathf.Max( _slowedPercentage, incomingSlowPercentage * 0.01f );

            _statusEffects |= StatusEffects.Slowed;

        }

    }

    public override void ApplyElementalResistances( ElementalResistances elementalResistances )
    {

        _elementalResistances |= elementalResistances;

    }

    protected override void Move()
    {

        float finalSpeed = _statusEffects.HasFlag( StatusEffects.Slowed ) ? _speed * ( 1 - _slowedPercentage ) : _speed;

        finalSpeed *= Time.deltaTime;

        float distanceToCurrentWaypoint = Vector2.Distance( transform.position, _pathWaypoints[ _currentWaypointIdx ] );

        float distanceFromPrevWaypoint = Vector2.Distance( _pathWaypoints[ _currentWaypointIdx - 1 ], transform.position );

        if ( distanceToCurrentWaypoint <= finalSpeed )
        {

            _currentWaypointIdx++;

            if ( _currentWaypointIdx >= _pathWaypoints.Count )
            {

                Destroy( gameObject );

                return;

            }

            _distanceBetweenPrevCurrWaypoint = Vector2.Distance( _pathWaypoints[ _currentWaypointIdx - 1 ], _pathWaypoints[ _currentWaypointIdx ] );

        }

        _pathTraversedPercentage = _currentWaypointIdx + ( distanceFromPrevWaypoint / _distanceBetweenPrevCurrWaypoint );

        Vector2 dir = _pathWaypoints[ _currentWaypointIdx ] - new Vector2( transform.position.x, transform.position.y );

        dir.Normalize();

        transform.Translate( finalSpeed * dir );

        //Debug.Log( $"{name}: {_pathTraversedPercentage}" );

    }

    protected override float CalculateFinalReceivedDamage( float damageReceived, bool isPercentage = false, ElementalTypes elementalTypes = 0 )
    {

        if ( _elementalResistances is 0 ||
            ( !elementalTypes.HasFlag( ElementalTypes.Fire ) &&
            !elementalTypes.HasFlag( ElementalTypes.Poison ) &&
            !elementalTypes.HasFlag( ElementalTypes.Physical ) ) )
            return isPercentage ? _maxHP * ( damageReceived * 0.01f ) : damageReceived;

        else
        {

            if ( ( elementalTypes.HasFlag( ElementalTypes.Fire ) && _elementalResistances.HasFlag( ElementalResistances.FireHigh ) ) ||
                ( elementalTypes.HasFlag( ElementalTypes.Poison ) && _elementalResistances.HasFlag( ElementalResistances.PoisonHigh ) ) ||
                ( elementalTypes.HasFlag( ElementalTypes.Physical ) && _elementalResistances.HasFlag( ElementalResistances.PhysicalHigh ) ) )
                damageReceived *= 0.1f;

            else if ( ( elementalTypes.HasFlag( ElementalTypes.Fire ) && _elementalResistances.HasFlag( ElementalResistances.FireMedium ) ) ||
                ( elementalTypes.HasFlag( ElementalTypes.Poison ) && _elementalResistances.HasFlag( ElementalResistances.PoisonMedium ) ) ||
                ( elementalTypes.HasFlag( ElementalTypes.Physical ) && _elementalResistances.HasFlag( ElementalResistances.PhysicalMedium ) ) )
                damageReceived *= 0.4f;

            else if ( ( elementalTypes.HasFlag( ElementalTypes.Fire ) && _elementalResistances.HasFlag( ElementalResistances.FireLow ) ) ||
                ( elementalTypes.HasFlag( ElementalTypes.Poison ) && _elementalResistances.HasFlag( ElementalResistances.PoisonLow ) ) ||
                ( elementalTypes.HasFlag( ElementalTypes.Physical ) && _elementalResistances.HasFlag( ElementalResistances.PhysicalLow ) ) )
                damageReceived *= 0.7f;


            return isPercentage ? _maxHP * ( damageReceived * 0.01f ) : damageReceived;

        }

    }

    protected override float CalculateFinalReceivedHealth( float healthReceived, bool isPercentage = false, ElementalTypes elementalTypes = 0 )
    {

        return isPercentage ? _maxHP * ( healthReceived * 0.01f ) : healthReceived;

    }

    protected override void OnDeath( bool deathByFire = false )
    {

        GameManager.Instance.EnemyManager.ReceivingObjectDestroyed?.Invoke( gameObject );
        GameManager.Instance.EnemyManager.AllEnemies.Remove( this );

        foreach ( Tetrimino t in _currentTargetingTetriminos )
        {

            t.RemoveDeadEnemyFromTargeting( this );

        }

        StopAllCoroutines();

        if ( deathByFire )
            Explode();

        //Until Object Pooling
        Destroy( gameObject );

    }

    protected override IEnumerator ProcessDamageOverTime( float tickRate, bool isFire )
    {

        if ( isFire )
        {

            while ( true )
            {

                yield return new WaitUntil( () => _fireDoTValue > 0 );

                _currentHP -= _fireDoTValue;

                _fireDoTValue--;

                if ( _currentHP <= 0 )
                {

                    OnDeath( true );
                    yield break;

                }

                yield return new WaitForSeconds( tickRate );

            }

        }
        else
        {

            while ( true )
            {

                yield return new WaitUntil( () => _poisonDoTValue > 0 );

                _currentHP -= _poisonDoTValue;

                _poisonDoTValue--;

                if ( _currentHP <= 0 )
                {

                    OnDeath();
                    yield break;

                }

                yield return new WaitForSeconds( tickRate );

            }

        }

    }

    protected override IEnumerator UseAbility()
    {

        yield break;

        //Copy this part for enemies with abiltiies
        //while ( true )
        //{

        //    //Uncomment if ability has a range
        //    //yield return new WaitUntil(() => _receivingObjects.Count > 0);

        //    Debug.Log( $"Enemy of type: {GetType()} used ability" );
        //    yield return new WaitForSeconds( _abilityCooldown );

        //}

    }

    protected override void OnTriggerEnter2D( Collider2D collidingObject )
    {

        //Override with this if enemy has ability
        //if ( collidingObject.TryGetComponent</* Type of Object That will be receiving the abilty effect from this Enemy */>( out var receivingObject ) )
        //{

        //    _receivingObjects.Add( collidingObject.gameObject );

        //    //Apply any effects to receivingObject here

        //}

    }

    protected override void OnTriggerExit2D( Collider2D collidingObject )
    {

        //Override with this if enemy has ability
        //if ( collidingObject.TryGetComponent</* Type of Object That will be receiving the abilty effect from this Enemy */>( out var receivingObject ) )
        //{

        //    _receivingObjects.Remove( collidingObject.gameObject );

        //    //Remove any applied effects from receivingObject here

        //}

    }

    public override void ClearStatusEffects( StatusEffects statusEffectsToClear = StatusEffects.All )
    {

        _statusEffects &= ~statusEffectsToClear;

        _statusEffects |= _baseStatusEffects;

        if ( statusEffectsToClear.HasFlag( StatusEffects.Slowed ) )
        {

            _slowedPercentage = 0;

        }


    }

    public override void ClearResistences( ElementalResistances elementalResistances = ElementalResistances.All )
    {

        _elementalResistances &= ~elementalResistances;

        _elementalResistances |= _baseElementalResistences;

    }

    protected override void Explode()
    {

        gameObject.layer = LayerMask.NameToLayer( "Default" );

        Collider2D[] results = Physics2D.OverlapCircleAll( transform.position, _deathExplosionRadius * GameManager.Instance.GridManager.CellSize * 0.5f, LayerMask.GetMask( "Enemy" ) );

        foreach ( var collider in results )
        {

            if ( collider.TryGetComponent<Enemy>( out var enemy ) )
            {

                //enemy.ApplyElementalEffects( ElementalTypes.Fire );
                enemy.ReceiveDamageOrHealth( _deathExplosionPower );


            }

        }

    }

    protected override void UpdateListData( GameObject obj )
    {

        if ( obj != gameObject )
            _receivingObjects.Remove( obj );

    }

    public override void AddTetriminoToTargetingList( Tetrimino tetrimino )
    {
        
        _currentTargetingTetriminos.Add( tetrimino );

    }

    public override void RemoveTetriminoFromTargetingList( Tetrimino tetrimino )
    {
        
        _currentTargetingTetriminos.Remove( tetrimino );

    }

    public override int CompareTo( object otherEnemy )
    {

        return PathTraversedPercetange.CompareTo( ( otherEnemy as Enemy ).PathTraversedPercetange );

    }
}
