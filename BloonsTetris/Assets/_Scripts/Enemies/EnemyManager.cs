using System;
using System.Collections.Generic;
using System.Linq;
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

        Debug.Log( enemyType.ToString() );

        if ( _enemyDataObjs.Count == 0 )
            return;

        GameObject gameObject = new();

        SpriteRenderer r = gameObject.AddComponent<SpriteRenderer>();

        r.sprite = _enemySprite;

        if ( enemyType == typeof( BaseEnemy ) )
        {

            r.color = Color.red;

        }
        else if ( enemyType == typeof( EnchanterEnemy ) )
        {

            r.color = Color.green;

        }

        gameObject.transform.localScale = new Vector3( gameObject.transform.localScale.x, gameObject.transform.localScale.y, 1 / GameManager.Instance.GridManager.CellSize ) * GameManager.Instance.GridManager.CellSize;

        gameObject.name = $"BaseEnemy{AllEnemies.Count + 1}";

        gameObject.transform.position = GameManager.Instance.GridManager.EnemyPathWaypoints[ 0 ];

        Enemy enemy = gameObject.AddComponent( enemyType ) as Enemy;

        enemy.Init( Instantiate( _enemyDataObjs.Find( x => x.name.Contains( enemyType.Name ) ) ) );

        _allEnemies.Add( enemy );

    }
}
