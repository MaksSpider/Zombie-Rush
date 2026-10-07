using UnityEngine;

public class GameSession : MonoBehaviour
{
    public static GameSession Instance { get; private set; }

    public string nickname { get; private set; }
    public int seed { get; private set; }
    public int score { get; set; }

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
        this.nickname = nickname;
        this.seed = seed;
        score = 0;
    }
}