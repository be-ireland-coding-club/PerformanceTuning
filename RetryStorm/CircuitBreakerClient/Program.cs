using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;

namespace CircuitBreakerClient
{
    class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private static readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()            
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 1, // Number of consecutive failures before breaking the circuit
                durationOfBreak: TimeSpan.FromSeconds(5) // Duration to keep the circuit open before retrying
            );

        static async Task Main(string[] args)
        {
            for (int i = 0; i < 100; i++)
            {
                _ = MakeRequestWithCircuitBreaker();
                await Task.Delay(100); // Slight delay to avoid instant storm
            }

            Console.WriteLine("Requests initiated. Press any key to exit.");
            Console.ReadKey();
        }

        private static async Task MakeRequestWithCircuitBreaker()
        {
            try
            {
                var response = await _circuitBreakerPolicy.ExecuteAsync(() => _httpClient.GetAsync("https://localhost:7118/antipattern/retry-storm/fail"));
                Console.WriteLine($"Response: {response.StatusCode}");
            }
            catch (BrokenCircuitException)
            {
                Console.WriteLine("Circuit breaker is open. Skipping request.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Request failed: {ex.Message}");
            }
        }
    }
}
