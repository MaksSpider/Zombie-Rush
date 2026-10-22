using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemiesData", menuName = "Scriptable Objects/EnemiesData")]
public class EnemiesData : ScriptableObject
{
    public List<EnemyData> enemies;
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
    public int itemId;
    public float chance;
    public Vector2Int amount;
}

[Serializable]
public struct EnemyData
{
    public string name;
    public GameObject prefab;

    public int spawnDay;
    public float spawnChance;

    public int health;
    public float speed;
    public float damageModifier;

    public int exp;
    public Vector2 moneyDrop;

    public string weaponItemId;

    public List<ItemDrop> loot;

    public List<EnemyAttributeType> attributes;
}