using System;
using System.Collections.Generic;
using UnityEngine;
using ZombieRush.Framework;

public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
}

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Scriptable Objects/UpgadeData")]
public class UpgradeData : ScriptableObject
{
    public List<Upgrade> upgrade;
}

[Serializable]
public class Upgrade
{
    [field: SerializeField]
    public string id { get; set; }
    [field: SerializeField]
    public string name { get; set; }
    [field: SerializeField]
    public string description { get; set; }
    [field: SerializeField]
    public Rarity rarity { get; set; }

    [field: SerializeField]
    public List<ModifierProperty> modifiers { get; set; }

    [field: SerializeField]
    public Sprite icon { get; set; }

    public Color GetRarityColor()
    {
        switch (rarity)
        {
            case Rarity.Common:
                return Color.white;
            case Rarity.Rare:
                return new Color(0.25f, 0.55f, 1.0f);
            case Rarity.Epic:
                return new Color(0.65f, 0.3f, 1.0f);
            case Rarity.Legendary:
                return new Color(1.0f, 0.65f, 0.1f);
        }

        return Color.white;
    }

    public (float baseChance, float chanceMultiplierPerLevel) GetUpgradeChance()
    {
        switch (rarity)
        {
            case Rarity.Common:
                return (0.5f, 1.05f);
            case Rarity.Rare:
                return (0.3f, 1.15f);
            case Rarity.Epic:
                return (0.15f, 1.2f);
            case Rarity.Legendary:
                return (0.05f, 1.25f);
        }

        return (0f, 0f);
    }
}