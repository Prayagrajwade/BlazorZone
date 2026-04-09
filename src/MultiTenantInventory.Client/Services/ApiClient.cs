using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using MultiTenantInventory.Application.DTOs;

namespace MultiTenantInventory.Client.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly AuthStateService _auth;

    public ApiClient(HttpClient http, AuthStateService auth)
    {
        _http = http;
        _auth = auth;
    }

    private void SetAuthHeader()
    {
        var token = _auth.Token;
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        else
            _http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<ApiResponse<T>?> GetAsync<T>(string url)
    {
        SetAuthHeader();
        try
        {
            var response = await _http.GetAsync(url);
            return await ParseResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<T>?> PostAsync<T>(string url, object data)
    {
        SetAuthHeader();
        try
        {
            var response = await _http.PostAsJsonAsync(url, data);
            return await ParseResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<T>?> PutAsync<T>(string url, object data)
    {
        SetAuthHeader();
        try
        {
            var response = await _http.PutAsJsonAsync(url, data);
            return await ParseResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<T>?> DeleteAsync<T>(string url)
    {
        SetAuthHeader();
        try
        {
            var response = await _http.DeleteAsync(url);
            return await ParseResponse<T>(response);
        }
        catch (Exception ex)
        {
            return ApiResponse<T>.Fail(ex.Message);
        }
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static async Task<ApiResponse<T>?> ParseResponse<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(json))
            return ApiResponse<T>.Fail($"HTTP {(int)response.StatusCode}");

        try
        {
            return JsonSerializer.Deserialize<ApiResponse<T>>(json, _jsonOptions);
        }
        catch
        {
            // Try to parse as AuthResponseDto for login
            try
            {
                var auth = JsonSerializer.Deserialize<AuthResponseDto>(json, _jsonOptions);
                if (auth != null)
                {
                    // Wrap in ApiResponse
                    if (typeof(T) == typeof(AuthResponseDto))
                        return new ApiResponse<T> { Success = auth.Success, Message = auth.Message, Data = (T)(object)auth };
                }
            }
            catch { }

            return ApiResponse<T>.Fail($"Unexpected response: {json[..Math.Min(200, json.Length)]}");
        }
    }
}
