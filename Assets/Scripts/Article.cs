using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

[System.Serializable]
public class Article 
{
    // DATA //
    // Basic Data
    [SerializeField] private string headline = string.Empty;

    // Generated data
    [SerializeField] private List<TextSegment> segments = new List<TextSegment>();
    private List<FillableTextSegment> fillableSegmentsReferences = new List<FillableTextSegment>();

    // Cached Data
    [System.NonSerialized] private NewspaperEdition ownerEdition;

    // Properties
    public string Headline { get { return headline; } set { headline = value; } }


    // FUNCTIONS //
    // Getters
    public string GetFullText(bool includeStyling)
    {
        // Combines the text of each segment together into a single string
        StringBuilder fullText = new StringBuilder();
        foreach(TextSegment seg in segments)
        {
            fullText.Append(seg.GetFullText(includeStyling, ownerEdition.NounTable));
        }

        // Returns the text as a string
        return fullText.ToString();
    }

    public List<FillableTextSegment> GetFillableSegments()
    {
        return fillableSegmentsReferences;
    }


    // Modifiers
    public void SetHeadline(string newHeadline)
    {
        headline = newHeadline;
    }

    public void AddSegment(TextSegment newSegment, bool isFillable)
    {
        segments.Add(newSegment);

        if(isFillable)
        {
            fillableSegmentsReferences.Add((FillableTextSegment)newSegment);
        }
    }

    public void ResetArticleText()
    {
        foreach(TextSegment segment in segments)
        {
            segment.ResetText();
        } 
    }

    public void SetNewspaperEdition(NewspaperEdition edition)
    {
        ownerEdition = edition;
    }
}
