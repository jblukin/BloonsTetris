using UnityEngine;

[CreateAssetMenu( fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData" )]
public class EnemyData : ScriptableObject
{

    public float MaxHP, Power, Speed, Range, AbilityCooldown, AbilityDuration, DeathExplosionRadius, DeathExplosionPower;

    public int StartingWaypointIndex;

    public bool HasAbility;

    public ElementalTypes ElementalTypes;

    public ElementalResistances ElementalResistances;

    public Enemy.StatusEffects BaseStatusEffects;

}
