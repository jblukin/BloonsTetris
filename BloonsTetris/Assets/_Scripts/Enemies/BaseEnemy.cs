using System.Collections;
using UnityEngine;

public class BaseEnemy : Enemy
{

    private float _maxHP, _currentHP;
    private float _speed, _range;
    private float _doTValue;
    private ElementalTypes _elementalTypes;
    private ElementalResistances _elementalResistances;
    private StatusEffects _statusEffects;

    private Coroutine _damageOverTime;

    public override StatusEffects Statuses { get { return _statusEffects; } protected set { _statusEffects = value; } }

    bool onInit = false;

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
        _speed = enemyData.Speed;
        _range = enemyData.Range;
        _elementalTypes = enemyData.ElementalTypes;
        _elementalResistances = enemyData.ElementalResistances;
        _statusEffects = enemyData.BaseStatusEffects;
        _doTValue = 0;

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
        if ( elementalTypes != ElementalTypes.None )
            ProcessElementalEffects( elementalTypes );

    }

    protected override void Move()
    {
        //Set to follow path from Grid Data in use

        transform.position += _speed * Time.deltaTime * Vector3.left; 

    }

    protected override float CalculateFinalReceivedDamage( float damageReceived )
    {

        return damageReceived;

    }

    protected override float CalculateFinalReceivedHealth( float healthReceived )
    {

        return healthReceived;

    }

    protected override void OnDeath( bool deathByFire = false )
    {

        StopAllCoroutines();

        //Until Object Pooling
        Destroy( gameObject );

    }

    protected override IEnumerator ProcessDamageOverTime( float tickSpeed, bool isFire = false )
    {

        while ( _doTValue > 0 )
        {

            _currentHP -= _doTValue;

            _doTValue--;

            if ( _currentHP <= 0 )
            {
                OnDeath( isFire );
                yield break;
            }

            yield return new WaitForSeconds( tickSpeed );

        }

    }

    protected override void ProcessElementalEffects( ElementalTypes elementalTypes )
    {

        if ( elementalTypes.HasFlag( ElementalTypes.Fire | ElementalTypes.Poison ) )
        {

            if ( _damageOverTime != null )
            {

                _doTValue += 5f;

            }
            else
            {

                bool isFire = elementalTypes.HasFlag( ElementalTypes.Fire );

                _damageOverTime = StartCoroutine( ProcessDamageOverTime( 5f, isFire ) );

                _statusEffects |= ( isFire ? StatusEffects.Burn : StatusEffects.Poisoned );

            }

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

        Debug.Log( $"Enemy of type: {GetType()} used ability" );
        yield return null;

    }

}
