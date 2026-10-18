using System;
using UnityEngine;


[CreateAssetMenu(fileName = "MeleeWeapon", menuName = "Items/Weapons/Melee")]
public class MeleeWeaponData : ScriptableObject
{
    public Item item;
    public MeleeWeapon weapon;
}

[Serializable]
public class MeleeWeapon : IWeapon
{

    [Header("Weapon Settings")]
    [field: SerializeField] public int damage { get; set; }
    [field: SerializeField] public float attackRate { get; set; }
    [field: SerializeField] public float attackRange { get; set; }
    [field: SerializeField] public float attackAngle { get; set; }
    [field: SerializeField] public float attackSpeed { get; set; }
    [field: SerializeField] public bool multiTargeting { get; set; }

    [field: SerializeField] public AudioClip attackSound { get; set; }

    [Header("UI Settings")]
    [field: SerializeField] public Sprite weaponUiSprite { get; set; }
}

