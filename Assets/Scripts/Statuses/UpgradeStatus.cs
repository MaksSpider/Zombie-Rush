using System.Collections.Generic;

public class UpgradeStatus : Status
{
    private PlayerController _playerController => PlayerController.Instance;

    public string upgradeId;

    private Modifier modifier;

    private List<ModifierProperty> modifiers;

    public UpgradeStatus(string upgradeId, List<ModifierProperty> modifiers)
    {
        this.upgradeId = upgradeId;
        this.modifiers = modifiers;
    }

    public override void OnAdd()
    {
        modifier = _statusesController.gameObject
            .GetComponent<PlayerController>()
            .modifiersController
            .AddModifier(modifiers);
    }

    public override void OnRemove()
    {
        _playerController.upgradesController.RemoveUpgrade(upgradeId, false);
        modifier.Remove();
    }
}
