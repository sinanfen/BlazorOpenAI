using System.Net.Http.Headers;
using System.Text.Json;

public class OpenAIService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public OpenAIService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenAI:ApiKey"];
        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> GenerateLogoAsync(string prompt)
    {
        try
        {
            var requestBody = new
            {
                prompt = prompt,
                n = 1,
                size = "1024x1024"
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("images/generations", content);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Error: {response.StatusCode}, Content: {responseBody}");
            }

            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseBody);

            return responseObject.GetProperty("data")[0].GetProperty("url").GetString();
        }
        catch (Exception ex)
        {
            // Hata mesajýný loglayýn
            Console.WriteLine($"Error generating logo: {ex.Message}");

            // Kullanýcýya dost bir hata mesajý döndürün
            if (ex.Message.Contains("billing_hard_limit_reached"))
            {
                return "Üzgünüz, þu anda hizmet kullanýlamýyor. Lütfen daha sonra tekrar deneyin.";
            }

            // Diðer hata durumlarý için...
            return "Bir hata oluþtu. Lütfen daha sonra tekrar deneyin.";
        }
    }
}