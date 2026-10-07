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
public struct EnemyData
{
    public string name;
    public GameObject prefab;

    public int spawnDay;
    public float spawnChance;

    public int health;
    public float speed;
    public int exp;

    public int weaponItemId;

    public List<EnemyAttributeType> attributes;
}