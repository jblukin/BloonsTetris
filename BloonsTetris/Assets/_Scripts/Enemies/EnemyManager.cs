using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class EnemyManager : MonoBehaviour
{

    [SerializeField]
    private List<EnemyData> _enemyDataObjs;
    [SerializeField]
    private Sprite _enemySprite;

    private HashSet<Enemy> _allEnemies;
    public HashSet<Enemy> AllEnemies => _allEnemies;

    private void Start()
    {

        _allEnemies = new HashSet<Enemy>();

    }

    public void SpawnEnemy( Type enemyType )
    {

        if ( _enemyDataObjs.Count == 0 )
            return;

        GameObject gameObject = new();

        SpriteRenderer r = gameObject.AddComponent<SpriteRenderer>();

        r.sprite = _enemySprite;

        Enemy enemy = null;

        if ( enemyType == typeof( BaseEnemy ) )
        {

            r.color = Color.red;

            enemy = gameObject.AddComponent<BaseEnemy>();

        }
        else if ( enemyType == typeof( EnchanterEnemy ) )
        {

            r.color = Color.green;

            enemy = gameObject.AddComponent<EnchanterEnemy>();

        }
        else if ( enemyType == typeof( DisablerEnemy ) )
        {

            r.color = Color.blue;

            enemy = gameObject.AddComponent<DisablerEnemy>();

        }
        else if ( enemyType == typeof( RunnerEnemy ) )
        {

            r.color = Color.yellow;

            enemy = gameObject.AddComponent<RunnerEnemy>();

        }
        else if ( enemyType == typeof( BomberEnemy ) )
        {

            r.color = Color.cyan;

            enemy = gameObject.AddComponent<BomberEnemy>();

        }
        else if ( enemyType == typeof( ShielderEnemy ) )
        {

            r.color = Color.magenta;

            enemy = gameObject.AddComponent<ShielderEnemy>();

        }

        if ( enemy == null )
            throw new Exception( "Attempted to Spawn Invalid Enemy Type (Enemy Type may not exist yet)" );

        gameObject.transform.localScale = new Vector3( gameObject.transform.localScale.x, gameObject.transform.localScale.y, 1 / GameManager.Instance.GridManager.CellSize ) * GameManager.Instance.GridManager.CellSize;

        gameObject.name = $"{enemyType.Name}{AllEnemies.Count + 1}";

        gameObject.transform.position = GameManager.Instance.GridManager.EnemyPathWaypoints[ 0 ];

        enemy.Init( Instantiate( _enemyDataObjs.Find( x => x.name.Contains( enemyType.Name ) ) ) );

        _allEnemies.Add( enemy );

    }
}
