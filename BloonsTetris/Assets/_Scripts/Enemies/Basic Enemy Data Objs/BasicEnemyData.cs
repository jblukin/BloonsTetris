using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class BasicEnemyData : ScriptableObject
{

    public float MaxHP, Power, Speed, Range, AbilityCooldown, DeathExplosionRadius, DeathExplosionPower;

    public bool HasAbility;

    public ElementalTypes ElementalTypes;

    public ElementalResistances ElementalResistances;

    public Enemy.StatusEffects BaseStatusEffects;

}
