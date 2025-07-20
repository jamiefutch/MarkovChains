using System.Runtime.CompilerServices;

namespace MarkovChains;

public static class Utilities
{
    
    /// <summary>
    /// Cleans and splits the input text into words.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string[] CleanAndSplit(string text)
    {
        List<string> wordsList = new();
        int start = -1;
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsLetterOrDigit(text[i]) || text[i] == '_')
            {
                if (start == -1) start = i;
            }
            else
            {
                if (start != -1)
                {
                    wordsList.Add(text.Substring(start, i - start));
                    start = -1;
                }
            }
        }
        if (start != -1)
            wordsList.Add(text.Substring(start, text.Length - start));
        var words = wordsList.ToArray();
        return words;
    }
    
    /// <summary>
    /// Cleans and splits the input text into words.
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static List<string> CleanAndSplitToList(string text)
    {
        List<string> wordsList = new();
        int start = -1;
        for (int i = 0; i < text.Length; i++)
        {
            if (char.IsLetterOrDigit(text[i]) || text[i] == '_')
            {
                if (start == -1) start = i;
            }
            else
            {
                if (start != -1)
                {
                    wordsList.Add(text.Substring(start, i - start));
                    start = -1;
                }
            }
        }
        if (start != -1)
            wordsList.Add(text.Substring(start, text.Length - start));
        
        return wordsList;
    }
    
}