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
            _playerController.AddTemporaryHealing(duration, maxHealth, regeneration, item.icon);
            return;
        }

        _playerController.ChangeMaxHealth(maxHealth);
        _playerController.ChangeRegeneration(regeneration);
    }
}