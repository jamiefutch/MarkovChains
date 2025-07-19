namespace MarkovChains;

public interface IMarkovChainFiles
{
    void SaveToFile(string filePath);
    void LoadFromFile(string filePath);
    void TrimChain();
}