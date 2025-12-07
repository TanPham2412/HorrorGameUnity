using System.Collections.Generic;
using UnityEngine;

public class StoryFlagManager : MonoBehaviour
{
    private static readonly HashSet<string> flags = new();

    public static void SetFlag(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;
        flags.Add(key);
        Debug.Log($"StoryFlagManager: set {key}");
    }

    public static bool HasFlag(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return false;
        return flags.Contains(key);
    }
}