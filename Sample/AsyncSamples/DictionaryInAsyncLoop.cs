namespace AsyncSamples;

public class DictionaryExample
{
    private static Dictionary<int, string> dict = new Dictionary<int, string>();

    public static void Execute()
    {
        var tasks = new List<Task>();

        for (int i = 0; i < 10; i++)
        {
            int localI = i;
            tasks.Add(Task.Run(() => AddItem(localI)));
        }

        Task.WhenAll(tasks).Wait();

        foreach (var kvp in dict)
        {
            Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }
    }

    private static void AddItem(int i)
    {
        lock (dict)
        {
            dict[i] = $"Value {i}";
        }
    }
}
