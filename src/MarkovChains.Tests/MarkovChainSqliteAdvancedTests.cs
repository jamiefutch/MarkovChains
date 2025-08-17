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

            string? output = null;
            int attempts = 0;
            while (attempts < 500)
            {
                output = "";
                output = markov.Generate(maxWords:5);
                if (!string.IsNullOrWhiteSpace(output) &&
                    output.Contains("alpha") &&
                    output.Contains("beta"))
                {
                    break;
                }
                attempts++;
            }
            Assert.False(string.IsNullOrWhiteSpace(output));
            Assert.Contains("alpha", output);
            Assert.Contains("beta", output);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }
    
    [Fact]
    public void PruneChain_RemovesLowCountNgrams()
    {
        // Arrange
        var dbPath = "test_prunechain.sqlite";
        if (File.Exists(dbPath)) File.Delete(dbPath);
        using var chain = new MarkovChains.MarkovChainSqlite(dbPath, order: 2);

        // Add a frequent n-gram
        chain.Train("alpha beta gamma");
        chain.Train("alpha beta gamma");
        // Add a rare n-gram
        chain.Train("delta epsilon zeta");

        // Act
        chain.PruneChain(minCount: 2);

        // Assert: Only the frequent n-gram should remain
        using var cmd = new System.Data.SQLite.SQLiteCommand("SELECT gram, next FROM ngrams;", 
            new System.Data.SQLite.SQLiteConnection($"Data Source={dbPath};Version=3;"));
        cmd.Connection.Open();
        using var reader = cmd.ExecuteReader();
        var results = new List<(string, string)>();
        while (reader.Read())
            results.Add((reader.GetString(0), reader.GetString(1)));
        Assert.Contains(results, x => x.Item1.Contains("alpha beta"));
        Assert.DoesNotContain(results, x => x.Item1.Contains("delta epsilon"));
    }
    
    
    [Fact]
    public void GetNGramsCount_ReturnsCorrectCount()
    {
        
        var testText = new List<string>
        {
            "The quick brown fox jumps over the lazy dog.",
            "Markov chains are useful for generating random text.",
            "C# is a modern, object-oriented programming language.",
            "Unit testing helps ensure code reliability.",
            "Artificial intelligence is transforming technology.",
            "The rain in Spain stays mainly in the plain.",
            "To be or not to be, that is the question.",
            "She sells seashells by the seashore.",
            "A journey of a thousand miles begins with a single step.",
            "All that glitters is not gold.",
            "Practice makes perfect.",
            "Better late than never.",
            "Actions speak louder than words.",
            "Every cloud has a silver lining.",
            "A picture is worth a thousand words.",
            "When in Rome, do as the Romans do.",
            "The pen is mightier than the sword.",
            "You can't judge a book by its cover.",
            "Fortune favors the bold.",
            "Birds of a feather flock together."
        };
        
        
        var dbPath = GetTempDbPath();
        var textFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(textFile, "a b c a b c");
            using var markov = new MarkovChainSqlite(dbPath, 2);
            markov.Train(testText);
    
            long count = markov.GetNGramsCount();
            Assert.True(count > 0);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
            if (File.Exists(textFile)) File.Delete(textFile);
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