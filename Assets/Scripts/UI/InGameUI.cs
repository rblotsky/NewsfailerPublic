using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Text;

public class InGameUI : UIPanelBase
{
    // DATA //
    // Scene References
    [Header("Game Reference")]
    [SerializeField] private Game game;
    [Header("Sound Effects")]
    [SerializeField] private AudioSource startCountdownFinishSound;
    [SerializeField] private AudioSource startCountdownIterationSound;
    [SerializeField] private AudioSource timerRunningOutSound;
    [SerializeField] private AudioSource gameEndSound;
    [SerializeField] private AudioSource moneyEarnSound;
    [SerializeField] private AudioSource musicSound;

    // Prefabs
    [Header("UI References")]
    [SerializeField] private GameObject fillableButtonPrefab;

    // UI References
    [SerializeField] private GameObject infoPromptPanel;
    [SerializeField] private TextMeshProUGUI segmentDisplayText;
    [SerializeField] private TextMeshProUGUI startCountdownText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button submitButton;
    [SerializeField] private RectTransform fillableButtonGrid;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI editionNameText;
    [SerializeField] private TextMeshProUGUI issueNumberText;

    // Cached Data
    private int currentSelectedOption = -1;
    private List<FillableButtonUI> fillableButtons = new List<FillableButtonUI>();
    private string cachedSegmentPrefix = string.Empty;
    private string cachedSegmentSuffix = string.Empty;

    // Constants
    private static readonly int[] wordLengthFreqs = { 1, 2, 2, 3, 3, 3, 3, 4, 4, 4, 5, 5, 5, 5, 5, 5, 5, 5, 5, 7, 7, 7, 7, 8, 8 };
    private static readonly int wordCountRangeStart = 3;
    private static readonly int wordCountRangeEnd = 15;
    private static readonly string REDACTED_TEXT_FONT = "Redacted-Regular SDF";


    // FUNCTIONS //
    // Unity Events
    private void Awake()
    {
        Debug.Assert(game != null, "[InGameUI.Awake()] Was not given a Game!");
    }

    private void OnEnable()
    {
        // Adds all event handlers to the game
        game.OnSetupFinish += OpenGameUI;
        game.OnScoreEarn += HandleEarnScore;
        game.OnNoEditionsLeft += HandleNoEditionsLeft;
        game.OnSegmentChange += RefreshEditionText;

        // Disables submit button
        ToggleSubmitButton(false);
    }

    private void OnDisable()
    {
        // Removes all event handlers from the game
        game.OnSetupFinish -= OpenGameUI;
        game.OnScoreEarn -= HandleEarnScore;
        game.OnNoEditionsLeft -= HandleNoEditionsLeft;
        game.OnSegmentChange -= RefreshEditionText;
    }


    // Game State Management
    private void OpenGameUI()
    {
        // Clears the UI first
        ClearDisplay();

        infoPromptPanel.SetActive(true);
    }

    private void StartGame()
    {
        // Opens up the input box and activates the timer
        StartCoroutine(GameTimer());
        DisplayNextSegment();
        RefreshEditionText();
        UpdateScoreDisplay(0);
        musicSound.Play();
    }

    private void FinishGame()
    {
        // Stops all coroutines on this object
        StopAllCoroutines();

        // Tells UI Manager to switch to the game finish panel
        UIManager.Instance.GoToFinishPanel(gameObject);
    }


    // UI Updating
    private void UpdateEditionText(string name, string issueNumber)
    {
        editionNameText.SetText(name);
        issueNumberText.SetText(issueNumber);
    }

    private void RefreshEditionText()
    {
        // Refreshes the display of the current edition if one exists
        string editionName = "";
        string issueNumber = "";
        if(game.RemainingEditionsCount > 0)
        {
            editionName = game.CurrentEdition.NewspaperName;
            issueNumber = game.CurrentEdition.IssueNumber;
        }

        UpdateEditionText(editionName, issueNumber);
    }

    private void UpdateScoreDisplay(int score)
    {
        scoreText.SetText($"${score}");
    }

    private void CreateFillableButtons()
    {
        // Throws an error if we still have existing buttons
        Debug.Assert(fillableButtons.Count == 0, "[InGameUI.CreateFillableButtons()] Still has preexisting fillable buttons!");

        // Gets the list of options, and creates buttons for each one
        List<string> numberedOptions = new List<string>(game.CurrentSegment.Options);
        List<string> options = new List<string>(game.CurrentSegment.Options);

        while(options.Count != 0)
        {
            string thisOption = options[Random.Range(0, options.Count - 1)];
            FillableButtonUI newButton = Instantiate(fillableButtonPrefab, fillableButtonGrid).GetComponent<FillableButtonUI>();
            newButton.Setup(this, numberedOptions.IndexOf(thisOption), thisOption);
            options.Remove(thisOption);
            fillableButtons.Add(newButton);
        }
    }

    private void DeleteFillableButtons()
    {
        // Destroys each fillableButton
        foreach(FillableButtonUI fillableButton in fillableButtons)
        {
            Destroy(fillableButton.gameObject);
        }

        // Then clears the list
        fillableButtons.Clear();
    }

    private void ToggleFillableButtons(bool status)
    {
        foreach(FillableButtonUI fillableButton in fillableButtons)
        {
            fillableButton.GetComponent<Button>().interactable = status;
        }
    }

    private void ToggleInfoPrompt(bool status)
    {
        // If enabling, also hide all other UI elements
        if(status)
        {
            infoPromptPanel.SetActive(true);
        }
        else
        {
            infoPromptPanel.SetActive(false);
        }
    }

    private void ToggleSubmitButton(bool status)
    {
        submitButton.interactable = status;
    }

    private void UpdateSegmentDisplay(int selectedOption)
    {
        string segmentText = game.CurrentSegment.GetTextWithOption(selectedOption, true, game.CurrentEdition.NounTable);
        string textWithFiller = AddFillerTextAroundSegment(segmentText);
        segmentDisplayText.SetText(textWithFiller);
    }

    private void ClearDisplay()
    {
        segmentDisplayText.SetText("");
        DeleteFillableButtons();
        UpdateTimer(game.TimeLimitSeconds);
        UpdateScoreDisplay(0);
        ToggleSubmitButton(false);
        UpdateEditionText("", "");
    }

    private void DisplayNextSegment()
    {
        // Resets the cached segment prefix/suffix text
        cachedSegmentPrefix = string.Empty;
        cachedSegmentSuffix = string.Empty;

        // Updates the text to display the next segment and creates
        // new option buttons
        UpdateSegmentDisplay(currentSelectedOption);
        CreateFillableButtons();

        // Disables submit button
        ToggleSubmitButton(false);
    }


    private void UpdateTimer(int secondsRemaining)
    {
        // Splits into minutes and seconds, then displays
        int minutes = secondsRemaining / 60;
        int seconds = secondsRemaining % 60;

        timerText.SetText(string.Format("{0:00}:{1:00}", minutes, seconds));

        // If time remaining <= 5, run time running out sound
        if(secondsRemaining <=5 )
        {
            timerRunningOutSound?.Play();
        }
    }

    private void UpdateCountdown(int secondsRemaining)
    {
        ToggleCountdownText(true);
        Debug.Log("secondsRemaining=" + secondsRemaining);
        startCountdownText.SetText(secondsRemaining.ToString());

        // If seconds remaining is 0, play the start sound. Otherwise, regular sound.
        if(secondsRemaining == 0)
        {
            startCountdownFinishSound.Play();
        }
        else
        {
            startCountdownIterationSound.Play();
        }
    }

    private void ToggleCountdownText(bool status)
    {
        startCountdownText.gameObject.SetActive(status);
    }

    private void ToggleSelectableOptions(bool status)
    {
        ToggleSubmitButton(status);
        ToggleFillableButtons(status);
    }


    // Event Handlers
    public void HandleNoEditionsLeft()
    {
        // Finish the game early
        FinishGame();
    }

    public void HandleCloseInfoPrompt()
    {
        // Disables the prompt, then starts the countdown timer
        ToggleInfoPrompt(false);
        StartCoroutine(StartCountdown());
    }

    public void HandleSelectFillableText(int optionIndex)
    {
        // Updates display with the option, and caches the option
        UpdateSegmentDisplay(optionIndex);
        currentSelectedOption = optionIndex;

        // Re-enables the submit button
        ToggleSubmitButton(true);
    }

    public void HandleSubmitOption()
    {
        // Submits the selected choice to the game and resets data related to options
        game.HandleSelectFillWord(currentSelectedOption);
        currentSelectedOption = -1;
        DeleteFillableButtons();

        // Updates the text if there's a new segment
        if (game.CurrentSegment != null)
        {
            DisplayNextSegment();
        }
    }

    public void HandleEarnScore(int pointsEarned)
    {
        UpdateScoreDisplay(game.Score);
        moneyEarnSound.Play();
    }

    // Helpers
    private string AddFillerTextAroundSegment(string segmentText)
    {
        // If we don't have a prefix or suffix set, sets them
        if (string.IsNullOrEmpty(cachedSegmentPrefix))
        {
            // Randomly decides how many words to give and randomly generates those words
            int prefixWordCount = Random.Range(wordCountRangeStart, wordCountRangeEnd);
            int suffixWordCount = Random.Range(wordCountRangeStart, wordCountRangeEnd);

            StringBuilder prefixBuilder = new StringBuilder($"<font=\"{REDACTED_TEXT_FONT}\">");
            StringBuilder suffixBuilder = new StringBuilder($"<font=\"{REDACTED_TEXT_FONT}\">");

            for (int i = 0; i < prefixWordCount; i++)
            {
                // Adds each word as a 'a' character repeated some number of times.
                prefixBuilder.Append('a', wordLengthFreqs[Random.Range(0, wordLengthFreqs.Length - 1)]);
                prefixBuilder.Append(' ');
            }

            prefixBuilder.Append("</font>");

            for (int i = 0; i < suffixWordCount; i++)
            {
                // Adds each word as a 'a' character repeated some number of times.
                suffixBuilder.Append('a', wordLengthFreqs[Random.Range(0, wordLengthFreqs.Length - 1)]);
                suffixBuilder.Append(' ');
            }

            suffixBuilder.Append("</font>");

            // Updates the cached prefix and suffix
            cachedSegmentPrefix = prefixBuilder.ToString();
            cachedSegmentSuffix = suffixBuilder.ToString();
        }

        // Returns the text with prefix and suffix
        // Creates the prefix and suffix
        return $"{cachedSegmentPrefix} {segmentText} {cachedSegmentSuffix}";
    }

    // Coroutines
    private IEnumerator StartCountdown()
    {
        // Runs a countdown before properly starting
        for (int i = game.StartCountdownSeconds; i >= 0; i--)
        {
            UpdateCountdown(i);
            yield return new WaitForSecondsRealtime(1);
        }

        // Waits just a tiny bit after displaying the end of the countdown for added effect
        yield return new WaitForSecondsRealtime(0.2f);
        ToggleCountdownText(false);

        // Starts the actual game
        StartGame();
        yield break;
    }

    private IEnumerator GameTimer()
    {
        // Runs the regular timer
        for (int i = game.TimeLimitSeconds; i >= 0; i--)
        {
            UpdateTimer(i);
            yield return new WaitForSecondsRealtime(1);
        }

        // Finishes the game a few seconds once the loop ends.
        ToggleSelectableOptions(false);
        musicSound.Stop();
        gameEndSound.Play();

        yield return new WaitForSecondsRealtime(2);

        FinishGame();

        // Exits coroutine
        yield break;
    }
}
