using Xunit;
using MarkovChains;

namespace MarkovChains.Tests;

public class MarkovChainSimdTests
{
    [Fact]
    public void Train_And_Generate_ShouldReturnExpectedText()
    {
        var markov = new MarkovChainSimd(order: 2, chainCapacity: 100);
        markov.Train("the quick brown fox jumps over the lazy dog");
        var output = markov.Generate(maxWords: 10);
        Assert.False(string.IsNullOrWhiteSpace(output));
    }

    [Fact]
    public void Generate_WithoutTraining_ThrowsException()
    {
        var markov = new MarkovChainSimd(order: 2, chainCapacity: 100);
        Assert.Throws<InvalidOperationException>(() => markov.Generate());
    }
    
    [Fact]
    public void Train_And_Generate_WithParagraphInput_ProducesExpectedOutput()
    {
        var paragraph = "Markov chains are mathematical systems that hop from one state to another. " +
                        "They are used in a variety of fields, from physics to finance, and are especially popular in text generation. " +
                        "By analyzing the probability of word sequences, Markov chains can generate new sentences that resemble the original input.";
        
        try
        {
            var markov = new MarkovChainNGram(order: 2, chainCapacity: 100);
            markov.Train(paragraph);

            string output = null;
            int attempts = 0;
            // Try up to 5 times to get a valid output
            while (attempts < 5)
            {
                output = markov.Generate(maxWords: 30);
                if (!string.IsNullOrWhiteSpace(output) &&
                    output.Split(' ').Length <= 30 &&
                    output.Contains("Markov") &&
                    output.Contains("chains") &&
                    output.Contains("generate"))
                {
                    break;
                }
                attempts++;
            }

            Assert.False(string.IsNullOrWhiteSpace(output));
            Assert.True(output.Split(' ').Length <= 30);
            Assert.Contains("Markov", output);
            Assert.Contains("chains", output);
            Assert.Contains("generate", output);
        }
        catch (Exception ex)
        {
            Assert.Fail($"Training and generation failed with exception: {ex.Message}");
        }
    }


    [Fact]
    public void SaveAndLoad_PreservesChain()
    {
        var markov = new MarkovChainSimd(order: 2, chainCapacity: 100);
        markov.Train("hello world hello universe");
        var path = "testchainsimd.json";
        markov.SaveToFile(path);

        var loaded = new MarkovChainSimd(order: 2, chainCapacity: 100);
        loaded.LoadFromFile(path);
        var output = loaded.Generate(maxWords: 5);
        Assert.False(string.IsNullOrWhiteSpace(output));
        File.Delete(path);
    }
}