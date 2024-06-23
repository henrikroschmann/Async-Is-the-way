using System.ComponentModel;
using System.Net;

namespace AsyncWillRockYourWorld;

public class SampleNet2Async
{
    public static void Execute()
    {
        string url = "https://swapi.dev/api/people/1";
        BackgroundWorker worker = new BackgroundWorker();
        worker.DoWork += Worker_DoWork;
        worker.RunWorkerAsync(url);
        
        // This is required to prevent the application from exiting immediately
        Console.WriteLine("Press any key to continue");
        Console.ReadLine();
        Console.WriteLine(".Net Framework 2.0 has delivered time to move on");
    }

    private static void Worker_DoWork(object? sender, DoWorkEventArgs e)
    {
        string url = (string)e.Argument;
        WebClient client = new WebClient();
        string responseText = client.DownloadString(url);
        Console.WriteLine(responseText);
    }
}