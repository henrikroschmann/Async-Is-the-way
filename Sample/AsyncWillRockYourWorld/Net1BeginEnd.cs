using System.Net;
using System.Text;

namespace AsyncWillRockYourWorld;

public class SampleNet1Async
{
    public static void Execute()
    {
        string url = "https://swapi.dev/api/people/1";
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.BeginGetResponse(new AsyncCallback(RespCallback), request);        

        // This is required to prevent the application from exiting immediately
        Console.WriteLine("Press any key to continue");
        Console.ReadLine();
        Console.WriteLine(".Net Framework 1.x has delivered time to move on");
    }

    private static void RespCallback(IAsyncResult ar)
    {
        HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
        HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);

        using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
        {
            string responseText = reader.ReadToEnd();
            Console.WriteLine(responseText);
        }
    }
}