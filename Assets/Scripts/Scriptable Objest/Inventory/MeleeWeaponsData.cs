using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MeleeWeaponsData", menuName = "Scriptable Objects/MeleeWeapons")]
public class MeleeWeaponsData : ScriptableObject
{
    public List<MeleeWeapon> meleeWeapons;
}

[Serializable]
public class MeleeWeapon: IWeapon
{
    [field: SerializeField] public Item item { get; set; }

    [Header("Weapon Settings")]
    [field: SerializeField] public int damage { get; set; }
    [field: SerializeField] public float attackRate { get; set; }
    [field: SerializeField] public float attackRange { get; set; }
    [field: SerializeField] public float attackAngle { get; set; }
    [field: SerializeField] public bool multiTargeting { get; set; }

    [field: SerializeField] public AudioClip attackSound { get; set; }

    [Header("UI Settings")]
    [field: SerializeField] public Sprite weaponUiSprite { get; set; }
}

