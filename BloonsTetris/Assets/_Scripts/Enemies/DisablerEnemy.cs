using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisablerEnemy : BaseEnemy
{

    private Tetrimino _currentDisabledTetrimino;

    private List<Tetrimino> _potentialTargets;

    private void Start()
    {

        _potentialTargets = new List<Tetrimino>();

    }

    protected override IEnumerator UseAbility()
    {

        while ( true )
        {

            yield return new WaitUntil( () => ( _receivingObjects.Count > 0 || _currentDisabledTetrimino != null ) );

            if ( _currentDisabledTetrimino != null )
            {

                _currentDisabledTetrimino.EnableAbility();

                Debug.Log( "Enabled: " + _currentDisabledTetrimino.name );

                _currentDisabledTetrimino = null;

            }

            if ( _potentialTargets.Count > 0 )
            {

                _currentDisabledTetrimino = _potentialTargets[ Random.Range( 0, _potentialTargets.Count ) ];

                _currentDisabledTetrimino.DisableAbility();

                Debug.Log( "Disabled: " + _currentDisabledTetrimino.name );

            }

            yield return new WaitForSeconds( _abilityCooldown );

        }

    }

    protected override void OnTriggerEnter2D( Collider2D collidingObject )
    {

        if ( collidingObject.transform.parent != null && collidingObject.transform.parent.TryGetComponent<Tetrimino>( out var tetrimino ) )
        {

            _receivingObjects.Add( collidingObject.transform.parent.gameObject );
            _potentialTargets.Add( tetrimino );

            //Apply any instant effects to receivingObject here

        }

    }

    protected override void OnTriggerExit2D( Collider2D collidingObject )
    {

        if ( collidingObject.transform.parent != null && collidingObject.transform.parent.TryGetComponent<Tetrimino>( out var tetrimino ) )
        {

            _receivingObjects.Remove( collidingObject.transform.parent.gameObject );
            _potentialTargets.Remove( tetrimino );


            //Remove any permanent/untimed applied effects from receivingObject here

        }

    }

}
