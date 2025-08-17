using System;
using System.Runtime.CompilerServices;

namespace MarkovChains;

public class MarkovChainSqliteMultiFile : IDisposable
{
    private const string StatusFileName = "training_status";
    
    private readonly MarkovChainSqlite _markovChain;
    private string? _inputPath;
    private string _dbPath;
    private bool _loadIntoMemory;
    private int _cacheSize;
    private int _order;
    private readonly string _searchPattern;
    private string? _statusFilePath;
    private readonly object _dbLock = new object();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public MarkovChainSqliteMultiFile(string dbPath
        , int order
        , string searchPattern = "*.txt"
        , bool loadIntoMemory = false
        , int cacheSize = 1_000_000)
    {
        //_inputPath = inputPath;
         _dbPath = dbPath;
         _loadIntoMemory = loadIntoMemory;
        _cacheSize = cacheSize;
        _order = order;
        _searchPattern = searchPattern;
        _markovChain = new MarkovChainSqlite(dbPath, order, loadIntoMemory, cacheSize);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TrainFromFiles(string? inputPath, 
        string searchPattern = "*.txt", 
        string? statusFilePath = "",
        bool showFileBeingProcessed = false)
    {
        _inputPath = inputPath;
        if (_inputPath != null)
        {
            var lastFilePath = string.Empty;
            _statusFilePath = Path.Combine(AppContext.BaseDirectory, StatusFileName);
            
            if(statusFilePath != null)
                lastFilePath = LoadStatusFile(statusFilePath);

            var resumeTraining = lastFilePath.Length != 0;
            
            var files = Directory.GetFiles(_inputPath, searchPattern);
            foreach (var file in files)
            {
                if (!resumeTraining && file != lastFilePath)
                {
                    // Skip files until we reach the last processed file
                    if (showFileBeingProcessed)
                    {
                        Console.WriteLine($"{DateTime.Now}\tSkipped file: {file}");
                    }    
                    continue;
                }
                
                if (showFileBeingProcessed)
                {
                    Console.WriteLine($"{DateTime.Now}\tProcessing file: {file}");
                }
                resumeTraining = true;
                SaveStatusFile(file);
                var lines = File.ReadAllLines(file);
                _markovChain.Train(lines);
            }
        }

        SaveStatusFile($"{DateTime.Now}\tTraining complete.");
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ParallelTrainFromFiles(string? inputPath, 
        string searchPattern = "*.txt",
        bool showFileBeingProcessed = false)
    {
        _inputPath = inputPath;
        if (_inputPath != null)
        {
            var files = Directory.GetFiles(_inputPath, searchPattern);
            Parallel.ForEach(files, file =>
            {
                if (showFileBeingProcessed)
                {
                    Console.WriteLine($"{DateTime.Now}\tProcessing file: {file}");
                }
                var lines = File.ReadAllLines(file);
                using MarkovChainSqlite mkv = new MarkovChainSqlite(_dbPath, 
                    _order, 
                    _loadIntoMemory, 
                    _cacheSize);
                
                lock (_dbLock)
                {
                    mkv.Train(lines);
                }
            });
        }

        SaveStatusFile($"{DateTime.Now}\tTraining complete.");
    }
    
    private void SaveStatusFile(string statusText)
    {
        if(File.Exists(_statusFilePath))
            File.Delete(_statusFilePath);
        if (_statusFilePath != null) File.WriteAllText(_statusFilePath, statusText);
    }
    
    private string LoadStatusFile(string? filePath)
    {
        var statusText = string.Empty;
        if (File.Exists(_statusFilePath))
        {
            return File.ReadAllText(_statusFilePath);
        }
        return statusText;    
    }


    public void Dispose()
    {
        _markovChain.Dispose();
    }
}