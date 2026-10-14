using UnityEngine;

[CreateAssetMenu(menuName = "Items/Usage/Ammo Box")]
public class AmmoBoxItem : ItemUsage
{
    private InventoryController _inventoryController => InventoryController.Instance;
    private PlayerCombatController _playerCombatController => PlayerCombatController.Instance;

    public string weaponId;
    public int amountOfMags = 3;

    public override void UseItem(Item item)
    {
        (bool result, IWeapon weapon) = _inventoryController.GetWeapon(weaponId);
        if (!result) return;

        if (weapon is Weapon rangeWeapon)
        {
            _playerCombatController.AddAmmo(weapon.item.id, amountOfMags * rangeWeapon.magazineSize);
        }
    }
}