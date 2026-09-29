using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradesData", menuName = "Scriptable Objects/UpgradesData")]
public class UpgradesData : ScriptableObject
{
    public List<Upgrade> upgradesData;
}

[Serializable]
public struct Bonus
{
    public string name;
    public float value;
}

[Serializable]
public struct Upgrade
{
    [field: SerializeField]
    public String name { get; set; }
    [field: SerializeField]
    public String title { get; set; }
    [field: SerializeField]
    public String description { get; set; }
    [field: SerializeField]
    public String type { get; set; }

    [field: SerializeField]
    public List<Bonus> bonuses { get; set; }

    [field: SerializeField]
    public Sprite icon { get; set; }
}