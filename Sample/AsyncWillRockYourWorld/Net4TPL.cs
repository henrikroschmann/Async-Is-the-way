
using System.Net;

namespace AsyncWillRockYourWorld;

public class SampleNet4Async
{
    public static void Execute()
    {
        string url = "https://swapi.dev/api/people/1";
        Task<string> task = DownloadStringAsync(url);
        task.ContinueWith(t => Console.WriteLine(t.Result));

        // This is required to prevent the application from exiting immediately
        Console.WriteLine("Press any key to continue");
        Console.ReadLine();
        Console.WriteLine(".Net Framework 4 has delivered time to move on");
    }

    private static async Task<string> DownloadStringAsync(string url)
    {
        using (WebClient client = new WebClient())
        {            
            return await client.DownloadStringTaskAsync(new Uri(url));
        }
    }
}