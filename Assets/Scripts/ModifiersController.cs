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
    public int id;
    public List<ModifierProperty> properties;

    private ModifiersController _modifiersController;

    public Modifier(int id, List<ModifierProperty> properties, ModifiersController modifiersController)
    {
        this.id = id;
        this.properties = properties;
        _modifiersController = modifiersController;
    }

    public void AddProperty(ModifierProperty property)
    {
        properties.Add(property);
        _modifiersController.ExecuteOnChange(property.name);
    }

    public void RemoveProperty(string name)
    {
        for(int i = 0; i < properties.Count; i++)
        {
            if (properties[i].name == name)
            {
                properties.RemoveAt(i);
                _modifiersController.ExecuteOnChange(name);
                return;
            }
        }
    }

    public void ChangePropertyValue(string name, float newValue)
    {
        foreach (ModifierProperty property in properties)
        {
            if (property.name == name)
            {
                property.value = newValue;
                _modifiersController.ExecuteOnChange(name);
                return;
            }
        }
    }

    public void Remove()
    {
        _modifiersController.RemoveModifier(id);
    }
}

public class ModifiersController : MonoBehaviour
{
    private int _nextId = 0;
    private Dictionary<string, List<Action>> OnModifiersChange = new();

    public List<Modifier> modifiers;

    public Modifier AddModifier(List<ModifierProperty> modifierProperties)
    {
        int id = _nextId;

        Modifier modifier = new Modifier(id, modifierProperties, this);

        modifiers.Add(modifier);

        foreach (ModifierProperty property in modifier.properties)
        {
            ExecuteOnChange(property.name);
        }

        _nextId++;

        return modifier;
    }

    public bool RemoveModifier(int id)
    {
        for (int i = 0; i < modifiers.Count; i++)
        {
            if (modifiers[i].id == id)
            {
                List<ModifierProperty> removedProperties = modifiers[i].properties;
                modifiers.RemoveAt(i);

                foreach (ModifierProperty property in removedProperties)
                {
                    ExecuteOnChange(property.name);
                }

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
            foreach(ModifierProperty property in modifier.properties)
            {
                if(property.name == name)
                {
                    value += property.value;
                }
            }
        }

        return value;
    }

    public void AddOnChangeListener(string name, Action action)
    {
        if (!OnModifiersChange.ContainsKey(name))
        {
            OnModifiersChange.Add(name, new List<Action>());
        }

        OnModifiersChange[name].Add(action);
    }

    public void AddOnChangeListener(List<string> names, Action action)
    {
        foreach (string name in names)
        {
            if (!OnModifiersChange.ContainsKey(name))
            {
                OnModifiersChange.Add(name, new List<Action>());
            }

            OnModifiersChange[name].Add(action);
        }
    }

    public void RemoveOnChangeListener(string name, Action action)
    {
        if (!OnModifiersChange.ContainsKey(name)) return;
        if (!OnModifiersChange[name].Contains(action)) return;

        OnModifiersChange[name].Remove(action);
    }

    public void RemoveOnChangeListener(List<string> names, Action action)
    {
        foreach(string name in names)
        {
            if (!OnModifiersChange.ContainsKey(name)) return;
            if (!OnModifiersChange[name].Contains(action)) return;

            OnModifiersChange[name].Remove(action);
        }
    }

    public void ExecuteOnChange(string name)
    {
        if(OnModifiersChange.ContainsKey(name))
        {
            foreach(Action action in OnModifiersChange[name])
            {
                action?.Invoke();
            }
        }
    }
}
