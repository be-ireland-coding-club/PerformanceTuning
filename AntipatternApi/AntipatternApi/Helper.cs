namespace AntipatternApi
{
    public static class Helper
    {
        public static void DoSynchronousOperation()
        {
            // Simulate a long-running task with Thread.Sleep
            Thread.Sleep(3000); // 2 seconds delay
        }

        public static async Task DoAsynchronousOperation()
        {
            // Simulate a long-running task asynchronously with Task.Delay
            await Task.Delay(3000); // 2 seconds delay            
        }
    }
}
