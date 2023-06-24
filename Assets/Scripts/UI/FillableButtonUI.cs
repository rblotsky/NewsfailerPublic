using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FillableButtonUI : MonoBehaviour
{
    // DATA //
    // Prefab References
    [SerializeField] private TextMeshProUGUI wordText;

    // Cached Data
    private InGameUI mainUI;
    private int wordIndex;


    // FUNCTIONS //
    // Setup
    public void Setup(InGameUI mainUIToCache, int newWordIndex, string word)
    {
        mainUI = mainUIToCache; 
        wordIndex = newWordIndex;
        UpdateDisplayWord(word);
    }

    
    // UI Updating
    private void UpdateDisplayWord(string text)
    {
        wordText.SetText(text);
    }


    // UI Events
    public void HandlePressButton()
    {
        // Passes event to the main UI
        mainUI.HandleSelectFillableText(wordIndex);
    }
}
