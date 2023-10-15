using System;


[System.Serializable]
public class WordPair
{
    public string word;
    public string wordZh;
    public int charLen;
    public int wordDepth;

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        WordPair other = (WordPair)obj;
        return string.Equals(this.word, other.word, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return StringComparer.OrdinalIgnoreCase.GetHashCode(this.word);
    }
}