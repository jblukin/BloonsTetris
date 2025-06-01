using UnityEngine;

public class BomberEnemy : BaseEnemy
{

    private float _speedBoostHPTriggerValue;

    private void Start()
    {

        _speedBoostHPTriggerValue = _maxHP * 0.15f;

    }

    protected override void Update()
    {

        if ( _currentHP <= _speedBoostHPTriggerValue )
        {

            _speed += _power;

            _speedBoostHPTriggerValue = 0;

        }

        ReceiveDamageOrHealth( 0.1f );

        base.Update();

    }

    protected override void OnDeath( bool deathByFire = false )
    {

        base.OnDeath( true );

    }

}
