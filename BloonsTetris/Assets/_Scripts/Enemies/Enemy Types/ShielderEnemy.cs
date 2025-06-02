using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShielderEnemy : BaseEnemy
{

    protected override void OnTriggerEnter2D( Collider2D collidingObject )
    {

        if ( collidingObject.TryGetComponent<Enemy>( out var enemy ) )
        {

            _receivingObjects.Add( collidingObject.gameObject );

            //Apply any instant effects to receivingObject here
            enemy.ApplyElementalResistances( ElementalResistances.MediumTypes );

        }

    }

    protected override void OnTriggerExit2D( Collider2D collidingObject )
    {

        if ( collidingObject.TryGetComponent<Enemy>( out var enemy ) )
        {

            _receivingObjects.Remove( collidingObject.gameObject );

            //Remove any permanent/untimed applied effects from receivingObject here
            enemy.ClearResistences( ElementalResistances.MediumTypes );

        }

    }

}
