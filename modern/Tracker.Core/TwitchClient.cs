using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Tracker.Core;


public sealed class TwitchClient(HttpClient http)
{
    private string? _publicClientId;

    public async Task<ChannelSnapshot?> GetChannelAsync(string login, CancellationToken ct)
    {
        for (var attempt = 0; attempt < 2; attempt++)
        {
            if (_publicClientId is null)
            {
                var html = await http.GetStringAsync("https://www.twitch.tv/" + Uri.EscapeDataString(login), ct);
                var match = Regex.Match(html, "clientId=\"([a-z0-9]+)\"");
                if (!match.Success) throw new InvalidOperationException("Twitch изменил формат сайта. Гостевой доступ временно недоступен.");
                _publicClientId = match.Groups[1].Value;
            }
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://gql.twitch.tv/gql");
            request.Headers.Add("Client-ID", _publicClientId);
            request.Content = JsonContent.Create(new
            {
                query = "query Tracker($login: String!) { user(login: $login) { login displayName stream { id title viewersCount game { id name } } } }",
                variables = new { login }
            });
            using var response = await http.SendAsync(request, ct);
            if (attempt == 0 && response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            { _publicClientId = null; continue; }
            response.EnsureSuccessStatusCode();
            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
            var root = json.RootElement;
            if (root.TryGetProperty("errors", out _)) throw new InvalidOperationException("Twitch отклонил гостевой запрос. Повторим проверку позже.");
            var user = root.GetProperty("data").GetProperty("user");
            if (user.ValueKind == JsonValueKind.Null) return null;
            var stream = user.GetProperty("stream");
            if (stream.ValueKind == JsonValueKind.Null) return new(login, Text(user, "displayName") ?? login, false, null, null, null);
            var game = stream.GetProperty("game");
            return new(login, Text(user, "displayName") ?? login, true, Text(game, "id"), Text(game, "name"), Text(stream, "title"), stream.GetProperty("viewersCount").GetInt32());
        }
        throw new InvalidOperationException("Гостевой доступ Twitch недоступен.");
    }

    private static string? Text(JsonElement element, string key) => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;
}