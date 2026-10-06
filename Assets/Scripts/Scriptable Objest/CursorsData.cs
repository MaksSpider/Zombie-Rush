using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CursorsData", menuName = "Scriptable Objects/CursorsData")]
public class CursorsData : ScriptableObject
{
    public List<CursorData> cursors;
}

[Serializable]
public struct CursorData
{
    public string name;
    public Vector2 hotspot;
    public Texture2D texture;
}
