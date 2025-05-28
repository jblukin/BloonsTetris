using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnchanterEnemy : BaseEnemy
{

    private HashSet<Enemy> _enchantedEnemies;

    private void Start()
    {

        _enchantedEnemies = new HashSet<Enemy>();

    }

    protected override IEnumerator UseAbility()
    {

        while ( true )
        {

            yield return new WaitUntil( () => _receivingObjects.Count > 0 );

            foreach ( var obj in _receivingObjects )
            {

                if ( obj.TryGetComponent<Enemy>( out var enemy ) )
                {

                    if ( _enchantedEnemies.Add( enemy ) )
                    {

                        enemy.ApplyElementalResistances( ElementalResistances.LowTypes );
                        StartCoroutine( nameof( RemoveResistences ), enemy );

                    }

                }

            }

            //Debug.Log( $"Enemy of type {GetType()} used ability" );

            yield return new WaitForSeconds( _abilityCooldown );

        }

    }

    protected override void OnTriggerEnter2D( Collider2D collidingObject )
    {

        if ( collidingObject.TryGetComponent<Enemy>( out _ ) )
        {

            _receivingObjects.Add( collidingObject.gameObject );

            //Apply any instant effects to receivingObject here

        }

    }

    protected override void OnTriggerExit2D( Collider2D collidingObject )
    {

        if ( collidingObject.TryGetComponent<Enemy>( out _ ) )
        {

            _receivingObjects.Remove( collidingObject.gameObject );

            //Remove any permanent/untimed applied effects from receivingObject here

        }

    }

    private IEnumerator RemoveResistences( Enemy enemy )
    {

        yield return new WaitForSeconds( _abilityDuration );

        if ( !_receivingObjects.Contains( enemy.gameObject ) )
        {

            enemy.ClearResistences( ElementalResistances.LowTypes );

            _enchantedEnemies.Remove( enemy );

        }
        else
        {

            StartCoroutine( nameof( RemoveResistences ), enemy );

        }    

    }

}
