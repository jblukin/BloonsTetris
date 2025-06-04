using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class EnemyManager : MonoBehaviour
{

    [SerializeField]
    private List<EnemyData> _enemyDataObjs;
    public List<EnemyData> EnemyDataObjs { get { return _enemyDataObjs; } }

    [SerializeField]
    private Sprite _enemySprite;

    private HashSet<Enemy> _allEnemies;
    public HashSet<Enemy> AllEnemies => _allEnemies;

    [HideInInspector]
    public UnityEvent<GameObject> ReceivingObjectDestroyed;

    private void Start()
    {

        _allEnemies = new HashSet<Enemy>();

        ReceivingObjectDestroyed ??= new();

    }

    public T SpawnEnemy<T>() where T : BaseEnemy
    {

        if ( _enemyDataObjs.Count == 0 )
            return null;

        GameObject gameObject = new();

        Enemy enemy = gameObject.AddComponent( typeof( T ) ) as T;

        SpriteRenderer r = gameObject.AddComponent<SpriteRenderer>();

        r.sprite = _enemySprite;

        r.color = enemy switch
        {

            EnchanterEnemy => r.color = Color.green,
            DisablerEnemy => r.color = Color.blue,
            RunnerEnemy => r.color = Color.yellow,
            BomberEnemy => r.color = Color.cyan,
            ShielderEnemy => r.color = Color.magenta,
            SummonerEnemy => r.color = Color.black,
            BaseEnemy => r.color = Color.red,
            _ => r.color = Color.clear

        };

        if ( enemy == null || r.color == Color.clear )
            throw new Exception( "Attempted to Spawn Invalid Enemy Type (Enemy Type may not exist yet)" );

        gameObject.transform.localScale = new Vector3( gameObject.transform.localScale.x, gameObject.transform.localScale.y, 1 / GameManager.Instance.GridManager.CellSize ) * GameManager.Instance.GridManager.CellSize;

        gameObject.name = $"{enemy.GetType().Name}{AllEnemies.Count + 1}";

        gameObject.transform.position = GameManager.Instance.GridManager.EnemyPathWaypoints[ 0 ];

        enemy.Init( Instantiate( _enemyDataObjs.Find( x => x.name.Contains( enemy.GetType().Name ) ) ) );

        _allEnemies.Add( enemy );

        return (T)enemy;

    }

    public T SpawnEnemy<T>( EnemyData customData, Vector3 startingWorldPos = default ) where T : BaseEnemy
    {

        if ( _enemyDataObjs.Count == 0 )
            return null;

        GameObject gameObject = new();

        Enemy enemy = gameObject.AddComponent( typeof( T ) ) as T;

        SpriteRenderer r = gameObject.AddComponent<SpriteRenderer>();

        r.sprite = _enemySprite;

        r.color = enemy switch
        {

            EnchanterEnemy => r.color = Color.green,
            DisablerEnemy => r.color = Color.blue,
            RunnerEnemy => r.color = Color.yellow,
            BomberEnemy => r.color = Color.cyan,
            ShielderEnemy => r.color = Color.magenta,
            SummonerEnemy => r.color = Color.black,
            BaseEnemy => r.color = Color.red,
            _ => r.color = Color.clear

        };

        if ( enemy == null || r.color == Color.clear )
            throw new Exception( "Attempted to Spawn Invalid Enemy Type (Enemy Type may not exist yet)" );

        gameObject.transform.localScale = new Vector3( gameObject.transform.localScale.x, gameObject.transform.localScale.y, 1 / GameManager.Instance.GridManager.CellSize ) * GameManager.Instance.GridManager.CellSize;

        gameObject.name = $"{enemy.GetType().Name}{AllEnemies.Count + 1}";

        if(startingWorldPos != default)
        {

            gameObject.transform.position = startingWorldPos;

        } else
        {

            gameObject.transform.position = GameManager.Instance.GridManager.EnemyPathWaypoints[ customData.StartingWaypointIndex ];

        }

            enemy.Init( customData );

        _allEnemies.Add( enemy );

        return (T)enemy;

    }

}
