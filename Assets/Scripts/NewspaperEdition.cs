using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


[System.Serializable]
public class NewspaperEdition
{
    // DATA //
    // Basic Data
    [SerializeField] private string newspaperName;
    [SerializeField] private string issueNumber;
    [SerializeField] private string date;

    // Articles
    [SerializeField] private List<Article> articles = new List<Article>();
    private List<FillableTextSegment> fillableSegments = new List<FillableTextSegment>();

    // Nouns
    private Dictionary<string, string> nounTable = new Dictionary<string, string>();

    // Properties
    public string NewspaperName { get { return newspaperName; } }
    public string IssueNumber { get { return issueNumber; } }
    public string Date { get { return date; } } 
    public Article[] Articles { get { return articles.ToArray(); } }
    public Dictionary<string, string> NounTable { get { return nounTable; } }

    // Constants
    private static readonly Regex NEWSPAPER_NAME_REGEX = new Regex(@"^\$\$\$(.+)\$\$\$$");
    private static readonly Regex ISSUE_NUMBER_REGEX = new Regex(@"^#(.+)$");
    private static readonly Regex DATE_REGEX = new Regex(@"^\@(.+)$");
    private static readonly Regex ARTICLE_START_REGEX = new Regex(@"^\$\$\s?(.+?)\s?\$\$$");
    private static readonly char SEGMENT_SPLIT_CHAR = '|';
    public static readonly Regex FILLABLE_SPACE_REGEX = new Regex(@"(.*)__\((?:\[(\w+)\])?(.+)\)__(.*)");
    private static readonly Regex END_REGEX = new Regex(@"^\$\$\$\s*END\s*\$\$\$$");
    private static readonly Regex FINAL_NEWLINE_REGEX = new Regex(@"\n$");
    public static readonly Regex NOUN_DEPENDENCY_REGEX = new Regex(@"__\[(\w+)\]__");


    // CONSTRUCTORS //
    public NewspaperEdition(string fileContents)
    {
        // Passes along to the compilation function, then runs some other setup functions
        CompileFromTextFile(fileContents);
        CacheFillableSegments();
        SetDelegatesInSegments();
        SetSegmentDependents();
    }


    // FUNCTIONS //
    // Setup
    public void CompileFromTextFile(string fileContents)
    {
        // Splits into lines
        string[] fileLines = fileContents.Split("\n");

        // Declares data that is being gathered
        Article currentArticle = null;
        string currentTextSegment = string.Empty;


        // Iterates over each line, parsing it as necessary depending on current file state
        foreach(string uneditedLine in fileLines)
        {
            // Removes newlines and carriage returns, those are added to the end of each line anyway.
            string workingLine = uneditedLine.Replace("\n", string.Empty).Replace("\r", string.Empty);


            // If reached end, we finish anything.
            if (END_REGEX.IsMatch(workingLine))
            {
                // If we had a previous article, we save its last segment
                if (currentArticle != null)
                {
                    SaveTextSegmentToArticle(currentArticle, currentTextSegment);
                    currentTextSegment = string.Empty;
                }

                // Exit the file parsing loop
                break;
            }

            // If this is the newspaper name, issue number, or date, save it.
            else if (NEWSPAPER_NAME_REGEX.IsMatch(workingLine))
            {
                newspaperName = NEWSPAPER_NAME_REGEX.Match(workingLine).Groups[1].Value;

            }

            else if(ISSUE_NUMBER_REGEX.IsMatch(workingLine))
            {
                issueNumber = ISSUE_NUMBER_REGEX.Match(workingLine).Groups[1].Value;

            }

            else if(DATE_REGEX.IsMatch(workingLine))
            {
                date = DATE_REGEX.Match(workingLine).Groups[1].Value;

            }

            // If we're starting a new article, save the last one's last segment
            else if(ARTICLE_START_REGEX.IsMatch(workingLine))
            {
                string articleTitle = ARTICLE_START_REGEX.Match(workingLine).Groups[1].Value;

                // If we had a previous article, we save its last segment
                if (currentArticle != null)
                {
                    SaveTextSegmentToArticle(currentArticle, currentTextSegment);
                    currentTextSegment = string.Empty;
                }

                // Then we add a new article to the list with the found name
                currentArticle = new Article();
                currentArticle.SetNewspaperEdition(this);
                currentArticle.SetHeadline(articleTitle);
                articles.Add(currentArticle);
            }

            // Finally if we're not starting a new article or anything else,
            // we just split the line according to segment splitters and add it to the current article.
            else if(currentArticle != null)
            {
                
                // Split line by segment splitters
                string[] lineSegments = workingLine.Split(SEGMENT_SPLIT_CHAR);

                // Each segment is added as a segment to the article (except the last one, which continues to next line)
                for(int i = 0; i < lineSegments.Length; i++)
                {
                    currentTextSegment += lineSegments[i] + "\n";

                    // If not the last item in the array, we save it
                    if (i != lineSegments.Length - 1)
                    {
                        SaveTextSegmentToArticle(currentArticle, currentTextSegment);
                        currentTextSegment = string.Empty;
                    }
                }
            }
        }
    }

    private void SaveTextSegmentToArticle(Article article, string segmentText)
    {
        // Removes the last newline from the segment
        string workingText = FINAL_NEWLINE_REGEX.Replace(segmentText, string.Empty);

        // Prepares the segment to add
        TextSegment segmentToAdd;
        bool isFillable = false;

        // If this segment contains a fillable space, save as a FillableTextSegment.
        MatchCollection regexMatches = FILLABLE_SPACE_REGEX.Matches(workingText);
        if (regexMatches.Count != 0)
        {
            segmentToAdd = new FillableTextSegment(workingText);
            isFillable = true;
        }

        else
        {
            segmentToAdd = new TextSegment(workingText);
        }

        // Adds the segment
        article.AddSegment(segmentToAdd, isFillable);
    }
    
    private void CacheFillableSegments()
    {
        // Caches all the fillable segments
        foreach (Article article in articles)
        {
            foreach (FillableTextSegment segment in article.GetFillableSegments())
            {
                fillableSegments.Add(segment);
            }
        }
    }

    private void SetDelegatesInSegments()
    {
        foreach(FillableTextSegment segment in fillableSegments)
        {
            segment.OnNounSet += HandleNounSet;
            segment.OnNounReset += HandleNounReset;
        }
    }

    private void SetSegmentDependents()
    {
        // Goes through all fillable segments and finds all their fillable dependents
        foreach(FillableTextSegment segment in fillableSegments)
        {
            foreach (FillableTextSegment dependentSegment in fillableSegments)
            {
                // If the dependent segment contains this segment's noun, we add it as a dependent
                if(dependentSegment.NounDependencies.Contains(segment.FillableNoun))
                {
                    segment.Dependents.Add(dependentSegment);
                }
            }
        }
    }

    // Modifiers
    public void ResetEdition()
    {
        // Resets the text for each article
        foreach(Article article in articles)
        {
            article.ResetArticleText();
        }
    }

    public void HandleNounSet(string nounName, string nounValue)
    {
        // Throws an error if the noun already exists, otherwise adds it to the noun table with its value
        if(nounTable.ContainsKey(nounName))
        {
            throw new System.Exception($"[NewspaperEdition.HandleNounSet()] Tried setting a noun {nounName} but this noun already exists!");
        }

        else
        {
            nounTable.Add(nounName, nounValue);
        }
    }

    public void HandleNounReset(string nounName)
    {
        // Removes this noun from the nounTable if it's there
        if(nounTable.ContainsKey(nounName))
        {
            nounTable.Remove(nounName);
        }
    }

    
    // Getters
    public int GetNumFillableSegments()
    {
        // Calculates the total amount by adding together amounts from all the articles
        int totalFillables = 0;

        foreach (Article article in articles)
        {
            totalFillables += article.GetFillableSegments().Count;
        }

        return totalFillables;
    }


    public List<FillableTextSegment> GetRandomizedFillableSegments()
    {
        // Algorithm:
        // We move segments between the three lists. First, segments are moved from remaining to next-up.
        // Next, we randomly select next-up segments to put in added.
        // When we add a segment, we move all its dependents that have all dependencies met to next up.
        
        // Caches three lists: one of remaining segments to add, one of added segments, and one of the next up segments
        List<FillableTextSegment> remainingSegments = new List<FillableTextSegment>(fillableSegments);
        List<FillableTextSegment> nextUpSegments = new List<FillableTextSegment>();
        List<FillableTextSegment> addedSegments = new List<FillableTextSegment>();

        // Caches hashset of met dependencies
        HashSet<string> metDependencies = new HashSet<string>();

        // Adds all no-dependency segments to next up
        foreach(FillableTextSegment segment in remainingSegments)
        {
            if(segment.NounDependencies.Count == 0)
            {
                nextUpSegments.Add(segment);
            }
        }

        // Removes them from remaining segments
        foreach(FillableTextSegment segment in nextUpSegments)
        {
            remainingSegments.Remove(segment);
        }

        // While there are next-up segments, we randomly choose one to add.
        while (nextUpSegments.Count > 0)
        {
            FillableTextSegment toAdd = nextUpSegments[Random.Range(0, nextUpSegments.Count - 1)];
            nextUpSegments.Remove(toAdd);
            addedSegments.Add(toAdd);

            // If it has a Noun, we add the noun to met dependencies and add its dependents to 
            // next-up if they have all dependencies met 
            if (!string.IsNullOrEmpty(toAdd.FillableNoun))
            {
                metDependencies.Add(toAdd.FillableNoun);

                foreach(FillableTextSegment dependent in toAdd.Dependents)
                {
                    if(DoesSegmentHaveDependenciesMet(dependent, metDependencies))
                    {
                        remainingSegments.Remove(dependent);
                        nextUpSegments.Add(dependent);
                    }
                }
            }
        }

        // If there are any remaining segments left that weren't added, display a warning
        // (this only occurs in case of circular dependencies)
        if(remainingSegments.Count > 0)
        {
            Debug.LogWarning("[NewspaperEdition.GetRandomizedFillableSegments()] Not all segments were retrieved! This is most likely caused by a circular dependency.");
        }

        // Returns the list of added segments
        return addedSegments;
    }


    // Helpers
    private bool DoesSegmentHaveDependenciesMet(FillableTextSegment segment, HashSet<string> filledNouns)
    {
        // If all dependencies in this segment are met, returns true. Otherwise, returns false.
        foreach(string dependency in segment.NounDependencies)
        {
            if(!filledNouns.Contains(dependency))
            {
                return false;
            }
        }

        return true;
    }
}
