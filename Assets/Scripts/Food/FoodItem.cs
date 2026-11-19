using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class FoodItem : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameVisuals visuals;
    [SerializeField] private ColorPalette palette;

    [Networked] public int SpriteIndex { get; set; }
    [Networked] public int ColorIndex { get; set; }

    public override void Spawned()
    {
        spriteRenderer.sprite = visuals.GetFoodSprite(SpriteIndex);
        spriteRenderer.color = palette.GetPlayerColor(ColorIndex);
    }
}