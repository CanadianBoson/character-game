using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;

using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    [System.Serializable]
    public class WordPairWrapper
    {
        public List<WordPair> wordPairs;
    }

    void Start ()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == "Menu")
        {
            TextMeshProUGUI highScoreText = GameObject.Find("HighScore").GetComponent<TextMeshProUGUI>();
            highScoreText.text = PlayerPrefs.GetInt("highScore", 0).ToString();
        }
    }

    public void OnPlayButton (string sceneName)
    {
        Scene currentScene = SceneManager.GetActiveScene();
        List<string> dynamicScenes = new List<string>{"Tutorial", "Game"};
        if (dynamicScenes.Contains(currentScene.name))
        {
            WordPairWrapper paramList = new WordPairWrapper();
            paramList.wordPairs = GetComponentInParent<Canvas>().GetComponentInChildren<ScrollDragInitializer>().parameterList;
            string characterList = JsonUtility.ToJson(paramList);
            PlayerPrefs.SetString("characterList" + currentScene.name, characterList);
            PlayerPrefs.Save();

            if (currentScene.name == "Game")
            {
                paramList.wordPairs = GetComponentInParent<Canvas>().GetComponent<GameUpdater>().successWords;
                characterList = JsonUtility.ToJson(paramList);
                PlayerPrefs.SetString("successWords", characterList);

                int highScore = PlayerPrefs.GetInt("highScore", 0);
                int currentHighScore = PlayerPrefs.GetInt("currentHighScore", 0);
                highScore = Mathf.Max(highScore, currentHighScore);
                PlayerPrefs.SetInt("highScore", highScore);

                PlayerPrefs.Save();
            }
        }
        
        StartCoroutine(GetComponentInParent<Canvas>().GetComponent<SceneFader>().FadeAndLoadScene(SceneFader.FadeDirection.In, sceneName));
    }

    public void ClearScreen ()
    {
        foreach (Transform child in transform)
        {
            ScrollDrag scrollDrag = child.GetComponent<ScrollDrag>();
            if (scrollDrag != null)
            {
                Destroy(scrollDrag.gameObject);
            }
        }
    }

    public void HintHighlight ()
    {
        List<WordPair> wordPairs = GetComponentInParent<Canvas>().GetComponentInChildren<ScrollDragInitializer>().parameterList;
        List<CombinationRule> combinationRules = GameObject.FindObjectOfType<CombinationRuleLoader>().combinationRules;
        List<CombinationRule> matchingRules = new List<CombinationRule>();

        ScrollDrag[] scrollDrags = GetComponentInParent<Canvas>().GetComponentsInChildren<ScrollDrag>();
        foreach (var drag in scrollDrags)
        {
            drag.GetComponent<TextMeshProUGUI>().color = new Color(0, 0, 0, 255);
        }

        foreach (var rule in combinationRules)
        {
            if (wordPairs.Any(wordPair => wordPair.word == rule.word1) &&
                wordPairs.Any(wordPair => wordPair.word == rule.word2) &&
                wordPairs.All(wordPair => wordPair.word != rule.wordPair.word))
            {
                matchingRules.Add(rule);
            }
        }
        
        if (matchingRules.Count > 0)
        {
            int randomIndex = Random.Range(0, matchingRules.Count);
            CombinationRule randomRule = matchingRules[randomIndex];

            foreach (var drag in scrollDrags)
            {
                if (drag.wordPair.word == randomRule.word1 || drag.wordPair.word == randomRule.word2)
                {
                    drag.GetComponent<TextMeshProUGUI>().color = new Color(255, 255, 255, 255);
                }
            }
        }

    }

    public void NextWord ()
    {
        GetComponentInParent<Canvas>().GetComponent<GameUpdater>().inProgress = false;
    } 

    public void LoadSave ()
    {
        string characterList = PlayerPrefs.GetString("successWords");
        if (!string.IsNullOrEmpty(characterList))
        {
            GetComponentInParent<Canvas>().GetComponent<GameUpdater>().successWords = JsonUtility.FromJson<WordPairWrapper>(characterList).wordPairs;
        }
        NextWord(); 
    }

    public void TranslateToggle ()
    {
        foreach (ScrollDrag drag in GetComponentInParent<Canvas>().GetComponentsInChildren<ScrollDrag>())
        {
            drag.translate = !drag.translate;
        }
    } 
}