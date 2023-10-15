using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameUpdater : MonoBehaviour
{
    public bool inProgress = false;
    public int maxAttempts = 5;
    public WordPair currentWordPair;
    public List<WordPair> paramList;
    public List<WordPair> successWords;
    private TextMeshProUGUI textField;
    private TextMeshProUGUI hoverText;
    private TextMeshProUGUI hoverTextEng;
    private TextMeshProUGUI allWordsPopup;

    [System.Serializable]
    public class WordPairWrapper
    {
        public List<WordPair> wordPairs;
    }
    
    void Start ()
    {
        string characterList = PlayerPrefs.GetString("characterListTutorial");
        if (!string.IsNullOrEmpty(characterList))
        {
            WordPairWrapper paramListSaved = JsonUtility.FromJson<WordPairWrapper>(characterList);
            paramList = paramListSaved.wordPairs;
        }
    }

    void Update ()
    {
        if (!inProgress)
        {
            inProgress = true;
            if (paramList.Count - 4 <= 0 || successWords.Count == paramList.Count - 4)
            {
                PopUpText();
                return;
            }

            maxAttempts = 5;

            textField = GameObject.Find("AttemptCounter").GetComponent<TextMeshProUGUI>();
            textField.text = maxAttempts.ToString();
            GetComponent<Canvas>().GetComponent<StartMenu>().ClearScreen();

            List<WordPair> paramListFiltered = paramList.Where(item => item.wordDepth > 0 && !successWords.Contains(item)).ToList();
            int randomIndex = Random.Range(0, paramListFiltered.Count);
            currentWordPair = paramListFiltered[randomIndex];
            paramListFiltered = paramList.Where(item => item.wordDepth < currentWordPair.wordDepth).ToList();

            hoverText = GameObject.Find("HoverText").GetComponent<TextMeshProUGUI>();
            hoverText.text = currentWordPair.wordZh;
            hoverTextEng = GameObject.Find("HoverTextEng").GetComponent<TextMeshProUGUI>();
            hoverTextEng.text = currentWordPair.word;

            ScrollDragInitializer scrollDragInitializer = GetComponentInParent<Canvas>().GetComponentInChildren<ScrollDragInitializer>();
            scrollDragInitializer.UpdateScrollDragsFromParameterList(false, paramListFiltered);
        }
    }

    public void CheckAttempt (WordPair newWordPair)
    {
        if (newWordPair.word == currentWordPair.word)
        {
            if (!successWords.Any(pair => pair.word == newWordPair.word))
            {
                successWords.Add(newWordPair);
                PlayerPrefs.SetInt("currentHighScore", successWords.Count);
                if (successWords.Count == paramList.Count - 4)
                {
                    GetComponentInParent<Canvas>().GetComponent<StartMenu>().ClearScreen();
                    PopUpText();
                    return;
                }
            }
            inProgress = false;
        }
        else
        {
            maxAttempts -= 1;
            textField.text = maxAttempts.ToString();
        }

        if (maxAttempts <= 0)
        {
            inProgress = false;
        }
    }

    public void PopUpText ()
    {
        allWordsPopup = GameObject.Find("AllWordsPopup").GetComponent<TextMeshProUGUI>();
        allWordsPopup.text = "All words made in the sandbox have been found! Return to the sandbox to make more!";
    }
}
