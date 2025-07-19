namespace MarkovChains;

public interface IMarkovChain : IDisposable
{
    void Train(string text);
    void Train(IEnumerable<string> lines);
    string Generate(string? start = null, int maxWords = 100);
    
}