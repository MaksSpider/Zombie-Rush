using System.Collections.Generic;

public class HealingStatus : Status
{
    private Modifier modifier;

    private int maxHealth = 0;
    private int healthRegeneration = 0;

    public HealingStatus(int maxHealth, int healthRegeneration)
    {
        this.maxHealth = maxHealth;
        this.healthRegeneration = healthRegeneration;
    }

    public override void OnAdd()
    {
        modifier = _statusesController.gameObject
            .GetComponent<PlayerController>()
            .modifiersController
            .AddModifier(new List<ModifierProperty>
            {
                new ModifierProperty
                {
                    name = "base_max_health",
                    value = maxHealth
                },
                new ModifierProperty
                {
                    name = "base_health_regeneration",
                    value = healthRegeneration
                }
            });
    }

    public override void OnRemove()
    {
        modifier.Remove();
    }
}
