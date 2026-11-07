using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RangeWeapon", menuName = "Items/Weapons/Range")]
public class RangeWeaponData : ScriptableObject
{
    public Item item;
    public RangeWeapon weapon;
}

public interface IWeapon
{
    int damage { get; set; }
    float attackRate { get; set; }
    float attackRange { get; set; }
    float attackAngle { get; set; }
    float attackSpeed { get; set; }
    bool multiTargeting { get; set; }

    Sprite weaponUiSprite { get; set; }
}

[Serializable]
public class RangeWeapon: IWeapon
{
    [Header("Weapon Settings")]
    [field: SerializeField] public int damage { get; set; }
    [field: SerializeField] public float attackRate { get; set; }
    [field: SerializeField] public float attackRange { get; set; }
    [field: SerializeField] public float attackAngle { get; set; }
    [field: SerializeField] public int bulletsPerShot { get; set; }
    [field: SerializeField] public float accuracy { get; set; }

    [field: SerializeField] public int magazineSize { get; set; }
    [field: SerializeField] public float reloadTime { get; set; }

    [field: SerializeField] public AudioClip attackSound { get; set; }

    [Header("Bullet Settings")]
    [field: SerializeField] public GameObject bulletGameObject { get; set; }
    [field: SerializeField] public float attackSpeed { get; set; }
    [field: SerializeField] public float bulletSpawnOffset { get; set; }
    [field: SerializeField] public float bulletLifeTime { get; set; }
    [field: SerializeField] public bool multiTargeting { get; set; }

    [Header("UI Settings")]
    [field: SerializeField] public Sprite weaponUiSprite { get; set; }
    [field: SerializeField] public Sprite bulletUiSprite { get; set; }
}
