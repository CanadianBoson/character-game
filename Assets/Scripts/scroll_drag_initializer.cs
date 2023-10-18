using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;

public class ScrollDragInitializer : MonoBehaviour
{
    public GameObject ScrollChar;
    public Transform contentParent;
    public List<WordPair> parameterList;
    private List<GameObject> instantiatedPrefabs = new List<GameObject>();
    private WordPairWrapper parameterListSaved = new WordPairWrapper();
    private Scene currentScene;
    private List<WordPair> listToUse;

    [System.Serializable]
    public class WordPairWrapper
    {
        public List<WordPair> wordPairs;
    }

    private void Start()
    {
        currentScene = SceneManager.GetActiveScene();
        string characterList = PlayerPrefs.GetString("characterListTutorial");

        if (!string.IsNullOrEmpty(characterList))
        {
            parameterListSaved = JsonUtility.FromJson<WordPairWrapper>(characterList);
            if (parameterListSaved.wordPairs.Count > 0 && currentScene.name == "Tutorial") {
                parameterList = parameterListSaved.wordPairs;
            }
        }

        UpdateScrollDragsFromParameterList(false);
    }

    public void UpdateScrollDragsFromParameterList(bool update, List<WordPair> updatedList = null)
    {
        if (updatedList != null)
        {
            foreach (var prefab in instantiatedPrefabs)
            {
                Destroy(prefab);
            }
            instantiatedPrefabs.Clear();
        }

        List<WordPair> listToUse = (updatedList != null) ? updatedList : parameterList;
        
        foreach (var parameterPair in listToUse)
        {
            if (update) 
            {
                GameObject existingPrefab = instantiatedPrefabs.Find(p => p.GetComponent<ScrollDrag>().wordPair.word == parameterPair.word);
                if (existingPrefab != null) {continue;}
            }
            GameObject newPrefab = Instantiate(ScrollChar, contentParent);
            newPrefab.GetComponent<ScrollDrag>().wordPair = parameterPair;
            newPrefab.GetComponent<TextMeshProUGUI>().text = parameterPair.wordZh;
            
            BoxCollider2D boxCollider = newPrefab.GetComponent<BoxCollider2D>();
            boxCollider.size = new Vector2(parameterPair.charLen * 30, boxCollider.size.y);
            boxCollider.offset = new Vector2(parameterPair.charLen * 15, boxCollider.offset.y);

            instantiatedPrefabs.Add(newPrefab);
        }
        if (currentScene.name == "Tutorial")
        {
            GameObject.Find("ComboCounter").GetComponent<TextMeshProUGUI>().text = (parameterList.Count - 4).ToString();
        }
        else if (currentScene.name == "Game")
        {
        GameObject.Find("ScoreCounter").GetComponent<TextMeshProUGUI>().text = 
            GetComponentInParent<Canvas>().GetComponent<GameUpdater>().successWords.Count.ToString();
        }

        Vector2 newSize = GetComponent<RectTransform>().sizeDelta;
        newSize.y = 50 * parameterList.Count;
        GetComponent<RectTransform>().sizeDelta = newSize;
    }
}

