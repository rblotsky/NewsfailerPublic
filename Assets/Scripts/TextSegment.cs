using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

[System.Serializable]
public class TextSegment
{
    // DATA //
    // Basic Data
    [SerializeField] protected string text;

    // Dependencies
    private List<string> nounDependencies = new List<string>();

    // Properties
    public List<string> NounDependencies { get { return nounDependencies; } }

    // Constants
    protected static readonly string FILLED_TEXT_FONT = "Mynerve-Regular SDF";
    protected static readonly string EMPTY_SPACE_FONT = "LiberationSans SDF";
    protected static readonly string EMPTY_SPACE_STRING = "_________";


    // CONSTRUCTORS //
    public TextSegment(string newText)
    {
        text = newText;
        SetOwnNounDependencies();
    }

    public TextSegment()
    {
        text = string.Empty;
        SetOwnNounDependencies();
    }


    // FUNCTIONS //
    // Getters
    public virtual string GetFullText(bool includeStyling, Dictionary<string, string> nounTable)
    {
        // Returns a copy of the text to avoid modifying the actual text
        string returnText = text;

        // Replaces the nouns with their actual content (throws an error if they don't have content)
        foreach(string dependency in NounDependencies)
        {
            string noun = "";
            if(nounTable.TryGetValue(dependency, out noun))
            {
                string dependencyInsertionText = $"__[{dependency}]__";

                if(includeStyling)
                {
                    noun = "<font=\"" + FILLED_TEXT_FONT + "\">" + noun + "</font>";
                }

                returnText = returnText.Replace(dependencyInsertionText, noun);
            }

            else
            {
                throw new System.Exception($"[TextSegment.GetText()] Failed to get text because dependency \"{dependency}\" has no noun yet !");
            }
        }

        // Returns the modified text
        return returnText;
    }


    // Checkers
    public virtual bool IsTextValid()
    {
        // Returns true by default but subclasses modify it
        return true;
    }


    // Modifiers
    public virtual void ResetText()
    {
        // Does nothing by default but subclasses modify it
    }

    protected void SetOwnNounDependencies()
    {
        // Reads through text for all noun dependencies, if text isn't empty
        if (!string.IsNullOrEmpty(text))
        {
            MatchCollection dependencyMatches = NewspaperEdition.NOUN_DEPENDENCY_REGEX.Matches(text);

            foreach (Match match in dependencyMatches)
            {
                string noun = match.Groups[1].Value;
                Debug.Log("Found noun dependency: " + noun);
                nounDependencies.Add(noun);
            }
        }
    }
}
