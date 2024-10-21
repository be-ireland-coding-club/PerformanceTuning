using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    private static async Task Main(string[] args)
    {
        //string url = "https://localhost:7118/antipattern/busy-database/get-xml?territoryId=1";
        string url = "https://localhost:7118/optimised/busy-database/get-xml?territoryId=1";
        int concurrentRequests = 50;

        // Create an HttpClient instance
        using HttpClient client = new HttpClient();

        // Create an array to hold the tasks
        Task<HttpResponseMessage>[] tasks = new Task<HttpResponseMessage>[concurrentRequests];

        // Start the concurrent requests
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks[i] = client.GetAsync(url);
        }

        // Wait for all tasks to complete
        HttpResponseMessage[] responses = await Task.WhenAll(tasks);

        // Output the status code of each response
        foreach (HttpResponseMessage response in responses)
        {
            Console.WriteLine($"Status Code: {response.StatusCode}");
        }
    }
}
