
namespace AsyncWillRockYourWorld;

public class SampleNet5
{
    public async Task ExecuteAsync()
    {
        string url = "https://swapi.dev/api/people/1";
        var responseText = await DownloadStringAsync(url);
        Console.WriteLine(responseText);
        Console.WriteLine(".Net 5 has delivered time to move on");
    }

    private static async Task<string> DownloadStringAsync(string url)
    {
        using (HttpClient client = new HttpClient())    
        {
            return await client.GetStringAsync(url);
        }
    }
}