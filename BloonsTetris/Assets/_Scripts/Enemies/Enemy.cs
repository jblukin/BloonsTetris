using System.Collections.Generic;
using System.Collections;
using System;
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

    protected float _maxHP, _currentHP, _power, _speed, _range, _abilityCooldown, _deathExplosionRadius, _deathExplosionPower, _poisonDoTValue, _fireDoTValue, _slowedPercentage;
    protected ElementalTypes _elementalTypes;
    protected ElementalResistances _elementalResistances;
    protected StatusEffects _statusEffects;
    protected Coroutine _poisonDoTAction, _fireDoTAction, _abilityAction;
    protected List<GameObject> _receivingObjects;

    public abstract void Init( BasicEnemyData enemyData );

    public abstract void ReceiveAbility( float amountReceived = 0, bool isPercentage = false, ElementalTypes elementalTypes = 0, bool isHealing = false );

    protected abstract void Move();

    protected abstract float CalculateFinalReceivedDamage( float damageReceived, ElementalTypes elementalTypes = 0 );

    protected abstract float CalculateFinalReceivedHealth( float healthReceived, ElementalTypes elementalTypes = 0 );

    protected abstract void ProcessElementalEffects( ElementalTypes elementalTypes, float doTValueIncrement = 0, float incomingSlowPercentage = 0 );

    protected abstract IEnumerator ProcessDamageOverTime( float tickRate, bool isFire );

    protected abstract IEnumerator UseAbility();

    public abstract void ClearStatusEffects( StatusEffects statusEffectsToClear );

    protected abstract void OnDeath( bool deathByFire = false );

    protected abstract void Explode();

}
