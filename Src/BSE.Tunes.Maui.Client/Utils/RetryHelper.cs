namespace BSE.Tunes.Maui.Client.Utils
{
    public static class RetryHelper
    {
        public static async Task RetryAsync(Func<Task> operation, int maxAttempts = 3, int delayMilliseconds = 1000)
        {
            int attempt = 0;
            while (attempt < maxAttempts)
            {
                try
                {
                    await operation();
                    return;
                }
                catch (Exception ex)
                {
                    attempt++;
                    Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");

                    if (attempt >= maxAttempts)
                        throw;

                    await Task.Delay(delayMilliseconds);
                }
            }
        }
    }
}
