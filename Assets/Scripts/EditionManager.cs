using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditionManager : MonoBehaviour
{
    // DATA //
    // Configuration
    [SerializeField] private TextAsset[] sourceFiles;

    // Generated data
    [SerializeField] private List<NewspaperEdition> editions = new List<NewspaperEdition>();


    // FUNCTIONS //
    // Unity Events
    private void Awake()
    {
        // Compiles all the source files and saves the compiled versions
        foreach(TextAsset file in sourceFiles)
        {
            editions.Add(new NewspaperEdition(file.text));
        }
    }


    // Data Access
    public Stack<NewspaperEdition> GetRandomEditionsStack()
    {
        // Stores the indices that can be taken
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < editions.Count; i++)
        {
            availableIndices.Add(i);
        }

        // Creates a stack with the editions in a random order
        Stack<NewspaperEdition> randomizedStack = new Stack<NewspaperEdition>();
        while(availableIndices.Count != 0)
        {
            int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];
            availableIndices.Remove(randomIndex);
            randomizedStack.Push(editions[randomIndex]);
        }

        // Return the generated stack
        return randomizedStack;
    }
}
