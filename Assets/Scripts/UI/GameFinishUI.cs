using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class GameFinishUI : UIPanelBase
{
    // DATA //
    // Scene References
    [SerializeField] private Game game;

    // UI References
    [SerializeField] private FinishedEditionsViewerUI editionsViewer;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI numEditionsFinishedText;
    [SerializeField] private TextMeshProUGUI gameFinishTypeText;
    [SerializeField] private GameObject newHighScoreText;

    // Scene Data
    [SerializeField] private string editionsRanOutTextContent;
    [SerializeField] private string timeRanOutTextContent;


    // FUNCTIONS //
    // Unity Events
    private void Awake()
    {
        Debug.Assert(game != null, "[GameFinishUI.Awake()] Was not given a Game!");
    }

    private void OnEnable()
    {
        // When the UI opens, we immediately refresh it with up-to-date data.
        SetupDisplayWithGameData();
    }


    // Data Acquisition
    private NewspaperEdition[] GetFinishedEditions()
    {
        // Gets the game's finished editions
        return game.HandleGetCompletedEditions();
    }

    private GameFinishType DetermineGameFinishType()
    {
        // If there are no editions left in the game, the game finish type is "ran out of editions"
        if (game.RemainingEditionsCount == 0) 
        { 
            return GameFinishType.EditionsRanOut; 
        }

        return GameFinishType.TimeRanOut;
    }


    // Internal UI Updating
    private void SetupDisplayWithGameData()
    {
        // Gets data from the last finished game
        NewspaperEdition[] editions = GetFinishedEditions();
        GameFinishType finishType = DetermineGameFinishType();
        int score = game.Score;
        int highScore = game.HighScore;
        bool newHighScore = (score > highScore);
        int numEditionsFinished = game.FinishedEditionsCount;

        // Refreshes the UI with this data
        gameFinishTypeText.SetText((finishType == GameFinishType.EditionsRanOut) ? editionsRanOutTextContent : timeRanOutTextContent);


        scoreText.SetText(score.ToString());
        highScoreText.SetText(highScore.ToString());
        newHighScoreText.SetActive(newHighScore);

        numEditionsFinishedText.SetText(numEditionsFinished.ToString());
        editionsViewer.OpenViewer(editions);
    }


    // UI Events
    public void HandleReturnToMenu()
    {
        // Updates the new high score if needed
        game.SaveHighScore();
        UIManager.Instance.GoToMenuPanel(gameObject);
    }
}
