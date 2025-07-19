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
    public void Train_ShortInput_DoesNotThrow()
    {
        var dbPath = GetTempDbPath();
        try
        {
            using var markov = new MarkovChainSqlite(dbPath, 3);
            markov.Train("short words rock");
            // Should not throw or insert anything
            var output = markov.Generate(maxWords: 5);
            Assert.False(string.IsNullOrWhiteSpace(output));
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