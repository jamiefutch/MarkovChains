using System.Data.SQLite;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace MarkovChains;

/// <summary>
/// Markov chain implementation using SQLite.
/// </summary>
public class MarkovChainSqlite : IDisposable
{
    private readonly int _order;
    private readonly SQLiteConnection _conn;

    /// <summary>
    /// MarkovChainSqlite constructor.
    /// </summary>
    /// <param name="dbPath">path to sqlite db</param>
    /// <param name="order">N-Gram size</param>
    /// <param name="loadIntoMemory">load db into memory?</param>
    /// <param name="cacheSize"></param>
    public MarkovChainSqlite(string dbPath, int order, bool loadIntoMemory = false, int cacheSize = 1_000_000)
    {
        _order = order;
        SQLiteConnection conn;

        if (loadIntoMemory)
        {
            // Open file DB
            using var fileConn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            fileConn.Open();

            // Open memory DB
            conn = new SQLiteConnection("Data Source=:memory:;Version=3;");
            conn.Open();

            // Copy file DB into memory DB
            fileConn.BackupDatabase(conn, "main", "main", -1, null, 0);
            _conn = conn;
        }
        else
        {
            bool create = !File.Exists(dbPath);
            conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            conn.Open();
            _conn = conn;

            using (var cacheCmd = new SQLiteCommand($"PRAGMA cache_size={cacheSize};", _conn))
                cacheCmd.ExecuteNonQuery();
            using (var walCmd = new SQLiteCommand("PRAGMA journal_mode=WAL;", _conn))
                walCmd.ExecuteNonQuery();

            if (create)
            {
                using var cmd = new SQLiteCommand(@"
                CREATE TABLE ngrams (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    gram TEXT,
                    next TEXT,
                    count INTEGER,
                    UNIQUE(gram, next)
                );", _conn);
                cmd.ExecuteNonQuery();
            }
        }
    }

    /// <summary>
    /// trains the Markov chain on a single line of text.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Train(string text)
    {
        //var words = Utilities.CleanAndSplitTokenizer(text);
        //var words = Utilities.TokenizeWithSentenceBoundaries(text).SelectMany(x => x).ToArray();
        
        // Efficiently flatten sentences into a single array using a pooled list
        var pooled = new List<string>(text.Length / 4); // rough initial capacity
        foreach (var sentence in Utilities.TokenizeWithSentenceBoundaries(text))
            pooled.AddRange(sentence);
        var words = pooled.ToArray();
        
        
        if (words.Length < _order + 1) return;
        var s = new StringBuilder();
        
        using var tx = _conn.BeginTransaction();
        using var cmd = new SQLiteCommand(@"INSERT INTO ngrams (gram, next, count) VALUES (@gram, @next, 1) ON CONFLICT(gram, next) DO UPDATE SET count = count + 1;", _conn, tx);
        
        var gramParam = cmd.Parameters.Add("@gram", System.Data.DbType.String);
        var nextParam = cmd.Parameters.Add("@next", System.Data.DbType.String);
        
        for (int i = 0; i < words.Length - _order; i++)
        {
            s.Clear();
            s.Append(words[i]);
            for (int j = 1; j < _order; j++)
            {
                s.Append(' ');
                s.Append(words[i + j]);
            }
            string gram = s.ToString();
            
            string next = words[i + _order];
            
            gramParam.Value = gram;
            nextParam.Value = next;
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
    }

    
    /// <summary>
    /// Trains the Markov chain on many lines of text.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Train(IEnumerable<string> lines)
    {
        foreach (var line in lines)
            Train(line);
    }
    
    
    /// <summary>
    /// Generates text based on the trained Markov chain.
    /// </summary>
    /// <param name="maxWords"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? Generate(int maxWords = 50)
    {
        var rnd = new Random();
        string startGram = string.Join(" ", Enumerable.Repeat("<START>", _order));

        // Check if startGram exists
        using var checkCmd = new SQLiteCommand("SELECT COUNT(*) FROM ngrams WHERE gram = @gram;", _conn);
        checkCmd.Parameters.AddWithValue("@gram", startGram);
        long exists = (long)checkCmd.ExecuteScalar();

        string currentGram = startGram;
        if (exists == 0)
        {
            // Pick a random gram from the database
            using var pickCmd = new SQLiteCommand("SELECT gram FROM ngrams ORDER BY RANDOM() LIMIT 1;", _conn);
            var picked = pickCmd.ExecuteScalar();
            if (picked == null) return string.Empty;
            currentGram = (string)picked;
        }

        var result = new List<string>(currentGram.Split(' '));
        for (int i = 0; i < maxWords - _order; i++)
        {
            using var cmd = new SQLiteCommand("SELECT next, count FROM ngrams WHERE gram = @gram;", _conn);
            cmd.Parameters.AddWithValue("@gram", currentGram);
            var nextWords = new List<(string word, int count)>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                nextWords.Add((reader.GetString(0), reader.GetInt32(1)));
            if (!nextWords.Any()) break;

            int total = nextWords.Sum(t => t.count);
            int pick = rnd.Next(total);
            int acc = 0;
            string next = nextWords[0].word;
            foreach (var (word, count) in nextWords)
            {
                acc += count;
                if (pick < acc)
                {
                    next = word;
                    break;
                }
            }
            if (next == "<END>") break;
            result.Add(next);
            currentGram = string.Join(" ", result.Skip(Math.Max(0, result.Count - _order)));
        }
        
        // kinda hacky way to skip the <START> tokens, but it works
        int skip = 0;
        while (skip < result.Count && result[skip] == "<START>") skip++;
        return string.Join(" ", result.Skip(skip));
    }
    

    /// <summary>
    /// Closes the SQLite connection.
    /// </summary>
    public void Close() => _conn.Close();
    
    /// <summary>
    /// Disposes the MarkovChainSqlite instance, closing the connection.
    /// </summary>
    public void Dispose()
    {
        _conn.Close();
        _conn.Dispose();
    }
}