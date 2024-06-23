namespace AsyncSamples;

public class AsyncInSync2
{
    public void SynchronousMethod() 
    {
       var result = TaskAsync.GetAwaiter().GetResult();
    }
    public async Task<string> TaskAsync()
    {
        // Avoid capturing the synchronization context
        await Task.Delay(1000).ConfigureAwait(false);
        return "Completed";
    }
}