using System;
using System.Collections;
using UnityEngine;

[RequireComponent( typeof( CircleCollider2D ) )]
public abstract class Enemy : MonoBehaviour
{

    [Flags]
    public enum StatusEffects
    {

        None = 0,
        Marked = 1,
        Cursed = 2,
        Silenced = 4,
        Weakened = 8,
        Primed = 16,
        Burn = 32,
        Poisoned = 64,
        Infested = 128,
        Shielded = 256,
        Slowed = 512

    }

    public abstract StatusEffects Statuses { get; protected set; }

    public abstract void Init( BasicEnemyData enemyData );

    public abstract void ReceiveAbility( float amountReceived = 0, ElementalTypes elementalTypes = 0, bool isHealing = false );

    protected abstract void Move();

    protected abstract float CalculateFinalReceivedDamage( float damageReceived );

    protected abstract float CalculateFinalReceivedHealth( float healthReceived );

    protected abstract void ProcessElementalEffects( ElementalTypes elementalTypes );

    protected abstract IEnumerator ProcessDamageOverTime( float tickSpeed, bool isFire = false );

    protected abstract IEnumerator UseAbility();

    protected abstract void OnDeath( bool deathByFire = false );

}
