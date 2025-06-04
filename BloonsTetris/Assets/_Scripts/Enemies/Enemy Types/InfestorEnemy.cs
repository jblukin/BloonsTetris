using System;
using System.Collections;
using UnityEngine;

public class InfestorEnemy : BaseEnemy
{

    private EnemyManager _enemyManager;

    private EnemyData _baseEnemyData;

    private void Start()
    {

        _enemyManager = GameManager.Instance.EnemyManager;

        _enemyManager.ReceivingObjectDestroyed.AddListener( SpawnBugsOnNearbyDeath );

        _baseEnemyData = Instantiate( _enemyManager.EnemyDataObjs.Find( x => x.name.Contains( typeof( BaseEnemy ).Name ) ) );

        _baseEnemyData.Speed *= 2;

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

    private void SpawnBugsOnNearbyDeath( GameObject obj )
    {

        if ( obj == gameObject )
            return;

        Enemy enemy = obj.GetComponent<Enemy>();

        EnemyData bugEnemyData = Instantiate( _baseEnemyData );

        bugEnemyData.MaxHP = enemy.MaxHP;

        bugEnemyData.StartingWaypointIndex = enemy.CurrentWaypointIndex - 1;

        StartCoroutine( nameof( SpawnBugs ), new Tuple<Vector3, EnemyData>( enemy.transform.position, bugEnemyData ) );

    }

    private IEnumerator SpawnBugs( Tuple<Vector3, EnemyData> data )
    {

        for ( int i = 0; i < _power; i++ )
        {

            _enemyManager.SpawnEnemy<BaseEnemy>( data.Item2, data.Item1 );

            yield return new WaitForSeconds( 0.1f );

        }

        Destroy( data.Item2 );

    }

}
