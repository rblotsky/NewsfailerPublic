using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    // DATA //
    // References
    [SerializeField] private EditionManager editionManager;

    // Basic Data
    [SerializeField] private int timeLimitSeconds = 60;
    [SerializeField] private int startCountdownSeconds = 3;

    // Status Tracking
    private Stack<NewspaperEdition> remainingEditions;
    private Queue<NewspaperEdition> completedEditions;
    private Queue<FillableTextSegment> remainingSegmentsRandomized;

    // Points tracking
    private int score;
    private int highScore;

    // Properties
    public int TimeLimitSeconds { get { return timeLimitSeconds; } }
    public int StartCountdownSeconds { get { return startCountdownSeconds; } }
    public int Score { get { return score; } }
    public int HighScore { get { return highScore; } }
    public NewspaperEdition CurrentEdition { get { return (remainingEditions.Count > 0) ? remainingEditions.Peek() : null; } }
    public FillableTextSegment CurrentSegment { get {  return (remainingSegmentsRandomized.Count > 0) ? remainingSegmentsRandomized.Peek() : null; } }
    public int FinishedEditionsCount { get { return completedEditions.Count; } }
    public int RemainingEditionsCount { get { return remainingEditions.Count; } }

    // Events
    public delegate void EmptyDelegate();
    public delegate void IntDelegate(int value);
    public delegate void NewspaperEditionDelegate(NewspaperEdition value);

    public event EmptyDelegate OnSetupFinish;
    public event EmptyDelegate OnSegmentChange;
    public event NewspaperEditionDelegate OnEditionComplete;
    public event IntDelegate OnScoreEarn;
    public event EmptyDelegate OnNoEditionsLeft;


    // FUNCTIONS //
    // Unity Events
    private void Awake()
    {
        Debug.Assert(editionManager != null, "[Game.Awake()] Was not given an EditionManager!");
    }


    // Game State Changes
    public void SetUpNewGame()
    {
        // First, reset game data from the last game.
        ResetGameData();

        // Prepares the collections of editions
        completedEditions = new Queue<NewspaperEdition>();
        remainingEditions = editionManager.GetRandomEditionsStack();

        // Prepares the first newspaper's fillable segment stack
        remainingSegmentsRandomized = new Queue<FillableTextSegment>(remainingEditions.Peek().GetRandomizedFillableSegments());
        Debug.Log("There are " + remainingSegmentsRandomized.Count + " segments left.");
        // Runs the on setup event
        OnSetupFinish?.Invoke();
    }


    // Getters
    public NewspaperEdition[] HandleGetCompletedEditions()
    {
        // Creates an array from the completed editions
        NewspaperEdition[] returnArray = new List<NewspaperEdition>(completedEditions).ToArray();
        return returnArray;
    }


    // Game Events
    public void HandleSelectFillWord(int wordIndex)
    {
        // Fills the segment and goes to the next one. Finishes the edition if needed.
        remainingSegmentsRandomized.Dequeue().SelectWord(wordIndex);

        if (remainingSegmentsRandomized.Count == 0)
        {
            FinishCurrentEdition();
        }

        // Runs the new segment event
        OnSegmentChange?.Invoke();
    }


    // Modifiers
    public void ResetGameData()
    {
        // Resets all the completed editions and the last remaining one (since the player may have done part of it)
        while (completedEditions != null && completedEditions.Count > 0)
        {
            completedEditions.Dequeue().ResetEdition();
        }

        if (remainingEditions != null && remainingEditions.Count > 0)
        {
            remainingEditions.Peek().ResetEdition();
        }

        // Clears all our collections
        completedEditions?.Clear();
        remainingEditions?.Clear();
        remainingSegmentsRandomized?.Clear();

        // Resets our score
        score = 0;
    }

    public void SaveHighScore()
    {
        highScore = Mathf.Max(score, highScore);
    }


    // Internal Functionality
    private void PrepareSegmentsForNewEdition()
    {
        // Fills up the remaining segments using ones from the current edition
        remainingSegmentsRandomized = new Queue<FillableTextSegment>(remainingEditions.Peek().GetRandomizedFillableSegments());
    }

    private void FinishCurrentEdition()
    {
        // Saves the current edition to completed ones
        NewspaperEdition finishedEdition = remainingEditions.Pop();
        completedEditions.Enqueue(finishedEdition);

        // Earn points for this edition
        EarnPointsForEdition(finishedEdition);

        // Runs the edition finish event
        OnEditionComplete?.Invoke(finishedEdition);

        // Prepares segments for the next edition if there is one
        if(remainingEditions.Count != 0)
        {
            PrepareSegmentsForNewEdition();
        }

        else
        {
            remainingSegmentsRandomized = new Queue<FillableTextSegment>();
            OnNoEditionsLeft?.Invoke();
        }

    }

    private void EarnPointsForEdition(NewspaperEdition finishedEdition)
    {
        // Earn 1 point for each filled segment
        int pointsEarned = finishedEdition.GetNumFillableSegments();
        score += pointsEarned;

        OnScoreEarn?.Invoke(pointsEarned);
    }
}
