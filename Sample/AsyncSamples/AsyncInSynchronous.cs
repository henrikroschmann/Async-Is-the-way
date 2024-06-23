namespace AsyncSamples;

public class AsyncInSync
{
    public void SynchronousMethod() 
    {
       var result = TaskAsync.GetAwaiter().GetResult();
    }
    public async Task<string> TaskAsync()
    {
        await Task.Delay(1000);
        return "Completed";
    }
}