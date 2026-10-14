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
    Item item { get; set; }

    int damage { get; set; }
    float attackRate { get; set; }
    float attackRange { get; set; }
    float attackAngle { get; set; }

    Sprite weaponUiSprite { get; set; }
}

[Serializable]
public class Weapon: IWeapon
{
    [field: SerializeField] public Item item { get; set; }

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
    [field: SerializeField] public float bulletSpeed { get; set; }
    [field: SerializeField] public float bulletSpawnOffset { get; set; }
    [field: SerializeField] public float bulletLifeTime { get; set; }

    [Header("UI Settings")]
    [field: SerializeField] public Sprite weaponUiSprite { get; set; }
    [field: SerializeField] public Sprite bulletUiSprite { get; set; }
}

