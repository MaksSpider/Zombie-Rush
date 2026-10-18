using UnityEngine;

[CreateAssetMenu(menuName = "Items/UsageFunctions/Ammo Box")]
public class AmmoBoxItem : ItemUsage
{
    private InventoryController _inventoryController => InventoryController.Instance;
    private PlayerCombatController _playerCombatController => PlayerCombatController.Instance;

    public string weaponId;
    public int amountOfMags = 3;

    public override void UseItem(UsableItem item)
    {
        (bool result, IWeapon weapon, IItem weaponItem) = _inventoryController.GetWeapon(weaponId);
        if (!result) return;

        if (weapon is RangeWeapon rangeWeapon)
        {
            _playerCombatController.AddAmmo(weaponItem.id, amountOfMags * rangeWeapon.magazineSize);
        }
    }
}