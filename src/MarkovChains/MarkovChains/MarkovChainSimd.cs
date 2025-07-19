using System.Text.Json;
using System.Text.RegularExpressions;

namespace MarkovChains;

/// <summary>
/// Simd-based Markov chain implementation.
/// </summary>
public class MarkovChainSimd : IDisposable, IMarkovChain, IMarkovChainFiles
{
    private static int _chainCapacity; // Initial capacity for the chain dictionary
    private readonly int _order;
    private Dictionary<string, List<string>>? _chain = new Dictionary<string, List<string>>(_chainCapacity);
    private readonly Random _random = new Random();

    /// <summary>
    /// markovChainSimd constructor.
    /// </summary>
    public MarkovChainSimd(int order, int chainCapacity)
    {
        if (order < 1) throw new ArgumentException("Order must be >= 1");
        _order = order;
        
        if (chainCapacity < 1)
            throw new ArgumentException("Chain capacity must be at least 1.");
        
        _chainCapacity = chainCapacity;
    }

    /// <summary>
    /// Trains the Markov chain on a single line of text.
    /// </summary>
    public void Train(string text)
    {
        // Remove punctuation/symbols, lower case, and split
        string cleaned = Regex.Replace(text, @"[^\w\s]", "");
        string?[] words = cleaned.Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < _order)
            return;
        for (int i = 0; i <= words.Length - _order; i++)
        {
            string key = string.Join(" ", words.Skip(i).Take(_order));
            string? next = (i + _order < words.Length) ? words[i + _order] : null;
            
            if (_chain != null && !_chain.ContainsKey(key))
                _chain[key] = new List<string>();
            
            if (next != null)
                _chain?[key].Add(next);
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
    /// Generates text using the trained Markov chain.
    /// </summary>
    public string Generate(string? start = null, int maxWords = 50)
    {
        if (_chain != null && _chain.Count == 0)
            throw new InvalidOperationException("The Markov chain is empty. Train it first.");

        string current;
        if (!string.IsNullOrWhiteSpace(start))
        {
            var startClean = Regex.Replace(start, @"[^\w\s]", "").ToLower();
            current = _chain?.Keys.FirstOrDefault(k => k.StartsWith(startClean));
            if (current == null)
                current = _chain?.Keys.ElementAt(_random.Next(_chain.Count));
        }
        else
        {
            current = _chain?.Keys.ElementAt(_random.Next(_chain.Count));
        }

        var result = new List<string>(current?.Split(' '));
        for (int i = 0; i < maxWords - _order; i++)
        {
            if (_chain != null && (!_chain.ContainsKey(current) || _chain[current].Count == 0))
                break;
            var next = _chain[current][_random.Next(_chain[current].Count)];
            result.Add(next);
            var parts = result.Skip(result.Count - _order).ToArray();
            current = string.Join(" ", parts);
        }
        return string.Join(" ", result);
    }

    /// <summary>
    /// Saves the trained Markov chain to a file in JSON format.
    /// </summary>
    public void SaveToFile(string filePath)
    {
        File.WriteAllText(filePath, JsonSerializer.Serialize(_chain));
    }

    /// <summary>
    /// Loads a Markov chain from a file.
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
    /// Saves the trained Markov chain to a file as JSON.
    /// </summary>
    public void Save(string filePath)
    {
        var json = JsonSerializer.Serialize(_chain);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Loads a Markov chain from a JSON file.
    /// </summary>
    public void LoadChain(string filePath)
    {
        var json = File.ReadAllText(filePath);
        _chain = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
    }

    /// <summary>
    /// Disposes of the Markov chain, clearing the internal dictionary.
    /// </summary>
    public void Dispose()
    {
        _chain?.Clear();
        _chain = null;
    }
}