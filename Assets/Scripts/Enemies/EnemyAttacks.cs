using System;
using UnityEngine;

public class MeleeAttack: EnemyAttribute
{
    private float _attackTimer = 0f;

    public override void UpdateAttribute(float deltaTime)
    {
        if (self.weapon is MeleeWeapon meleeWeapon)
        {
            if (Vector2.Distance(self.target.position, gameObject.transform.position) > meleeWeapon.attackRange) return;

            _attackTimer -= deltaTime;

            if (_attackTimer > 0f) return;
            _attackTimer = meleeWeapon.attackRate;

            self.gameObject.GetComponent<AudioSource>().PlayOneShot(meleeWeapon.attackSound);
            self.playerController.OnDamageDealt(meleeWeapon.damage);
        }
    }
}

public class RangedAttack : EnemyAttribute
{
    private float _nextAttackTime = 0f;

    public override void UpdateAttribute(float deltaTime)
    {
        if (self.weapon is Weapon normalWeapon)
        {
            if (Vector2.Distance(self.target.position, gameObject.transform.position) > normalWeapon.attackRange) return;

            if (Time.time < _nextAttackTime) return;
            _nextAttackTime = Time.time + normalWeapon.attackRate;

            Vector2 direction = (self.target.position - transform.position).normalized;

            LayerMask include = LayerMask.GetMask("Player");

            self.attacksManager.CreateBullet(gameObject.transform.position, direction, normalWeapon, 30f, new Vector2(0, 0.15f), 1f, include);
            self.gameObject.GetComponent<AudioSource>().PlayOneShot(normalWeapon.attackSound);
        }
    }
}