using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WormShape
{
    [SerializeField] private Sprite head;
    [SerializeField] private Sprite body;

    public Sprite Head => head;
    public Sprite Body => body;
}

[CreateAssetMenu(fileName = "GameVisuals", menuName = "Game/Game Visuals")]
public class GameVisuals : ScriptableObject
{
    [SerializeField] private WormShape[] wormShapes;
    [SerializeField] private Sprite[] foodSprites;

    public int ShapeCount => wormShapes.Length;
    public int FoodSpriteCount => foodSprites.Length;

    public Sprite GetHeadSprite(int index)
    {
        return wormShapes[index % wormShapes.Length].Head;
    }

    public Sprite GetBodySprite(int index)
    {
        return wormShapes[index % wormShapes.Length].Body;
    }

    public Sprite GetFoodSprite(int index)
    {
        return foodSprites[index % foodSprites.Length];
    }
}