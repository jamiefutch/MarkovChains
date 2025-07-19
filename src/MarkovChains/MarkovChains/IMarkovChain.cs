namespace MarkovChains;

public interface IMarkovChain : IDisposable
{
    void Train(string text);
    string Generate(string? start = null, int maxWords = 100);
    void SaveToFile(string filePath);
    void LoadFromFile(string filePath);
    void TrimChain();
}