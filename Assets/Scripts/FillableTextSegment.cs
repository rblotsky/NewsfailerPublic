using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

[System.Serializable]
public class FillableTextSegment : TextSegment
{
    // DATA //
    // Options Data
    [SerializeField] private string[] fillableOptions;
    private int selectedOption = -1;
    private string fillableNoun = string.Empty;

    // Cached Data
    private List<FillableTextSegment> dependents;

    // Properties
    public string[] Options { get { return fillableOptions; } }
    public string FillableNoun { get { return fillableNoun; } }
    public List<FillableTextSegment> Dependents { get { return dependents; } }

    // Events
    public delegate void NounSetDelegate(string nounName, string nounValue);
    public delegate void NounResetDelegate(string nounName);

    public event NounSetDelegate OnNounSet;
    public event NounResetDelegate OnNounReset;


    // CONSTRUCTORS //
    public FillableTextSegment(string segmentText)
    {
        // Sets object data
        dependents = new List<FillableTextSegment>();

        // Parses the text to find the fillables, then finds the noun dependencies
        ParseFileText(segmentText);
        SetOwnNounDependencies();
    }


    // FUNCTIONS //
    // Parsing
    public void ParseFileText(string segmentText)
    {
        // Parses file text into the correct format here.
        // First splits into the three parts: left, right, and fillable.
        Match regexMatch = NewspaperEdition.FILLABLE_SPACE_REGEX.Match(segmentText);

        string leftSideText = (string.IsNullOrEmpty(regexMatch.Groups[1].Value)) ? string.Empty : regexMatch.Groups[1].Value;
        string rightSideText = (string.IsNullOrEmpty(regexMatch.Groups[4].Value)) ? string.Empty : regexMatch.Groups[4].Value;

        // Now decides the fillable options by splitting the middle section by commas
        fillableOptions = regexMatch.Groups[3].Value.Split(",");

        // The actual text is the left and right with a blank in the middle
        text = leftSideText + EMPTY_SPACE_STRING + rightSideText;

        // Sets a noun entry for this text segment if needed
        string noun = regexMatch.Groups[2].Length != 0 ? regexMatch.Groups[2].Value : string.Empty;

        if (!string.IsNullOrEmpty(noun))
        {
            fillableNoun = noun;
        }

        // Throws an exception if we have no fill options
        if (fillableOptions.Length == 0)
        {
            throw new System.Exception($"[FillableTextSegment.ParseFileText()] Received a fillable segment with no fill options! ({segmentText})");
        }
    }


    // Getters
    public override string GetFullText(bool includeStyling, Dictionary<string,string> nounTable)
    {
        Debug.Log("Getting text! Selected Option is " + selectedOption + "!");
        // Returns the text with the current selected option
        return GetTextWithOption(selectedOption, includeStyling, nounTable);
    }

    public string GetTextWithOption(int wordIndex, bool includeStyling, Dictionary<string, string> nounTable)
    {
        // First uses the base implementation to fill all noun dependencies
        string returnText = base.GetFullText(includeStyling, nounTable);
        
        // Gets the option text to use. If invalid option, uses a blank space.
        string fillableToUse = EMPTY_SPACE_STRING;
        if (wordIndex >= 0 && wordIndex < fillableOptions.Length)
        {
            fillableToUse = fillableOptions[wordIndex];

            // Styles the fillable if needed
            if (includeStyling)
            {
                fillableToUse = "<font=\"" + FILLED_TEXT_FONT + "\">" + fillableToUse + "</font>";
            }
        }

        // Also styles empty spaces with a different font
        else if (includeStyling)
        {
            fillableToUse = "<font=\"" + EMPTY_SPACE_FONT + "\">" + fillableToUse + "</font>";
        }

        // Replaces the empty space with the fillable and returns it
        returnText = returnText.Replace(EMPTY_SPACE_STRING, fillableToUse);
        return returnText;
    }


    // Checkers
    public override bool IsTextValid()
    {
        // Text is valid if the filled section isn't empty.
        return (selectedOption >= 0 && selectedOption < fillableOptions.Length);
    }


    // Modifiers
    public void SelectWord(int wordIndex)
    {
        // Sets the filled content equal to the word at that index, if legal value
        if (wordIndex >= 0 && wordIndex < fillableOptions.Length)
        {
            selectedOption = wordIndex;
            if(!string.IsNullOrEmpty(FillableNoun))
            {
                OnNounSet?.Invoke(FillableNoun, fillableOptions[selectedOption]);
            }
        }
    }

    public override void ResetText()
    { 
        // Resets the selected option to -1 and reset noun if there is one
        selectedOption = -1;

        if(!string.IsNullOrEmpty(FillableNoun))
        {
            OnNounReset?.Invoke(FillableNoun);
        }
    }
}
