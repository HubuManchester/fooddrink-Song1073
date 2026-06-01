namespace FoodDrinkApp.Services;

public static class SpeechService
{
    private static CancellationTokenSource? _cts;

    public static async Task SpeakTextAsync(string text)
    {
        Stop();
        _cts = new CancellationTokenSource();
        var options = new SpeechOptions { Volume = 0.9f, Pitch = 1.0f };

        try
        {
            await TextToSpeech.Default.SpeakAsync(text, options, cancelToken: _cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"TTS Error: {ex.Message}");
        }
    }

    public static void Stop()
    {
        if (_cts?.IsCancellationRequested == false)
        {
            _cts.Cancel();
        }
    }
}