using System.Data.SQLite;
using System.Text.RegularExpressions;

namespace MarkovChains;

public class MarkovChainSqlite : IDisposable
{
    private readonly int _order;
    private readonly SQLiteConnection _conn;

    public MarkovChainSqlite(string dbPath, int order)
    {
        _order = order;
        bool create = !File.Exists(dbPath);
        _conn = new SQLiteConnection($"Data Source={dbPath};Version=3;");
        _conn.Open();
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

    public void Train(string text)
    {
        var words = Regex.Replace(text, @"[^\w\s]", "")
                         .Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        if (words.Length < _order + 1) return;
        using var tx = _conn.BeginTransaction();
        for (int i = 0; i <= words.Length - _order; i++)
        {
            string gram = string.Join(" ", words.Skip(i).Take(_order));
            string next = (i + _order < words.Length) ? words[i + _order] : null;
            if (next == null) continue;
            using var cmd = new SQLiteCommand(@"
                INSERT INTO ngrams (gram, next, count) VALUES (@gram, @next, 1)
                ON CONFLICT(gram, next) DO UPDATE SET count = count + 1;", _conn, tx);
            cmd.Parameters.AddWithValue("@gram", gram);
            cmd.Parameters.AddWithValue("@next", next);
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
    }

    public void Train(IEnumerable<string> lines)
    {
        foreach (var line in lines)
            Train(line);
    }

    public string Generate(string start = null, int maxWords = 50)
    {
        var rnd = new Random();
        string currentGram = start;
        if (string.IsNullOrWhiteSpace(currentGram))
        {
            using var cmd = new SQLiteCommand("SELECT gram FROM ngrams ORDER BY RANDOM() LIMIT 1;", _conn);
            currentGram = (string)cmd.ExecuteScalar();
        }
        
        if (currentGram == null) throw new InvalidOperationException("The Markov chain is empty.");
        
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
            // Weighted random selection
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
            result.Add(next);
            currentGram = string.Join(" ", result.Skip(result.Count - _order));
        }
        return string.Join(" ", result);
    }

    public void Close() => _conn.Close();
    public void Dispose()
    {
        _conn.Close();
        _conn.Dispose();
    }
}