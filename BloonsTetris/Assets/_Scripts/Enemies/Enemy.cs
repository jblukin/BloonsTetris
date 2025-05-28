using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;

[RequireComponent( typeof( CircleCollider2D ) ), RequireComponent( typeof( Rigidbody2D ) ), DisallowMultipleComponent]
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
        Burnt = 32,
        Poisoned = 64,
        Infested = 128,
        Shielded = 256,
        Slowed = 512,
        All = Marked | Cursed | Silenced | Weakened | Primed | Burnt | Poisoned | Infested | Shielded | Slowed

    }

    protected float _maxHP, _currentHP, _power, _speed, _range, _abilityCooldown, _abilityDuration, _deathExplosionRadius, _deathExplosionPower, _poisonDoTValue, _fireDoTValue, _slowedPercentage, _pathTraversedPercentage, _distanceBetweenPrevCurrWaypoint;
    protected int _currentWaypointIdx;
    protected ElementalTypes _elementalTypes, _baseElementalTypes;
    protected ElementalResistances _elementalResistances, _baseElementalResistences;
    protected StatusEffects _statusEffects, _baseStatusEffects;
    protected Coroutine _poisonDoTAction, _fireDoTAction, _abilityAction;
    protected HashSet<GameObject> _receivingObjects;
    protected List<Vector2> _pathWaypoints;
    public abstract float PathTraversedPercetange { get; }

    protected abstract void Update();

    public abstract void Init( EnemyData enemyData );

    public abstract void ReceiveDamageOrHealth( float amountReceived, bool isPercentage = false, bool isHealing = false );

    public abstract void ApplyElementalEffects( ElementalTypes elementalTypes, float doTValueIncrement = 0, float incomingSlowPercentage = 0 );

    public abstract void ApplyElementalResistances( ElementalResistances elementalResistances );

    protected abstract void Move();

    protected abstract float CalculateFinalReceivedDamage( float damageReceived, bool isPercentage = false, ElementalTypes elementalTypes = 0 );

    protected abstract float CalculateFinalReceivedHealth( float healthReceived, bool isPercentage = false, ElementalTypes elementalTypes = 0 );

    protected abstract IEnumerator ProcessDamageOverTime( float tickRate, bool isFire );

    protected abstract IEnumerator UseAbility();

    protected abstract void OnTriggerEnter2D( Collider2D collidingObject );

    protected abstract void OnTriggerExit2D( Collider2D collidingObject );

    public abstract void ClearStatusEffects( StatusEffects statusEffectsToClear = StatusEffects.All );

    public abstract void ClearResistences( ElementalResistances elementalResistances = ElementalResistances.All );

    protected abstract void OnDeath( bool deathByFire = false );

    protected abstract void Explode();

}

public class PathTravelledComparer : IComparer<Enemy>
{

    public int Compare( Enemy x, Enemy y )
    {

        return y.PathTraversedPercetange.CompareTo( x.PathTraversedPercetange );

    }

}
