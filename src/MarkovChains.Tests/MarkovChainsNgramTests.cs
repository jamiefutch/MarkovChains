using Xunit;
using MarkovChains;
    
namespace MarkovChains.Tests;

public class MarkovChainsNgramTests
{
    [Fact]
    public void Train_And_Generate_ShouldReturnExpectedText()
    {
        var markov = new MarkovChainNGram(order: 2, chainCapacity: 100);
        markov.Train("the quick brown fox jumps over the lazy dog");
        var output = markov.Generate(maxWords: 10);
        Assert.False(string.IsNullOrWhiteSpace(output));
    }

    [Fact]
    public void Generate_WithoutTraining_ThrowsException()
    {
        var markov = new MarkovChainNGram(order: 2, chainCapacity: 100);
        Assert.Throws<InvalidOperationException>(() => markov.Generate());
    }

    [Fact]
    public void SaveAndLoad_PreservesChain()
    {
        var markov = new MarkovChainNGram(order: 2, chainCapacity: 100);
        markov.Train("hello world hello universe");
        var path = "testchain.json";
        markov.SaveToFile(path);

        var loaded = new MarkovChainNGram(order: 2, chainCapacity: 100);
        loaded.LoadFromFile(path);
        var output = loaded.Generate(maxWords: 5);
        Assert.False(string.IsNullOrWhiteSpace(output));
        File.Delete(path);
    } 
}