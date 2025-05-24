using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyManager : MonoBehaviour
{

    [SerializeField]
    private List<BasicEnemyData> _basicEnemyDataObjs;
    [SerializeField]
    private Sprite _enemySprite;

    public void SpawnEnemy()
    {

        if ( _basicEnemyDataObjs.Count == 0 )
            return;

        GameObject gameObject = new();

        SpriteRenderer r = gameObject.AddComponent<SpriteRenderer>();

        r.sprite = _enemySprite;

        r.color = Color.red;   

        gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, 1 / GameManager.Instance.GridManager.CellSize ) * GameManager.Instance.GridManager.CellSize;

        gameObject.name = $"BaseEnemy{_basicEnemyDataObjs.Count}";

        gameObject.transform.position = GameManager.Instance.GridManager.EnemyPathWaypoints[ 0 ];

        BaseEnemy enemy = gameObject.AddComponent<BaseEnemy>();

        enemy.Init( Instantiate( _basicEnemyDataObjs[ 0 ] ) );

    }
}
