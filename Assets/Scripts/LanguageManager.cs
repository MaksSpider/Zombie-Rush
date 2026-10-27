using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using ZombieRush.Framework;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    private Dictionary<string, Dictionary<string, string>> languages;

    private string currentLanguage = "en";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        //JsonFile[] allLanguages = Resources.LoadAll<JsonFile>("Items");
    }

    public void ChangeLanguage(string name)
    {
        if (!languages.ContainsKey(name)) return;

        currentLanguage = name;
    }

    public string GetText(string key)
    {
        if (!languages[currentLanguage].ContainsKey(key)) return key;

        return languages[currentLanguage][key];
    }
}
