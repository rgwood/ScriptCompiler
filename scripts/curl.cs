Console.WriteLine("Getting example.com...");
var client = new HttpClient();
var response = await client.GetStringAsync("https://example.com");
Console.WriteLine(response);
