using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerProfile
{
    private const string NameKey = "profile_name";
    private const string ShapeKey = "profile_shape";
    private const string ColorKey = "profile_color";
    private const string DefaultName = "Player";

    public static string Name
    {
        get => PlayerPrefs.GetString(NameKey, DefaultName);
        set => PlayerPrefs.SetString(NameKey, string.IsNullOrWhiteSpace(value) ? DefaultName : value.Trim());
    }

    public static int ShapeIndex
    {
        get => PlayerPrefs.GetInt(ShapeKey, 0);
        set => PlayerPrefs.SetInt(ShapeKey, value);
    }

    public static int ColorIndex
    {
        get => PlayerPrefs.GetInt(ColorKey, 0);
        set => PlayerPrefs.SetInt(ColorKey, value);
    }
}