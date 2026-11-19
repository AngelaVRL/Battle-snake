using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    private const int MaxNameLength = 12;

    [SerializeField] private PlayerSpawner spawner;
    [SerializeField] private GameVisuals visuals;
    [SerializeField] private ColorPalette palette;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_InputField roomInput;
    [SerializeField] private Image headPreview;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button prevShapeButton;
    [SerializeField] private Button nextShapeButton;
    [SerializeField] private Button prevColorButton;
    [SerializeField] private Button nextColorButton;

    private void Awake()
    {
        nameInput.characterLimit = MaxNameLength;
        nameInput.text = PlayerProfile.Name;

        hostButton.onClick.AddListener(OnHostClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
        prevShapeButton.onClick.AddListener(() => ChangeShape(-1));
        nextShapeButton.onClick.AddListener(() => ChangeShape(1));
        prevColorButton.onClick.AddListener(() => ChangeColor(-1));
        nextColorButton.onClick.AddListener(() => ChangeColor(1));

        RefreshPreview();
    }

    private void OnHostClicked()
    {
        PlayerProfile.Name = nameInput.text;
        spawner.StartHost(roomInput.text);
    }

    private void OnJoinClicked()
    {
        PlayerProfile.Name = nameInput.text;
        spawner.JoinGame(roomInput.text);
    }

    private void ChangeShape(int step)
    {
        PlayerProfile.ShapeIndex = Wrap(PlayerProfile.ShapeIndex + step, visuals.ShapeCount);
        RefreshPreview();
    }

    private void ChangeColor(int step)
    {
        PlayerProfile.ColorIndex = Wrap(PlayerProfile.ColorIndex + step, palette.PlayerColorCount);
        RefreshPreview();
    }

    private void RefreshPreview()
    {
        headPreview.sprite = visuals.GetHeadSprite(PlayerProfile.ShapeIndex);
        headPreview.color = palette.GetPlayerColor(PlayerProfile.ColorIndex);
    }

    private static int Wrap(int value, int count)
    {
        return (value % count + count) % count;
    }
}