using System.Collections;
using UnityEngine;

public class BaseEnemy : Enemy
{

    private bool onInit = false;

    // Update is called once per frame
    void Update()
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

    public override void Init( BasicEnemyData enemyData )
    {

        onInit = true;

        _maxHP = _currentHP = enemyData.MaxHP;
        _power = enemyData.Power;
        _speed = enemyData.Speed;
        _range = enemyData.Range;
        _elementalTypes = enemyData.ElementalTypes;
        _elementalResistances = enemyData.ElementalResistances;
        _statusEffects = enemyData.BaseStatusEffects;
        _deathExplosionPower = enemyData.DeathExplosionPower;
        _deathExplosionRadius = enemyData.DeathExplosionRadius;

        _poisonDoTValue = _fireDoTValue = 0;

        if ( enemyData.HasAbility )
            _abilityAction = StartCoroutine( UseAbility() );

        gameObject.layer = LayerMask.NameToLayer( "Enemy" );

    }

    public override void ReceiveAbility( float amountReceived = 0, ElementalTypes elementalTypes = 0, bool isHealing = false )
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

                    OnDeath( _statusEffects.HasFlag( StatusEffects.Burn ) );
                    return;

                }

            }

        }

        if ( elementalTypes is not 0 )
            ProcessElementalEffects( elementalTypes );

    }

    protected override void Move()
    {
        //Set to follow path from Grid Data in use

        transform.position += _speed * Time.deltaTime * Vector3.left;

    }

    protected override float CalculateFinalReceivedDamage( float damageReceived, ElementalTypes elementalTypes = 0 )
    {

        if ( _elementalResistances is 0 ||
            ( !elementalTypes.HasFlag( ElementalTypes.Fire ) &&
            !elementalTypes.HasFlag( ElementalTypes.Poison ) &&
            !elementalTypes.HasFlag( ElementalTypes.Physical ) ) )
            return damageReceived;

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


            return damageReceived;

        }

    }

    protected override float CalculateFinalReceivedHealth( float healthReceived, ElementalTypes elementalTypes = 0 )
    {

        return healthReceived;

    }

    protected override void OnDeath( bool deathByFire = false )
    {

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

    protected override void ProcessElementalEffects( ElementalTypes elementalTypes, float doTValueIncrement = 0 )
    {

        if ( elementalTypes.HasFlag( ElementalTypes.Fire ) )
        {

            _fireDoTValue += doTValueIncrement;

            _fireDoTAction ??= StartCoroutine( ProcessDamageOverTime( 5f, true ) );

            _statusEffects |= StatusEffects.Burn;

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

            _statusEffects |= StatusEffects.Slowed;

        }

    }

    protected override IEnumerator UseAbility()
    {

        while ( true )
        {

            //Uncomment if ability has a range
            //yield return new WaitUntil(() => _receivingObjects.Count > 0);

            Debug.Log( $"Enemy of type: {GetType()} used ability" );
            yield return new WaitForSeconds( _abilityCooldown );

        }

    }

    protected override void Explode()
    {

        Collider2D[] results = Physics2D.OverlapCircleAll( transform.position, _deathExplosionRadius, LayerMask.GetMask( "Enemy" ) );

        foreach ( var collider in results )
        {

            if ( collider.TryGetComponent<Enemy>( out var enemy ) )
            {

                enemy.ReceiveAbility( _deathExplosionPower, ElementalTypes.Fire );

            }

        }

    }
}
