using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponsData", menuName = "Scriptable Objects/Weapons")]
public class WeaponsData : ScriptableObject
{
    public List<Weapon> weaponsData;
}

public interface IWeapon
{
    int itemId { get; set; }
    int damage { get; set; }
    float attackRate { get; set; }
}

[Serializable]
public class Weapon: IWeapon
{
    [field: SerializeField]
    public int itemId { get; set; }

    [Header("Weapons Settings")]
    [field: SerializeField]
    public int damage { get; set; }
    [field: SerializeField]
    public float attackRate { get; set; }
    [field: SerializeField]
    public int bulletsPerShot { get; set; }
    [field: SerializeField]
    public float accuracy { get; set; }

    [field: SerializeField]
    public int ammoId { get; set; }
    [field: SerializeField]
    public int magazineSize { get; set; }
    [field: SerializeField]
    public float reloadTime { get; set; }

    [Header("Bullet Settings")]
    [field: SerializeField]
    public Sprite bulletSprite { get; set; }
    [field: SerializeField]
    public float bulletSpeed { get; set; }
    [field: SerializeField]
    public float bulletSize { get; set; }
    [field: SerializeField]
    public float bulletSpawnOffset { get; set; }
    [field: SerializeField]
    public float bulletLifeTime { get; set; }

    [Header("UI Settings")]
    [field: SerializeField]
    public Sprite weaponUiSprite { get; set; }
    [field: SerializeField]
    public Sprite bulletUiSprite { get; set; }
}

