using System;
using System.IO;
using System.Linq;
using Xunit;
using MarkovChains;

namespace MarkovChains.Tests;

public class MarkovChainSqliteMultiFileTests
{
    private string GetTempDbPath() => Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".sqlite");

    [Fact]
    public void Constructor_CreatesDatabaseFile()
    {
        var dbPath = GetTempDbPath();
        try
        {
            Assert.False(File.Exists(dbPath));
            using var markov = new MarkovChainSqliteMultiFile(dbPath, 2);
            Assert.True(File.Exists(dbPath));
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }

    [Fact]
    public void TrainFromFiles_WithValidFiles_CreatesModel()
    {
        var dbPath = GetTempDbPath();
        var textFile = Path.GetTempFileName();
        var tmpDir = Path.GetDirectoryName(textFile);
        
        try
        {
            File.WriteAllText(textFile, "hello world this is a test");
            using var markov = new MarkovChainSqliteMultiFile(dbPath, 2);
            markov.TrainFromFiles(tmpDir);
            
            MarkovChainSqlite markovChain = new MarkovChainSqlite(dbPath, 2);
            var output = markovChain.Generate(maxWords:5);
            Assert.NotNull(output);
            Assert.False(string.IsNullOrWhiteSpace(output));
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
            if (File.Exists(textFile)) File.Delete(textFile);
        }
    }

    [Fact]
    public void Generate_WithTrainedModel_ReturnsText()
    {
        var dbPath = GetTempDbPath();
        var textFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(textFile, "the quick brown fox jumps over the lazy dog");
            using var markov = new MarkovChainSqliteMultiFile(dbPath, 2);
            markov.TrainFromFiles(textFile);
            
            MarkovChainSqlite markovChain = new MarkovChainSqlite(dbPath, 2);
            var output = markovChain.Generate(maxWords:5);
            Assert.NotNull(output);
            Assert.True(output.Split(' ').Length <= 10);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
            if (File.Exists(textFile)) File.Delete(textFile);
        }
    }

    [Fact]
    public void TrainFromFiles_WithMultipleFiles_CombinesData()
    {
        var dbPath = GetTempDbPath();
        var file1 = Path.GetTempFileName();
        var file2 = Path.GetTempFileName();
        
        try
        {
            File.WriteAllText(file1, "first file content");
            File.WriteAllText(file2, "second file content");
            var tmpDir = Path.GetDirectoryName(file1);
            using var markov = new MarkovChainSqliteMultiFile(dbPath, 2);
            markov.TrainFromFiles(tmpDir);
            
            MarkovChainSqlite markovChain = new MarkovChainSqlite(dbPath, 2);
            var output = markovChain.Generate(maxWords:5);
            Assert.NotNull(output);
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
            if (File.Exists(file1)) File.Delete(file1);
            if (File.Exists(file2)) File.Delete(file2);
        }
    }

    [Fact]
    public void Constructor_WithInvalidOrder_ThrowsException()
    {
        var dbPath = GetTempDbPath();
        try
        {
            Assert.Throws<ArgumentException>(() => new MarkovChainSqliteMultiFile(dbPath, 0));
        }
        finally
        {
            if (File.Exists(dbPath)) File.Delete(dbPath);
        }
    }
}