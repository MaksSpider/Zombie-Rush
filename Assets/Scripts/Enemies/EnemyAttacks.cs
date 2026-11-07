using UnityEngine;

public class MeleeAttack: EnemyAttribute
{
    private float _attackTimer = 0f;

    public override void UpdateAttribute(float deltaTime)
    {
        if (self.weapon is not MeleeWeapon meleeWeapon) return;

        if (Vector2.Distance(self.playerController.transform.position, transform.position) > meleeWeapon.attackRange) return;

        _attackTimer -= deltaTime;
        if (_attackTimer > 0f) return;
        _attackTimer = meleeWeapon.attackRate;

        float multiplier = 1f
            + self.modifiersController.GetModifierValue("damage_multipler_modifier")
            + self.modifiersController.GetModifierValue("melee_damage_multipler_modifier");

        int damage = -Mathf.Abs(Mathf.RoundToInt(meleeWeapon.damage * multiplier));

        self.playerController.ChangeHealth(damage);
        self.animator.SetTrigger("Attack");

        if (self.TryGetComponent(out AudioSource audio) && meleeWeapon.attackSound != null)
        {
            audio.PlayOneShot(meleeWeapon.attackSound);
        }
    }
}

public class RangedAttack : EnemyAttribute
{
    private float _attackTimer = 0f;

    public override void UpdateAttribute(float deltaTime)
    {
        if (self.weapon is RangeWeapon normalWeapon)
        {
            if (Vector2.Distance(self.playerController.transform.position, gameObject.transform.position) > normalWeapon.attackRange) return;

            if (Time.time < _attackTimer) return;
            _attackTimer = Time.time + normalWeapon.attackRate;

            Vector2 direction = (self.playerController.transform.position - transform.position).normalized;

            LayerMask include = LayerMask.GetMask("Player");

            self.animator.SetTrigger("Attack");

            self.attacksManager.CreateBullet(gameObject.transform.position, direction, normalWeapon, new AttackOptions 
            { 
                damageModifer = (1f + self.modifiersController.GetModifierValue("damage_multipler_modifier") + self.modifiersController.GetModifierValue("bullets_damage_multipler_modifier")),
                targets = include,
            });
            self.gameObject.GetComponent<AudioSource>().PlayOneShot(normalWeapon.attackSound);
        }
    }
}