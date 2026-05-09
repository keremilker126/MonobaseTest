using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;

public class NotesController : Controller
{
    private readonly string _apiKey = "7f7d473ac140452baec21aa1fa31f35e";
    private readonly string _apiUrl = "http://localhost:5242/api/data/Notlar";
    private readonly HttpClient _httpClient = new HttpClient();

    public async Task<IActionResult> Index()
    {
        var response = await _httpClient.GetAsync($"{_apiUrl}?apiKey={_apiKey}");
        if (!response.IsSuccessStatusCode) return View(new List<Notes>());

        var jsonString = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(jsonString);
        var notes = doc.RootElement.EnumerateArray().Select(x => new Notes
        {
            Id = x.GetProperty("id").GetString(),
            Title = x.GetProperty("data").GetProperty("Title").GetString(),
            Content = x.GetProperty("data").GetProperty("Content").GetString()
        }).ToList();

        return View(notes);
    }

    [HttpPost]
    public async Task<IActionResult> Create(string title, string content)
    {
        var newNote = new Notes { Title = title, Content = content };
        var json = JsonSerializer.Serialize(newNote);
        var body = new StringContent(json, Encoding.UTF8, "application/json");

        await _httpClient.PostAsync($"{_apiUrl}?apiKey={_apiKey}", body);
        return RedirectToAction("Index");
    }
    [HttpPost]
    public async Task<IActionResult> Delete(string id) // Buradaki 'id' ismi formdaki name="id" ile aynı olmalı
    {
        if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

        // DİKKAT: API'ye DELETE isteği gönderiyoruz. 
        // URL formatı: http://localhost:5242/api/data/Notlar/ID_DEGERI?apiKey=...
        var requestUrl = $"{_apiUrl}/{id}?apiKey={_apiKey}";

        try
        {
            var response = await _httpClient.DeleteAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                // Hata durumunu loglayabilir veya view tarafına hata basabilirsin
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Silme Hatası: {error}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Bağlantı Hatası: {ex.Message}");
        }

        return RedirectToAction("Index");
    }
    [HttpPost]
    public async Task<IActionResult> Update(string id, string title, string content)
    {
        if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");

        var updatedData = new { Title = title, Content = content };
        var json = JsonSerializer.Serialize(updatedData);
        var body = new StringContent(json, Encoding.UTF8, "application/json");

        // MonoBase API'ye PUT (Güncelleme) isteği atıyoruz
        var requestUrl = $"{_apiUrl}/{id}?apiKey={_apiKey}";
        var response = await _httpClient.PutAsync(requestUrl, body);

        return RedirectToAction("Index");
    }
}