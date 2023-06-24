using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;

public class FinishedEditionsViewerUI : MonoBehaviour
{
    // DATA //
    // Scene References
    [SerializeField] private RectTransform viewPanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI issueNumberText;
    [SerializeField] private TextMeshProUGUI dateText;

    [SerializeField] private Button prevPaperButton;
    [SerializeField] private Button nextPaperButton;
    [SerializeField] private TextMeshProUGUI currentPaperText;

    // Prefabs
    [SerializeField] private GameObject articleDisplayer;

    // Current Newspapers
    private NewspaperEdition[] editionsToView;

    // Cached Data
    private int currentDisplayIndex;
    private List<ArticleDisplayUI> displayedArticles = new List<ArticleDisplayUI>();


    // FUNCTIONS //
    // External Management
    public void OpenViewer(NewspaperEdition[] newEditions)
    {
        // Caches the editions to view
        editionsToView = newEditions;

        // Only opens if there are any newspapers to view in the first place.
        if (editionsToView.Length > 0)
        {
            currentDisplayIndex = 0;
            RefreshPaperDisplay();
        }
    }


    // Internal UI Updating
    private void RefreshPaperDisplay()
    {
        // Clears the display first
        ClearPaperDisplay();

        // If the current paper exists, displays it.
        if(editionsToView.Length != 0)
        {
            WriteBasicPaperInfo();
            WriteAllArticles();
        }

        // Refreshes the buttons
        RefreshCurrentPaperText();
        RefreshButtons();
    }

    private void WriteBasicPaperInfo()
    {
        // Sets the name and basic info of this paper into the UI
        NewspaperEdition currentPaper = editionsToView[currentDisplayIndex];
        nameText.SetText(currentPaper.NewspaperName);
        issueNumberText.SetText(currentPaper.IssueNumber);
        dateText.SetText(currentPaper.Date);
    }

    private void WriteAllArticles()
    {
        // Creates a display for each article in the current newspaper.
        NewspaperEdition currentEdition = editionsToView[currentDisplayIndex];

        foreach(Article article in currentEdition.Articles)
        {
            ArticleDisplayUI displayLocation = Instantiate(articleDisplayer, viewPanel).GetComponent<ArticleDisplayUI>();
            displayLocation.SetupArticleDisplay(article);
            displayedArticles.Add(displayLocation);
        }
    }

    private void DeleteAllArticles()
    {
        foreach(ArticleDisplayUI articleDisplay in displayedArticles)
        {
            Destroy(articleDisplay.gameObject);
        }

        displayedArticles.Clear();
    }

    private void ClearPaperDisplay()
    {
        // Clears all set values for the current paper
        DeleteAllArticles();
        nameText.SetText("");
        issueNumberText.SetText("");
        dateText.SetText("");
    }

    private void RefreshButtons()
    {
        // Refreshes the previous/next newspaper buttons according to position in list
        if(currentDisplayIndex <= 0)
        {
            prevPaperButton.interactable = false;
        }
        else
        {
            prevPaperButton.interactable = true;
        }

        if(currentDisplayIndex >= editionsToView.Length - 1)
        {
            nextPaperButton.interactable = false;
        }
        else
        {
            nextPaperButton.interactable = true;
        }
    }

    private void RefreshCurrentPaperText()
    {
        // Refreshes the text for the current viewed paper
        currentPaperText.SetText($"{currentDisplayIndex + 1}/{editionsToView.Length}");
    }


    // UI Events
    public void HandleChangeCurrentNewspaper(int change)
    {
        // Moves up or down through the array of newspapers
        // But only if the change is to a valid index
        int newIndex = currentDisplayIndex + change;

        if (newIndex >= 0 && newIndex < editionsToView.Length)
        {
            currentDisplayIndex = newIndex;
            RefreshPaperDisplay();
        }
    }
}
