namespace KinoPrototype.Client;

public class Everything
{
    private readonly HttpClient _http;

    public Everything(HttpClient http)
    {
        _http = http;
    }
    
    public async Task<string> GetHelloMessageAsync()
    {
        var response = await _http.GetAsync("hello");
        response.EnsureSuccessStatusCode();
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        return await response.Content.ReadAsStringAsync();
    }

}