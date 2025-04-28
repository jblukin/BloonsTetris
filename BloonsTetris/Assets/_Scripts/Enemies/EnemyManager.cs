using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyManager : MonoBehaviour
{

    [SerializeField]
    private List<BasicEnemyData> _basicEnemyDataObjs;

    public void SpawnEnemy()
    {

        if ( _basicEnemyDataObjs.Count == 0 )
            return;

        GameObject gameObject = new();

        BaseEnemy enemy = gameObject.AddComponent<BaseEnemy>();

        SpriteRenderer r = gameObject.AddComponent<SpriteRenderer>();

        r.sprite = default;

        gameObject.transform.localScale *= GameManager.Instance.GridManager.CellSize;

        gameObject.name = $"BaseEnemy{_basicEnemyDataObjs.Count}";

        gameObject.transform.position = new( 400f, 200f, 1f );

        enemy.Init( Instantiate( _basicEnemyDataObjs[ 0 ] ) );

    }
}
