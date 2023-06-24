using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ArticleDisplayUI : MonoBehaviour
{
    // DATA //
    // Scene References
    [SerializeField] private TextMeshProUGUI articleTitleText;
    [SerializeField] private TextMeshProUGUI articleContentText;

    
    // FUNCTIONS //
    public void SetupArticleDisplay(Article article)
    {
        articleTitleText.SetText(article.Headline);
        articleContentText.SetText(article.GetFullText(true));
    }
}
