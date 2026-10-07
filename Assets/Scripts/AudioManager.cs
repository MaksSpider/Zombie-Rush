using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [SerializeField] private MusicData _musicData;

    private float _totalVolume = 1.0f;
    private float _sfxVolume = 1.0f;
    private float _musicVolume = 1.0f;

    private AudioData _currentSong;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(string name, bool loop = true)
    {
        AudioData audioData = GetMusic(name);

        _musicSource.volume = GetMusicVolume(audioData.volume);
        _musicSource.clip = audioData.audio;
        _musicSource.loop = loop;
        _musicSource.Play();

        _currentSong = audioData;
    }

    public void PlayRandom()
    {
        AudioData audioData = _musicData.music[Random.Range(0, _musicData.music.Count)];
        _musicSource.volume = GetMusicVolume(audioData.volume);
        _musicSource.clip = audioData.audio;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        _musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        _sfxSource.volume = GetSFXVolume(volume);
        _sfxSource.PlayOneShot(clip);
    }

    public void PlaySFX(string name)
    {
        AudioData audioData = GetSFX(name);
        _sfxSource.volume = GetSFXVolume(audioData.volume);
        _sfxSource.PlayOneShot(audioData.audio);
    }

    private float GetSFXVolume(float modifier = 1f)
    {
        return _totalVolume * _sfxVolume * modifier;
    }

    private float GetMusicVolume(float modifier = 1f)
    {
        return _totalVolume * _musicVolume * modifier;
    }

    private AudioData GetSFX(string name)
    {
        foreach(AudioData data in _musicData.sfx)
        {
            if(data.name == name) return data;
        }

        return new AudioData();
    }

    private AudioData GetMusic(string name)
    {
        foreach (AudioData data in _musicData.music)
        {
            if (data.name == name) return data;
        }

        return new AudioData();
    }
}