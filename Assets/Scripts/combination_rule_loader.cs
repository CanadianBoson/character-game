using UnityEngine;
using System.Collections.Generic;


public class CombinationRuleLoader : MonoBehaviour
{

    [System.Serializable]
    public class CombinationRuleUnpacked
    {
        public string word1;
        public string word2;
        public string resultWord;
        public string resultWordZh;
        public int charLen;
        public int wordDepth;
    }

    [System.Serializable]
    public class CombinationRulesWrapper
    {
        public List<CombinationRuleUnpacked> combinations;
    }

    public TextAsset jsonFile;
    private List<CombinationRuleUnpacked> combinationRulesUnpacked = new List<CombinationRuleUnpacked>();
    public List<CombinationRule> combinationRules = new List<CombinationRule>();

    private void Start()
    {
        if (jsonFile != null)
        {
            CombinationRulesWrapper rulesWrapper = JsonUtility.FromJson<CombinationRulesWrapper>(jsonFile.text);
            if (rulesWrapper != null && rulesWrapper.combinations != null)
            {
                combinationRulesUnpacked = rulesWrapper.combinations;
            }
            else
            {
                Debug.LogError("Invalid JSON format or no combinations found.");
            }

            foreach (var unpackedRule in combinationRulesUnpacked)
            {
                CombinationRule newRule = new CombinationRule
                {
                    word1 = unpackedRule.word1,
                    word2 = unpackedRule.word2,
                    wordPair = new WordPair
                    {
                        word = unpackedRule.resultWord,
                        wordZh = unpackedRule.resultWordZh,
                        charLen = unpackedRule.charLen,
                        wordDepth = unpackedRule.wordDepth
                    }
                };
                combinationRules.Add(newRule);
            }
        }
        else
        {
            Debug.LogError("No JSON file assigned.");
        }
    }
}
