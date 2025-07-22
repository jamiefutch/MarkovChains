using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static string[] CleanAndSplitTokenizer(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

        // Regex pattern explanation:
        // 1. Email: [\w.-]+@[\w.-]+\.[a-zA-Z]{2,}
        // 2. URL: https?://[^\s]+
        // 3. Date (ISO): \d{4}-\d{2}-\d{2}
        // 4. Time: \d{2}:\d{2}(:\d{2})?
        // 5. Contraction: \b\w+'\w+\b
        // 6. Words: \b\w+\b

        var pattern = @"[\w.-]+@[\w.-]+\.[a-zA-Z]{2,}" + // email
                      @"|https?://[^\s]+" +             // url
                      @"|\d{4}-\d{2}-\d{2}" +           // date
                      @"|\d{2}:\d{2}(:\d{2})?" +        // time
                      @"|\b\w+'\w+\b" +                 // contraction
                      @"|\b\w+\b";                      // other words

        var matches = Regex.Matches(text, pattern);

        var wordsList = new List<string>();
        foreach (Match match in matches)
        {
            wordsList.Add(match.Value);
        }
        return wordsList.ToArray();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static List<string> CleanAndSplitTokenizerToList(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<string>();
        var words = CleanAndSplitTokenizer(text);
        
        return words.Length > 0 ? words.ToList() : new List<string>();
    }
    
}