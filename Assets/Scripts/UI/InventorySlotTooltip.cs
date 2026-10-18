using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [NonSerialized] public int slotIndex = 0;

    private InventoryController _inventoryController => InventoryController.Instance;
    private TooltipController _tooltipController => TooltipController.Instance;

    public void OnPointerEnter(PointerEventData eventData)
    {
        (bool empty, IItem item) = _inventoryController.GetItemOnSlot(slotIndex);
        if (empty) return;

        string description = item.description;

        (bool isWeapon, IWeapon weapon, _) = _inventoryController.GetWeapon(item.id);

        if (isWeapon)
        {
            if (weapon is MeleeWeapon meleeWeapon)
            {
                description += $"\nDamage: {meleeWeapon.damage}\nAttack Rate: {meleeWeapon.attackRate}\nAttack Range: {meleeWeapon.attackRange}\nAttack Radius: {meleeWeapon.attackAngle}\nMulti Targeting: {meleeWeapon.multiTargeting}";
            }
            else if (weapon is RangeWeapon normalWeapon)
            {
                description += $"\nDamage: {normalWeapon.damage}\nAttack Rate: {normalWeapon.attackRate}\nBullets Per Shot: {normalWeapon.bulletsPerShot}\nAccuracy: {normalWeapon.accuracy}\nMagazine Size: {normalWeapon.magazineSize}\nReload Time: {normalWeapon.reloadTime}";
            }
        }

        _tooltipController.Show(item.name, description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltipController.Close();
    }
}
