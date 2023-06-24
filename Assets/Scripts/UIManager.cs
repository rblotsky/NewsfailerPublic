using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    // DATA //
    // Singleton Pattern
    public static UIManager Instance { get; private set; }

    // Scene References
    [SerializeField] private EditionManager editionManager; 
    [SerializeField] private Game game;

    // UI References
    [SerializeField] private GameObject gameUIPanel;
    [SerializeField] private GameObject menuUIPanel;
    [SerializeField] private GameObject gameFinishPanel;
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    // Transition Data
    [SerializeField] private float crossFadeSeconds;

    // Constants
    private static readonly int FADEOUT_INCREMENTS_PER_SEC = 100;


    // FUNCTIONS //
    // Unity Events
    private void Awake()
    {
        // Sets singleton
        Instance = this;

        // Asserts presence of important UI items
        Debug.Assert(editionManager != null, "[UIManager.Awake()] Was not given an EditionManager!");
        Debug.Assert(game != null, "[UIManager.Awake()] Was not given a Game!");
    }

    private void Start()
    {
        menuUIPanel.SetActive(true);
    }


    // UI Swapping
    public void GoToFinishPanel(GameObject currentPanelChild)
    {
        StartCoroutine(CrossFade(currentPanelChild.GetComponentInParent<UIPanelBase>().gameObject, gameFinishPanel));
    }

    public void GoToGamePanel(GameObject currentPanelChild)
    {
        StartCoroutine(CrossFade(currentPanelChild.GetComponentInParent<UIPanelBase>().gameObject, gameUIPanel));
    }

    public void GoToMenuPanel(GameObject currentPanelChild)
    {
        StartCoroutine(CrossFade(currentPanelChild.GetComponentInParent<UIPanelBase>().gameObject, menuUIPanel));
    }


    // Coroutines
    private IEnumerator CrossFade(GameObject oldPanel, GameObject newPanel)
    {
        // Caches fade increment
        float fadeIncrement = 1.0f / (crossFadeSeconds * FADEOUT_INCREMENTS_PER_SEC);

        // Fade out
        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.alpha = 0;
        for (int i = 0; i < crossFadeSeconds * FADEOUT_INCREMENTS_PER_SEC; i++)
        {
            fadeCanvasGroup.alpha += fadeIncrement;
            yield return new WaitForSecondsRealtime(1.0f / FADEOUT_INCREMENTS_PER_SEC);
        }

        fadeCanvasGroup.alpha = 1;

        // Swaps panels
        oldPanel.SetActive(false);
        newPanel.SetActive(true);

        // If this is fading into the game panel, also set up the game
        if(newPanel == gameUIPanel)
        {
            // Sets up the game, this has to be done after UI opens b/c events.
            game.SetUpNewGame();
        }

        // Fade in
        for (int i = 0; i < crossFadeSeconds * FADEOUT_INCREMENTS_PER_SEC; i++)
        {
            fadeCanvasGroup.alpha -= fadeIncrement;
            yield return new WaitForSecondsRealtime(1.0f / FADEOUT_INCREMENTS_PER_SEC);
        }

        fadeCanvasGroup.alpha = 0;
        fadeCanvasGroup.gameObject.SetActive(false);
        yield break;
    }
}
