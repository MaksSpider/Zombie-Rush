using System;
using UnityEngine;
using ZombieRush.Framework;

public class PlayerProfile
{
    public string nickname;
    public int score;
    public int seed;
    public int kills;
}

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    [NonSerialized] public PlayerProfile playerProfile = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Setup(string nickname, int seed)
    {
        playerProfile.nickname = nickname;
        playerProfile.seed = seed;
        playerProfile.score = 0;
        playerProfile.kills = 0;
    }

    public void Save()
    {
        JsonFile.Save(playerProfile.nickname + "_" + DateTime.Now.ToString("dd.MM.yyyy_HH-mm-ss"), "scoreboard", playerProfile);
    }
}