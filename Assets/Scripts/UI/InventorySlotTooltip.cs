using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [NonSerialized] public int slotIndex = 0;

    private InventoryController _inventoryController => InventoryController.Instance;
    private TranslationManager _translationManager => TranslationManager.Instance;
    private TooltipController _tooltipController => TooltipController.Instance;

    public void OnPointerEnter(PointerEventData eventData)
    {
        (bool empty, IItem item) = _inventoryController.GetItemOnSlot(slotIndex);
        if (empty) return;

        string description = _translationManager.GetText(item.id + ".desc");

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

        _tooltipController.Show(_translationManager.GetText(item.id + ".name"), description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _tooltipController.Close();
    }
}
