using UnityEngine.EventSystems;

public class UpgradeStatus : Status
{
    private TooltipController _tooltipController => TooltipController.Instance;
    private PlayerController _playerController => PlayerController.Instance;

    public Upgrade upgrade;

    private Modifier modifier;

    public UpgradeStatus(Upgrade upgrade)
    {
        this.upgrade = upgrade;
    }

    public override void OnAdd()
    {
        modifier = _statusesController.gameObject
            .GetComponent<PlayerController>()
            .modifiersController
            .AddModifier(upgrade.modifiers);

        gameObject.AddComponent<ToolTipDisplayer>()
            .SetTitle(upgrade.GetName())
            .SetDesc(upgrade.GetDesc());
    }

    public override void OnRemove()
    {
        _playerController.upgradesController.RemoveUpgrade(upgrade.id, false);
        modifier.Remove();
    }
}