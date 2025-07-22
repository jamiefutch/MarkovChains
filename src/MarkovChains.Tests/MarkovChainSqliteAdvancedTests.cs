using System.Diagnostics;
using Xunit;
using MarkovChains;
using System.IO;

namespace MarkovChains.Tests;

public class MarkovChainSqliteAdvancedTests
{
    private string GetTempDbPath() => Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".sqlite");

    [Fact]
    public void Constructor_CreatesDatabaseFile()
    {
        var dbPath = GetTempDbPath();
        try
        {
            Assert.False(File.Exists(dbPath));
            using var markov = new MarkovChainSqlite(dbPath, 2);
            Assert.True(File.Exists(dbPath));
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public void Train_ShortInput_ThrowsOnGenerate()
    {
        var dbPath = GetTempDbPath();
        try
        {
            using var markov = new MarkovChainSqlite(dbPath, 3);
            markov.Train("short words rock");
            var ex = Assert.Throws<InvalidOperationException>(() => markov.Generate(maxWords: 5));
            Assert.Equal("The Markov chain is empty.", ex.Message);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }
    
    [Fact]
    public void Train_And_Generate_WithParagraphInput_ProducesExpectedOutput()
    {
        var paragraph = "Markov chains are mathematical systems that hop from one state to another. " +
                        "They are used in a variety of fields, from physics to finance, and are especially popular in text generation. " +
                        "By analyzing the probability of word sequences, Markov chains can generate new sentences that resemble the original input.";
        
        var dbPath = GetTempDbPath();
        try
        {
            using var markov = new MarkovChainSqlite(dbPath, 2);
            markov.Train(paragraph);

            string? output = null;
            int attempts = 0;
            // Try up to 500 (sigh) times to get a valid output
            while (attempts < 500)
            {
                output = markov.Generate(maxWords: 30);
                if (!string.IsNullOrWhiteSpace(output) &&
                    output.Split(' ').Length <= 30 &&
                    output.Contains("markov") &&
                    output.Contains("chains") &&
                    output.Contains("generate"))
                {
                    break;
                }
                attempts++;
            }
            Assert.False(string.IsNullOrWhiteSpace(output));
            Assert.False(string.IsNullOrWhiteSpace(output));
            Assert.True(output.Split(' ').Length <= 30);
            Assert.Contains("markov", output);
            Assert.Contains("chains", output);
            Assert.Contains("generate", output);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }
    

    [Fact]
    public void Generate_WithStartGram_RespectsStart()
    {
        var dbPath = GetTempDbPath();
        try
        {
            using var markov = new MarkovChainSqlite(dbPath, 2);
            markov.Train("alpha beta gamma delta");
            var output = markov.Generate(start: "alpha beta", maxWords: 5);
            Assert.StartsWith("alpha beta", output);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public void Close_DisposesConnection()
    {
        var dbPath = GetTempDbPath();
        using var markov = new MarkovChainSqlite(dbPath, 2);
        markov.Close();
        // After Close, further commands should throw
        Assert.ThrowsAny<Exception>(() => markov.Generate());
        if (File.Exists(dbPath)) File.Delete(dbPath);
    }
}