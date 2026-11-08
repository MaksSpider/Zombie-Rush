using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

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
    public Rarity rarity { get; set; }

    [field: SerializeField]
    public List<ModifierProperty> modifiers { get; set; }

    [field: SerializeField]
    public Sprite icon { get; set; }

    [NonSerialized] public UpgradeStatus status;

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
                return (0.65f, 1.05f);
            case Rarity.Rare:
                return (0.25f, 1.15f);
            case Rarity.Epic:
                return (0.075f, 1.2f);
            case Rarity.Legendary:
                return (0.025f, 1.25f);
        }

        return (0f, 0f);
    }

    public string GetName()
    {
        return TranslationManager.Instance.GetText(id + ".name");
    }

    public string GetDesc()
    {
        StringBuilder builder = new StringBuilder();

        foreach (ModifierProperty modifier in modifiers)
        {
            builder.AppendLine(string.Format(TranslationManager.Instance.GetText(modifier.name + ".display"), modifier.value));
        }

        return builder.ToString().TrimEnd();
    }
}