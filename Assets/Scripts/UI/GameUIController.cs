using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIController : MonoBehaviour
{
    private const int LeaderboardRows = 5;

    [SerializeField] private ColorPalette palette;

    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject resultsPanel;

    [Header("Lobby")]
    [SerializeField] private TMP_Text lobbyPlayersText;
    [SerializeField] private Button startButton;

    [Header("HUD")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text lengthText;
    [SerializeField] private TMP_Text leaderboardText;

    [Header("Results")]
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private TMP_Text rankingText;
    [SerializeField] private TMP_Text nextRoundText;

    private void Awake()
    {
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void Update()
    {
        GameManager manager = GameManager.Instance;

        if (manager == null)
        {
            ShowPanels(true, false, false, false);
            return;
        }

        switch (manager.State)
        {
            case GameState.Lobby:
                ShowPanels(false, true, false, false);
                UpdateLobby(manager);
                break;
            case GameState.Playing:
                ShowPanels(false, false, true, false);
                UpdateHud(manager);
                break;
            case GameState.Results:
                ShowPanels(false, false, false, true);
                UpdateResults(manager);
                break;
        }
    }

    private void ShowPanels(bool menu, bool lobby, bool hud, bool results)
    {
        menuPanel.SetActive(menu);
        lobbyPanel.SetActive(lobby);
        hudPanel.SetActive(hud);
        resultsPanel.SetActive(results);
    }

    private void OnStartClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartRound();
        }
    }

    private void UpdateLobby(GameManager manager)
    {
        lobbyPlayersText.text = BuildRanking(NetworkWorm.ActiveWorms.Count, false);
        startButton.gameObject.SetActive(manager.IsHost);
        startButton.interactable = NetworkWorm.ActiveWorms.Count >= GameManager.MinPlayersToStart;
    }

    private void UpdateHud(GameManager manager)
    {
        int secondsLeft = Mathf.CeilToInt(manager.TimeLeft);
        timerText.text = $"{secondsLeft / 60}:{secondsLeft % 60:00}";

        NetworkWorm local = NetworkWorm.Local;

        if (local != null)
        {
            scoreText.text = local.Score.ToString();
            lengthText.text = local.SegmentCount.ToString();
        }

        leaderboardText.text = BuildRanking(LeaderboardRows, true);
    }

    private void UpdateResults(GameManager manager)
    {
        winnerText.text = FindPlayerName(manager.Winner);
        rankingText.text = BuildRanking(NetworkWorm.ActiveWorms.Count, true);
        nextRoundText.text = Mathf.CeilToInt(manager.TimeLeft).ToString();
    }

    private string BuildRanking(int maxRows, bool showScore)
    {
        StringBuilder builder = new StringBuilder();
        int rank = 1;

        foreach (NetworkWorm worm in NetworkWorm.ActiveWorms.OrderByDescending(w => w.Score).Take(maxRows))
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(palette.GetPlayerColor(worm.ColorIndex));
            string line = $"{rank}. <color=#{colorHex}>{worm.PlayerName}</color>";

            if (showScore)
            {
                line += $"  {worm.Score}";
            }

            builder.AppendLine(line);
            rank++;
        }

        return builder.ToString();
    }

    private string FindPlayerName(Fusion.PlayerRef player)
    {
        foreach (NetworkWorm worm in NetworkWorm.ActiveWorms)
        {
            if (worm.Object.InputAuthority == player)
            {
                return worm.PlayerName.ToString();
            }
        }

        return "-";
    }
}