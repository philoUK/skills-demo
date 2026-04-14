using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace AdminModule.Keycloak;

internal class KeycloakAdminClient : IKeycloakAdminClient
{
    private readonly HttpClient _httpClient;
    private readonly string _adminBaseUrl;
    private readonly string _tokenEndpoint;
    private readonly string _clientId;
    private readonly string _clientSecret;

    public KeycloakAdminClient(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        var authority = configuration["Keycloak:Authority"]!;
        var parts = authority.Split("/realms/", 2);
        var keycloakBase = parts[0];
        var realm = parts[1];

        _adminBaseUrl = $"{keycloakBase}/admin/realms/{realm}";
        _tokenEndpoint = $"{authority}/protocol/openid-connect/token";
        _clientId = configuration["Keycloak:AdminClientId"]!;
        _clientSecret = configuration["Keycloak:AdminClientSecret"]!;
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        var formData = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("client_id", _clientId),
            new KeyValuePair<string, string>("client_secret", _clientSecret),
        ]);

        var response = await _httpClient.PostAsync(_tokenEndpoint, formData, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }

    public async Task<string> CreateUserAsync(string email, string firstName, string lastName, CancellationToken ct)
    {
        var token = await GetAccessTokenAsync(ct);

        var user = new
        {
            username = email,
            email = email,
            firstName = firstName,
            lastName = lastName,
            enabled = true,
            emailVerified = true,
            requiredActions = new[] { "UPDATE_PASSWORD" },
        };

        var request = new HttpRequestMessage(HttpMethod.Post, $"{_adminBaseUrl}/users")
        {
            Content = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location!.ToString();
        return location.Split('/').Last();
    }

    public async Task AssignRealmRoleAsync(string userId, string roleName, CancellationToken ct)
    {
        var token = await GetAccessTokenAsync(ct);

        var getRoleRequest = new HttpRequestMessage(HttpMethod.Get, $"{_adminBaseUrl}/roles/{roleName}");
        getRoleRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var getRoleResponse = await _httpClient.SendAsync(getRoleRequest, ct);
        getRoleResponse.EnsureSuccessStatusCode();

        var roleJson = await getRoleResponse.Content.ReadAsStringAsync(ct);
        var roleDoc = JsonDocument.Parse(roleJson);
        var roleId = roleDoc.RootElement.GetProperty("id").GetString()!;

        var roles = new[] { new { id = roleId, name = roleName } };
        var assignRequest = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_adminBaseUrl}/users/{userId}/role-mappings/realm"
        )
        {
            Content = new StringContent(JsonSerializer.Serialize(roles), Encoding.UTF8, "application/json"),
        };
        assignRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var assignResponse = await _httpClient.SendAsync(assignRequest, ct);
        assignResponse.EnsureSuccessStatusCode();
    }

    public async Task SendUpdatePasswordEmailAsync(string userId, string redirectUri, CancellationToken ct)
    {
        var token = await GetAccessTokenAsync(ct);

        var actions = new[] { "UPDATE_PASSWORD" };
        var url =
            $"{_adminBaseUrl}/users/{userId}/execute-actions-email"
            + $"?client_id=adminweb&redirect_uri={Uri.EscapeDataString(redirectUri)}&lifespan=86400";

        var request = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(actions), Encoding.UTF8, "application/json"),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _httpClient.SendAsync(request, ct);
        response.EnsureSuccessStatusCode();
    }
}
