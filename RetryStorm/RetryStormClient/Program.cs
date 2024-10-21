using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace RetryStormClient
{
    class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        static async Task Main(string[] args)
        {
            for (int i = 0; i < 100; i++)
            {
                _ = MakeRequestWithRetry();
                await Task.Delay(100); // Slight delay to avoid instant storm
            }

            Console.WriteLine("Requests initiated. Press any key to exit.");
            Console.ReadKey();
        }

        private static async Task MakeRequestWithRetry()
        {
            try
            {
                var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync("https://localhost:7118/antipattern/retry-storm/fail"));
                Console.WriteLine($"Response: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request failed: {ex.Message}");
            }
        }
    }
}
