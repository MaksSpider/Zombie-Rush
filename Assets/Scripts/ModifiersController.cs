using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ModifierProperty
{
    public string name;
    public float value;
}

[Serializable]
public class Modifier
{
    public int id {  get; private set; }
    public List<ModifierProperty> modifiers { get; set; }

    private ModifiersController _modifiersController;

    public Modifier(int id, List<ModifierProperty> modifiers, ModifiersController modifiersController)
    {
        this.id = id;
        this.modifiers = modifiers;
        _modifiersController = modifiersController;
    }

    public void Remove()
    {
        _modifiersController.RemoveModifier(id);
    }
}

public class ModifiersController : MonoBehaviour
{
    private int _nextId = 0;

    public List<Modifier> modifiers;

    public Modifier AddModifier(List<ModifierProperty> modifierProperties)
    {
        int id = _nextId;

        Modifier modifier = new Modifier(id, modifierProperties, this);

        modifiers.Add(modifier);

        _nextId++;

        return modifier;
    }

    public bool RemoveModifier(int id)
    {
        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i].id == id)
            {
                modifiers.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    public (bool result, Modifier modifier) GetModifier(int id)
    {
        foreach (Modifier modifier in modifiers)
        {
            if(modifier.id == id) return (true, modifier);
        }

        return (false, null);
    }

    public float GetModifierValue(string name)
    {
        float value = 0f;

        foreach(Modifier modifier in modifiers)
        {
            foreach(ModifierProperty property in modifier.modifiers)
            {
                if(property.name == name)
                {
                    value += property.value;
                }
            }
        }

        return value;
    }
}
