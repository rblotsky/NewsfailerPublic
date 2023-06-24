using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuUI : UIPanelBase
{
    // DATA //
    // UI References
    [SerializeField] private TextMeshProUGUI highScoreText;


    // FUNCTIONS //
    // Unity Events
    private void OnEnable()
    {
        // Refreshes UI with high score
        highScoreText.SetText(FindObjectOfType<Game>().HighScore.ToString());
    }


    // UI Events
    public void HandleStartGame()
    {
        // Opens the game panel and sets up the game
        UIManager.Instance.GoToGamePanel(gameObject);
    }

    public void HandleQuitGame()
    {
        Application.Quit();
    }
}
