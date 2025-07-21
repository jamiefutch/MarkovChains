using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace MarkovChains;

/// <summary>
/// Markov chain implementation using n-grams.
/// </summary>
public class MarkovChainNGram : IDisposable, IMarkovChain, IMarkovChainFiles
{
    private static int _chainCapacity; // = 4_000_000; // Initial capacity for the chain dictionary
    private readonly int _order;
    private Dictionary<string, List<string>>? _chain = new Dictionary<string, List<string>>(_chainCapacity);
    private readonly Random _random = new Random();
    private readonly string _terminator = "<END>";

    /// <summary>
    /// markovChainNGram constructor.
    /// </summary>
    public MarkovChainNGram(int order, int chainCapacity)
    {
        if (order < 1)
            throw new ArgumentException("Order must be at least 1.");
        _order = order;
        
        if (chainCapacity < 1)
            throw new ArgumentException("Chain capacity must be at least 1.");
        _chainCapacity = chainCapacity;
    }

    /// <summary>
    /// Builds the Markov chain from input text (split by whitespace). 
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Train(string text)
    {
        var words = Utilities.CleanAndSplitToList(text);
        words.Add(_terminator); // End token

        StringBuilder s = new StringBuilder();
        for (int i = 0; i < words.Count - _order; i++)
        {
            s.Clear();
            s.Append(words[i]);
            for (int j = 1; j < _order; j++)
            {
                s.Append(' ');
                s.Append(words[i + j]);
            }
            // Build n-gram key
            var key = s.ToString();
            var next = words[i + _order];

            if (_chain != null && !_chain.ContainsKey(key))
                _chain[key] = new List<string>();

            if (_chain != null) _chain[key].Add(next);
        }
    }
    
    /// <summary>
    /// Trains the Markov chain on many lines of text.
    /// </summary>
    public void Train(IEnumerable<string> lines)
    {
        foreach (var line in lines)
            Train(line);
    }

    /// <summary>
    /// Generates text starting from a given n-gram (or random if null) and up to maxWords.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string Generate(string? start = null, int maxWords = 100)
    {
        if (_chain != null && _chain.Count == 0)
            throw new InvalidOperationException("The Markov chain is empty. Train it first.");

        if (_chain != null)
        {
            string current = start ?? _chain.Keys.ElementAt(_random.Next(_chain.Count));
            var result = new List<string>(current.Split(' '));

            for (int i = 0; i < maxWords - _order; i++)
            {
                if (!_chain.ContainsKey(current) || _chain[current].Count == 0)
                    break;

                string next = _chain[current][_random.Next(_chain[current].Count)];
                if (next == _terminator)
                    break;

                result.Add(next);

                // Build next n-gram key
                current = string.Join(" ", result.Skip(result.Count - _order).Take(_order));
            }

            return string.Join(" ", result);
        }
        throw new InvalidOperationException("The Markov chain is not initialized.");
    }

    /// <summary>
    /// Saves the Markov chain to a file in JSON format.
    /// </summary>
    public void SaveToFile(string filePath)
    {
        File.WriteAllText(filePath, JsonSerializer.Serialize(_chain));
    }
    
    /// <summary>
    /// Loads the Markov chain from a file in JSON format.
    /// </summary>
    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        _chain = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(
            File.ReadAllText(filePath));
    }
    
    /// <summary>
    /// Trims the internal dictionary to remove excess capacity.
    /// </summary>
    public void TrimChain()
    {
        _chain?.TrimExcess();
    }

    /// <summary>
    /// Disposes of the Markov chain resources.
    /// </summary>
    public void Dispose()
    {
        _chain?.Clear();
        _chain = null;
    }
}