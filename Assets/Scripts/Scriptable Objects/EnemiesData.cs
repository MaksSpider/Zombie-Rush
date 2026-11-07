using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string id;
    public GameObject prefab;

    public int spawnDay;
    public float spawnChance;

    public List<EnemyAttributeType> attributes;

    public ScriptableObject weapon;

    public List<ModifierProperty> startingModifiers;
    public List<ModifierProperty> modifierPerDifficultyIncrease;

    public List<ItemDrop> loot;
}

public enum EnemyAttributeType
{
    SimpleMovement,
    CircleMovement,
    MeleeAttack,
    RangedAttack
}

[Serializable]
public struct ItemDrop
{
    public string itemId;
    public float chance;
    public Vector2Int amount;
}