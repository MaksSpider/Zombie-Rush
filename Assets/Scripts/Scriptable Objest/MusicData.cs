using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MusicData", menuName = "Scriptable Objects/MusicData")]
public class MusicData : ScriptableObject
{
    public List<AudioData> music;
    public List<AudioData> sfx;
}

[Serializable]
public struct AudioData
{
    public string name;
    public float volume;
    public AudioClip audio;
}
