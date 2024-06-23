namespace AsyncInSync;

public class ConcurrentBagExample
{
    private static ConcurrentBag<int> bag = new ConcurrentBag<int>();

    public static void Main()
    {
        var tasks = new List<Task>();

        for (int i = 0; i < 10; i++)
        {
            int localI = i;
            tasks.Add(Task.Run(() => AddItem(localI)));
        }

        Task.WhenAll(tasks).Wait();

        foreach (var item in bag)
        {
            Console.WriteLine(item);
        }
    }

    private static void AddItem(int i)
    {
        bag.Add(i);
    }
}
