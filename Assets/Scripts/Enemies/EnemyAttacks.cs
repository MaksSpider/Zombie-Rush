using System;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack: EnemyAttribute
{
    public int attackDamage = 2;
    public float attackCooldown = 1f;
    public float attackRange = 0.3f;

    private float _attackTimer = 0f;

    public override void UpdateAttribute(float deltaTime)
    {
        if (Vector2.Distance(self.target.position, gameObject.transform.position) > attackRange) return;

        _attackTimer -= deltaTime;

        if (_attackTimer > 0f) return;
        _attackTimer = attackCooldown;

        self.playerController.ChangeHealth(-attackDamage);
    }
}

public class RangedAttack : EnemyAttribute
{
    private float _nextAttackTime = 0f;
    private float attackRange = 8f;

    public Weapon weapon = new Weapon
    {
        damage = 5,
        attackRate = 2f,
        bulletsPerShot = 1,
        accuracy = 0.7f,
        bulletSpeed = 3f,
        bulletSpawnOffset = 0.5f,
        bulletLifeTime = 4f,
    };

    public override void UpdateAttribute(float deltaTime)
    {
        if (Vector2.Distance(self.target.position, gameObject.transform.position) > attackRange) return;

        if (Time.time < _nextAttackTime) return;
        _nextAttackTime = Time.time + weapon.attackRate;

        Vector2 direction = (self.target.position - transform.position).normalized;

        LayerMask include = LayerMask.GetMask("Player");

        self.attacksManager.CreateBullet(gameObject.transform.position, direction, weapon, 30f, include);
    }
}