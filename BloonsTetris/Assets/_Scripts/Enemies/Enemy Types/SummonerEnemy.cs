using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SummonerEnemy : BaseEnemy
{

    private EnemyManager _enemyManager;

    private EnemyData _baseEnemyData;

    private void Start()
    {

        _enemyManager = GameManager.Instance.EnemyManager;

        _baseEnemyData = Instantiate( _enemyManager.EnemyDataObjs.Find( x => x.name.Contains( typeof( BaseEnemy ).Name ) ) );

    }

    protected override IEnumerator UseAbility()
    {

        while ( true )
        {

            //Uncomment if ability has a range
            //yield return new WaitUntil( () => _receivingObjects.Count > 0 );

            yield return new WaitForSeconds( _abilityCooldown );

            _baseEnemyData.StartingWaypointIndex = _currentWaypointIdx - 1;

            for ( int i = 0; i < (int)_power; i++ )
            {

                _enemyManager.SpawnEnemy<BaseEnemy>( _baseEnemyData );

                yield return new WaitForSeconds( 0.5f );

            }


        }

    }

}
