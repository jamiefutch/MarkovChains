namespace MarkovChains;

public static class ExtensionMethods
{
    public static void Print(this string txt)
    {
        Console.WriteLine($"{DateTime.Now}\t{txt}");
    }
}