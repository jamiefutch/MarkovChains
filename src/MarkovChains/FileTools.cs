namespace MarkovChains;

/// <summary>
/// Utility class for file operations.
/// </summary>
public static class FileTools
{
    /// <summary>
    /// Gets the size of a file in bytes.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public static long GetFileSize(string  filePath)  
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified file does not exist.", filePath);
        }

        FileInfo fileInfo = new FileInfo(filePath);
        return fileInfo.Length;
    }
    
    /// <summary>
    /// Counts the number of lines in a file.
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public static long GetLinesInFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified file does not exist.", filePath);
        }

        long lineCount = 0;
        using StreamReader reader = new StreamReader(filePath);
        while (reader.ReadLine() != null)
        {
            lineCount++;
        }

        return lineCount;
    }
}