public static class HttpResponseExtensions
{
    public static async Task EnsureSuccessWithDetailsAsync(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"{response.StatusCode}: {content}");
        }
    }
}