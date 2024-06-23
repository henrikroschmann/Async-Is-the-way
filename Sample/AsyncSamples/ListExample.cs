namespace AsyncInSync;

public class ListExample
{
    private static List<int> list = new List<int>();

    public static void Main()
    {
        var tasks = new List<Task>();

        for (int i = 0; i < 10; i++)
        {
            int localI = i;
            tasks.Add(Task.Run(() => AddItem(localI)));
        }

        Task.WhenAll(tasks).Wait();

        foreach (var item in list)
        {
            Console.WriteLine(item);
        }
    }

    private static void AddItem(int i)
    {
        lock (list)
        {
            list.Add(i);
        }
    }
}
