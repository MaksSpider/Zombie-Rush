using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/UsageFunctions/Healing")]
public class HealingItem : ItemUsage
{
    private PlayerController _playerController => PlayerController.Instance;

    public int health = 0;
    public int maxHealth = 0;
    public int regeneration = 0;

    public float duration = 0f;

    public override void UseItem(UsableItem item)
    {
        _playerController.ChangeHealth(health);

        if (duration > 0)
        {
            _playerController.statusesController.AddStatus(new HealingStatus(maxHealth, regeneration),duration, item.icon, new Color(255, 0, 0));
            return;
        }

        _playerController.modifiersController.AddModifier(new List<ModifierProperty>
        {
            new ModifierProperty
            {
                name = "base_max_health",
                value = maxHealth
            },
            new ModifierProperty
            {
                name = "base_health_regeneration",
                value = regeneration
            }
        });
    }
}