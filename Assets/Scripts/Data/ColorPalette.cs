using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ColorPalette", menuName = "Game/Color Palette")]
public class ColorPalette : ScriptableObject
{
    [Header("Environment")]
    [SerializeField] private Color mapBackground;
    [SerializeField] private Color gridLines;
    [SerializeField] private Color boundary;
    [SerializeField] private Color boundaryGlow;

    [Header("Interface")]
    [SerializeField] private Color hudBackground;
    [SerializeField] private Color activeButton;
    [SerializeField] private Color text;
    [SerializeField] private Color alert;

    [Header("Players")]
    [SerializeField] private Color[] playerColors;

    public Color MapBackground => mapBackground;
    public Color GridLines => gridLines;
    public Color Boundary => boundary;
    public Color BoundaryGlow => boundaryGlow;
    public Color HudBackground => hudBackground;
    public Color ActiveButton => activeButton;
    public Color Text => text;
    public Color Alert => alert;
    public int PlayerColorCount => playerColors.Length;

    public Color GetPlayerColor(int index)
    {
        return playerColors[index % playerColors.Length];
    }
}