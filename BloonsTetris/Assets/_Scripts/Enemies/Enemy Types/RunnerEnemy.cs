using UnityEngine;

public class RunnerEnemy : BaseEnemy
{

    private float _speedBoostMultValue;

    private void Start()
    {

        _speedBoostMultValue = _speed * 0.25f;

    }

    protected override void Update()
    {
        
        base.Update();

        ReceiveDamageOrHealth( 0.01f );

    }

    protected override void Move()
    {

        float finalSpeed = _statusEffects.HasFlag( StatusEffects.Slowed ) ? _speed * ( 1 - _slowedPercentage ) : _speed;

        //Runner Ability
        finalSpeed += ( _speedBoostMultValue * ( 1 - ( _currentHP / _maxHP ) ) );

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

    }

}
